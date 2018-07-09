using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using LBHTenancyAPI.Domain;
using LBHTenancyAPI.Gateways;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace LBHTenancyAPITest.Test.Gateways
{
    public class UhTenanciesGatewayTest : IClassFixture<DatabaseFixture>
    {
        private readonly SqlConnection _db;

        public UhTenanciesGatewayTest(DatabaseFixture fixture)
        {
            _db = fixture.Db;
            _db.Open();
        }

        [Fact]
        public void WhenGivenNoTenancyRefs_GetTenanciesByRefs_ShouldReturnNoTenancies()
        {
            var tenancies = GetTenanciesByRef(new List<string>() { });

            Assert.Empty(tenancies);
        }

//        [Fact]
//        public void WhenGivenASingleTenancyRef_GetTenanciesByRefs_ShouldReturnASingleResult()
//        {
//            var tenancies = GetTenanciesByRef(new List<string>() {"FAKE/01"});
//
//            Assert.Single(tenancies);
//        }

        [Fact]
        public void WhenGivenTenancyRef_GetTenanciesByRefs_ShouldReturnTenancyObjectForThatRef()
        {
            var random = new Bogus.Randomizer();
            var thing = new
            {
                tenancyRef = random.Hash(),
                currentBalance = Math.Round(random.Double(), 2),
                lastActionTime = new DateTime(random.Int(1900, 1999), random.Int(1, 12), random.Int(1, 28), 9, 30, 0),
                lastActionType = random.Hash(),
                agreementStatus = random.Word()
            };

            string commandText =
                "INSERT INTO araction (tag_ref, action_code, action_date) VALUES (@tenancyRef, @lastActionType, @lastActionTime)";
            
            SqlCommand command = new SqlCommand(commandText, _db);
            command.Parameters.Add("@tenancyRef", SqlDbType.NVarChar);
            command.Parameters["@tenancyRef"].Value = thing.tenancyRef;
            command.Parameters.Add("@lastActionType", SqlDbType.NVarChar);
            command.Parameters["@lastActionType"].Value = thing.lastActionType;
            command.Parameters.Add("@lastActionTime", SqlDbType.SmallDateTime);
            command.Parameters["@lastActionTime"].Value = thing.lastActionTime;

            command.ExecuteNonQuery();

            _db.Query($"INSERT INTO tenagree (tag_ref, cur_bal) VALUES ('{thing.tenancyRef}', {thing.currentBalance})");
            
//            _db.Query("INSERT INTO raaction (act_code, act_name) VALUES (x, x)");
            _db.Query($"INSERT INTO arag (tag_ref, arag_status) VALUES ('{thing.tenancyRef}', '{thing.agreementStatus}')");

            var tenancies = GetTenanciesByRef(new List<string>() {thing.tenancyRef});
            var expectedTenancy = new TenancyListItem()
            {
                TenancyRef = thing.tenancyRef,
                PrimaryContactName = "Test User",
                ShortAddress = "123 Test",
                Postcode = "E1 123",
                Balance = thing.currentBalance,
                LastActionTime = thing.lastActionTime,
                LastActionType = thing.lastActionType,
                CurrentAgreementStatus = thing.agreementStatus
            };
            
            Assert.Single(tenancies);

            Assert.Contains(expectedTenancy, tenancies);
        }

        private List<object> GetTenanciesByRef(List<string> refs)
        {
            var gateway = new UhTenanciesGateway();
            var tenancies = gateway.GetTenanciesByRefs(refs);

            return tenancies;
        }
    }
}
