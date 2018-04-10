using System;
using RestSharp;
using RestSharp.Deserializers;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    abstract public class ApiClient
    {
        protected const string ApiKey = "FCjdYAcwQE";
        protected const string Domain = "login.directagents.com";
        protected readonly string BaseUrl;

        protected ApiClient(int version, string asmx, string operation)
        {
            BaseUrl = "https://" + Domain + "/api/" + version + "/" + asmx + ".asmx/" + operation;
        }

        public T Execute<T>(ApiRequest apiRequest, IDeserializer deserializer = null) where T : new()
        {
            var restRequest = new RestRequest();

            restRequest.AddParameter("api_key", ApiKey);
       
            apiRequest.AddParameters(restRequest);

            var restClient = new RestClient { BaseUrl = new Uri(BaseUrl) };

            if (deserializer != null)
            {
                restClient.AddHandler("text/xml", deserializer);
            }

            var response = restClient.ExecuteAsGet<T>(restRequest, "GET");

            if (response.ErrorException != null)
            {
                Logger.Error(response.ErrorException);
                throw new ApplicationException("Error retrieving response.", response.ErrorException);
            }

            return response.Data;
        }
    }
}