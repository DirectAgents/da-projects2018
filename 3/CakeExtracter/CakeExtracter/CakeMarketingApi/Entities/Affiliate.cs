using System.Collections.Generic;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class Affiliate
    {
        public int AffiliateId { get; set; }
        public string AffiliateName { get; set; }
        //AccountManagers
        //AccountStatus
        //Address
        //PaymentType
        //Contacts
        //etc...
    }

    public class AffiliateExportResponse
    {
        public List<Affiliate> Affiliates { get; set; }
    }
}
