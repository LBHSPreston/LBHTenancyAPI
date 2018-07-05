using System;

namespace LBHTenancyAPI.Domain
{
    public struct TenancyListItem
    {
        public string TenancyRef;
        public string PrimaryContactName;
        public string ShortAddress;
        public string Postcode;
        public double Balance;
        public DateTime LastActionTime;
        public string LastActionType;
        public string CurrentAgreementStatus;
    }
}
