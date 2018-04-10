using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class TrafficClient : ApiClient
    {
        public TrafficClient()
            : base(1, "reports", "TrafficExport")
        {
        }

        public ArrayOfTraffic Traffic(TrafficRequest request)
        {
            var result = Execute<ArrayOfTraffic>(request);
            return result;
        }
    }
}