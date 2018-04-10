using System;
using System.ComponentModel.Composition;
using Adform;
using CakeExtracter.Common;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;
using Yahoo;

namespace CakeExtracter.Commands.Test
{
    [Export(typeof(ConsoleCommand))]
    public class TestAdformCommand : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public TestAdformCommand()
        {
            IsCommand("testAdform");
        }

        public override int Execute(string[] remainingArguments)
        {
            Test2();
            return 0;
        }

        public void Test3()
        {
            var adformUtility = new AdformUtility();
            adformUtility.GetDimensions();
        }

        public void Test2()
        {
            var adformUtility = new AdformUtility();
            //adformUtility.TestReport();
            var start = new DateTime(2017, 4, 20);
            var end = new DateTime(2017, 4, 20);
            int clientId = 54314; // G&T
            var parms = adformUtility.CreateReportParams(start, end, clientId, byMedia: true, RTBonly: false);
            var reportData = adformUtility.GetReportData(parms);

            foreach (var row in reportData.rows)
            {
                Logger.Info("row");
            }
        }

        public void Test1()
        {
            string ClientID = "portal.directagents.us@clients.adform.com";
            string ClientSecret = "zA6Fn7PEFk103Vf2FV93NzP66d2411QHL817m7B54B";
            string Scope = "https://api.adform.com/scope/eapi";

            var restClient = new RestClient
            {
                BaseUrl = new Uri("https://id.adform.com/sts/connect/token"),
                Authenticator = new HttpBasicAuthenticator(ClientID, ClientSecret)
            };
            restClient.AddHandler("application/json", new JsonDeserializer());

            var request = new RestRequest();
            request.AddParameter("grant_type", "client_credentials");
            request.AddParameter("scope", Scope);

            var response = restClient.ExecuteAsPost<TestTokenResponse>(request, "POST");
        }
    }

    public class TestTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }
}
