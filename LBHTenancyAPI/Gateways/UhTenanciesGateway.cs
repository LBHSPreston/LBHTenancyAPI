using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using LBHTenancyAPI.Domain;

namespace LBHTenancyAPI.Gateways
{
    public class UhTenanciesGateway : ITenanciesGateway
    {
        private readonly SqlConnection conn;

        public UhTenanciesGateway(string connectionString)
        {
            conn = new SqlConnection(connectionString);
            conn.Open();
        }

        public async Task<List<TenancyListItem>> GetTenanciesByRefsAsync(List<string> tenancyRefs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT");
            sb.AppendLine("  tenagree.tag_ref as TenancyRef,");
            sb.AppendLine("  tenagree.cur_bal as CurrentBalance,");
            sb.AppendLine("  contacts.con_name as PrimaryContactName,");
            sb.AppendLine("  property.short_address as PrimaryContactShortAddress,");
            sb.AppendLine("  property.post_code as PrimaryContactPostcode,");
            sb.AppendLine("  araction.action_code AS LastActionCode,");
            sb.AppendLine("  araction.action_date AS LastActionDate,");
            sb.AppendLine("  arag.arag_status as ArrearsAgreementStatus,");
            sb.AppendLine("  arag.arag_startdate as ArrearsAgreementStartDate");
            sb.AppendLine("FROM tenagree");
            sb.AppendLine("LEFT JOIN contacts");
            sb.AppendLine("ON contacts.tag_ref = tenagree.tag_ref");
            sb.AppendLine("LEFT JOIN property");
            sb.AppendLine("ON property.prop_ref = tenagree.prop_ref");
            sb.AppendLine("LEFT JOIN(");
            sb.AppendLine("  SELECT");
            sb.AppendLine("    tag_ref,");
            sb.AppendLine("    action_code,");
            sb.AppendLine("    action_date");
            sb.AppendLine("  FROM(");
            sb.AppendLine("    SELECT");
            sb.AppendLine("      tag_ref,");
            sb.AppendLine("      action_code,");
            sb.AppendLine("      action_date,");
            sb.AppendLine("      ROW_NUMBER() OVER(PARTITION BY tag_ref ORDER BY action_date DESC) as row");
            sb.AppendLine("      FROM araction");
            sb.AppendLine("  ) t");
            sb.AppendLine("  WHERE row = 1");
            sb.AppendLine(") AS araction ON araction.tag_ref = tenagree.tag_ref");
            sb.AppendLine("LEFT JOIN(");
            sb.AppendLine("  SELECT");
            sb.AppendLine("    tag_ref,");
            sb.AppendLine("    arag_status,");
            sb.AppendLine("    arag_startdate");
            sb.AppendLine("  FROM(");
            sb.AppendLine("    SELECT");
            sb.AppendLine("      tag_ref,");
            sb.AppendLine("      arag_status,");
            sb.AppendLine("      arag_startdate,");
            sb.AppendLine("      ROW_NUMBER() OVER(PARTITION BY tag_ref ORDER BY arag_startdate DESC) AS row");
            sb.AppendLine("    FROM arag");
            sb.AppendLine("  ) t");
            sb.AppendLine("  WHERE row = 1");
            sb.AppendLine(") AS arag ON arag.tag_ref = tenagree.tag_ref");
            sb.AppendLine("WHERE tenagree.tag_ref IN @allRefs");

            var allEnum = await conn.QueryAsync<TenancyListItem>(
                sb.ToString(),new { allRefs = tenancyRefs }
            ).ConfigureAwait(false);

            var all = allEnum.ToList();

            var results = new List<TenancyListItem>();

            foreach (var reference in tenancyRefs)
            {
                try
                {
                    results.Add(all.First(e => e.TenancyRef == reference));
                }
                catch (InvalidOperationException)
                {
                    Console.Write($"No valid tenancy for ref: {reference}");
                }
            }

            return results;
        }

