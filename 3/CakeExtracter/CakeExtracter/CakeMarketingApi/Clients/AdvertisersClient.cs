using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class AdvertisersClient : ApiClient
    {
        public AdvertisersClient()
            : base(5, "export", "Advertisers")
        {
        }

        public AdvertiserExportResponse Advertisers(AdvertisersRequest request)
        {
            var result = Execute<AdvertiserExportResponse>(request);
            return result;
        }
    }
}
