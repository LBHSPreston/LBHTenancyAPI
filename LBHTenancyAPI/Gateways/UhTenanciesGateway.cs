using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using LBHTenancyAPI.Domain;
using Dapper;

namespace LBHTenancyAPI.Gateways
{
    public class UhTenanciesGateway
    {
        public List<object> GetTenanciesByRefs(List<string> tenancyRefs)
        {
            var tenancies = new List<object>();

            for (int i = 0; i < tenancyRefs.Count; i++)
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder {["Data Source"] = "(local)"};


                if (Environment.GetEnvironmentVariable("CI_TEST") == "True")
                {
                    builder["Data Source"] = "tcp:stubuniversalhousing";
                }

                builder["integrated Security"] = false;
                builder["Initial Catalog"] = "StubUH";
                builder.UserID = "sa";
                builder.Password = "Rooty-Tooty";
                var db = new SqlConnection(builder.ConnectionString);
                db.Open();
                List<TenagreeModel> tenagrees = db.Query<TenagreeModel>($"SELECT cur_bal FROM tenagree WHERE tag_ref = '{tenancyRefs[i]}'").ToList();
                List<AractionModel> aractions = db.Query<AractionModel>($"SELECT action_code, action_date FROM araction WHERE tag_ref = '{tenancyRefs[i]}'").ToList();

                var expectedTenancy = new TenancyListItem()
                {
                    TenancyRef = tenancyRefs[i],
                    PrimaryContactName = "Test User",
                    ShortAddress = "123 Test",
                    Postcode = "E1 123",
                    Balance = tenagrees[0].cur_bal,
                    LastActionTime = aractions[0].action_date,
                    LastActionType = aractions[0].action_code,
                    CurrentAgreementStatus = "Active"
                };

                tenancies.Add(expectedTenancy);
            }

            return tenancies;
        }

        private class AractionModel
        {
            public string action_code { get; set; }
            public DateTime action_date { get; set; }
        }
        
        private class TenagreeModel
        {
            public double cur_bal { get; set; }
        }
    }
}