        public List<ArrearsActionDiaryEntry> GetActionDiaryEntriesbyTenancyRef(string tenancyRef)
        {
            return conn.Query<ArrearsActionDiaryEntry>(
                "SELECT " +
                "tag_ref as TenancyRef, " +
                "action_code as ActionCode, " +
                "action_type as Type, " +
                "action_date as ActionDate, " +
                "action_comment as ActionComment, " +
                "username as UHUsername, " +
                "action_balance as ActionBalance " +
                "FROM araction " +
                "WHERE tag_ref = @tRef " +
                "ORDER BY araction.action_date DESC",
                new { tRef = tenancyRef.Replace("%2F", "/") }
            ).ToList();
        }

        public List<PaymentTransaction> GetPaymentTransactionsByTenancyRef(string tenancyRef)
        {
            return conn.Query<PaymentTransaction>(
                "SELECT " +
                "tag_ref AS TenancyRef," +
                "prop_ref AS PropertyRef, " +
                "trans_type AS Type, " +
                "real_value AS Amount, " +
                "post_date AS Date, " +
                "trans_ref AS TransactionRef " +
                "FROM rtrans " +
                "WHERE tag_ref = @tRef " +
                "ORDER BY post_date DESC",
                new { tRef = tenancyRef.Replace("%2F", "/") }
            ).ToList();
        }

        public Tenancy GetTenancyForRef(string tenancyRef)
        {
            var result = conn.Query<Tenancy>(
                "SELECT " +
                "tenagree.tag_ref as TenancyRef, " +
                "tenagree.cur_bal as CurrentBalance, " +
                "tenagree.tenure as Tenure, " +
                "contacts.con_name as PrimaryContactName, " +
                "property.address1 as PrimaryContactLongAddress, " +
                "property.post_code as PrimaryContactPostcode, " +
                "contacts.con_phone1 as PrimaryContactPhone " +
                "FROM tenagree " +
                "LEFT JOIN arag " +
                "ON arag.tag_ref = tenagree.tag_ref " +
                "LEFT JOIN contacts " +
                "ON contacts.tag_ref = tenagree.tag_ref " +
                "LEFT JOIN property " +
                "ON property.prop_ref = tenagree.prop_ref " +
                "WHERE tenagree.tag_ref = @tRef " +
                "ORDER BY arag.arag_startdate DESC",
                new { tRef = tenancyRef.Replace("%2F", "/") }
            ).FirstOrDefault();

            result.ArrearsAgreements = GetLastFiveAgreementsForTenancy(tenancyRef);
            result.ArrearsActionDiary = GetLatestFiveArrearsActionForRef(tenancyRef);

            return result;
        }

        private List<ArrearsAgreement> GetLastFiveAgreementsForTenancy(string tenancyRef)
        {
            return conn.Query<ArrearsAgreement>(
                "SELECT TOP 5" +
                "tag_ref AS TenancyRef," +
                "arag_status AS Status, " +
                "arag_startdate Startdate, " +
                "arag_amount Amount, " +
                "arag_frequency AS Frequency, " +
                "arag_breached AS Breached, " +
                "arag_startbal AS StartBalance, " +
                "arag_clearby AS ClearBy " +
                "FROM arag " +
                "WHERE tag_ref = @tRef " +
                "ORDER BY arag_startdate DESC ",
                new { tRef = tenancyRef.Replace("%2F", "/") }
            ).ToList();
        }

        public List<ArrearsActionDiaryEntry> GetLatestFiveArrearsActionForRef(string tenancyRef)
        {
            return conn.Query<ArrearsActionDiaryEntry>(
                "SELECT top 5" +
                "tag_ref as TenancyRef, " +
                "action_code as Code, " +
                "action_type as Type, " +
                "action_date as Date, " +
                "action_comment as Comment, " +
                "username as UniversalHousingUsername, " +
                "action_balance as Balance " +
                "FROM araction " +
                "WHERE tag_ref = @tRef " +
                "ORDER BY araction.action_date DESC",
                new { tRef = tenancyRef.Replace("%2F", "/") }
            ).ToList();
        }
    }
}
