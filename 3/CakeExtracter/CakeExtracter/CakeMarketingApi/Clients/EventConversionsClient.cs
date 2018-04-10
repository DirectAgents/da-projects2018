using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class EventConversionsClient : ApiClient
    {
        public EventConversionsClient()
            : base(17, "reports", "EventConversions")
        {
        }

        public EventConversionResponse EventConversions(EventConversionsRequest request)
        {
            var result = Execute<EventConversionResponse>(request, new ConversionsDeserializer());
            return result;
        }
    }
}
