using System;
using System.Collections.Generic;
using LBHTenancyAPI.Domain;

namespace LBHTenancyAPI.Gateways
{
    public class UhTenanciesGateway
    {
        public List<object> GetTenanciesByRefs(List<string> tenancyRefs)
        {
            var tenancies = new List<object>();

            for (int i = 0; i < tenancyRefs.Count; i++)
            {
                var expectedTenancy = new TenancyListItem()
                {
                    TenancyRef = "FAKE/01",
                    PrimaryContactName = "Test User",
                    ShortAddress = "123 Test",
                    Postcode = "E1 123",
                    Balance = -100.00,
                    LastActionTime = new DateTime(1990,10,11),
                    LastActionType = "CO1",
                    CurrentAgreementStatus = "Active"
                };
                
                tenancies.Add(expectedTenancy);
            }

            return tenancies;
        }
    }
}
