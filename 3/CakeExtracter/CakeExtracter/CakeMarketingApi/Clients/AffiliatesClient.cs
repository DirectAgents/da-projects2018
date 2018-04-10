using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class AffiliatesClient : ApiClient
    {
        public AffiliatesClient()
            : base(5, "export", "Affiliates")
        { }

        public AffiliateExportResponse Affiliates(AffiliatesRequest request)
        {
            var result = Execute<AffiliateExportResponse>(request);
            return result;
        }
    }
}
