using System;
using System.Collections.Generic;
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
        }

        [Fact]
        public void WhenGivenNoTenancyRefs_GetTenanciesByRefs_ShouldReturnNoTenancies()
        {
            var tenancies = GetTenanciesByRef(new List<string>() { });

            Assert.Empty(tenancies);
        }

        [Fact]
        public void WhenGivenASingleTenancyRef_GetTenanciesByRefs_ShouldReturnASingleResult()
        {
            var tenancies = GetTenanciesByRef(new List<string>() {"FAKE/01"});

            Assert.Single(tenancies);
        }

        [Fact]
        public void WhenGivenTenancyRef_GetTenanciesByRefs_ShouldReturnTenancyObjectForThatRef()
        {
            var random = new Bogus.Randomizer();
            var thing = new
            {
                tenancyRef = random.String(),
                currentBalance = random.Double()
            };
            
//            _db.Query($"INSERT INTO tenagree (tag_ref, cur_bal) VALUES ({thing.tenancyRef}, {thing.currentBalance})");
//            _db.Query($"INSERT INTO araction (tag_ref, action_code, action_date) VALUES ({thing.tenancyRef}, {thing.actionCode}, x)");
//            _db.Query("INSERT INTO raaction (act_code, act_name) VALUES (x, x)");
//            _db.Query("INSERT INTO arag (tag_ref, arag_status) VALUES (x, x)");

            
            
            var tenancies = GetTenanciesByRef(new List<string>() {"FAKE/01"});
            var expectedTenancy = new TenancyListItem()
            {
                TenancyRef = thing.tenancyRef,
                PrimaryContactName = "Test User",
                ShortAddress = "123 Test",
                Postcode = "E1 123",
                Balance = thing.currentBalance,
                LastActionTime = new DateTime(1990, 10, 11),
                LastActionType = "CO1",
                CurrentAgreementStatus = "Active"
            };

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
