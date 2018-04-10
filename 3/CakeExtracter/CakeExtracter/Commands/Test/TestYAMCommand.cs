using System;
using System.ComponentModel.Composition;
using CakeExtracter.Common;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;
using Yahoo;

namespace CakeExtracter.Commands.Test
{
    [Export(typeof(ConsoleCommand))]
    public class TestYAMCommand : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public TestYAMCommand()
        {
            IsCommand("testYAM");
        }

        public override int Execute(string[] remainingArguments)
        {
            Test2();
            return 0;
        }

        public void Test2()
        {
            var yamUtility = new YAMUtility();
            //yamUtility.CreateTestReport();
            //string id = "85661025-129d-4331-9379-392e4a27a6a2";
            //yamUtility.GetReportStatus(id);
            //yamUtility.ObtainReportUrl();
        }

        public void Test1()
        {
            string ClientID = "dj0yJmk9alpSQTNtTHRRTkdOJmQ9WVdrOVZqWk1NRUp1TXpBbWNHbzlNQS0tJnM9Y29uc3VtZXJzZWNyZXQmeD1iNA--";
            string ClientSecret = "71865f432c8d52026fdab55d3bacc905858ace28";
            string ApplicationAccessCode = "a3whumv";

            var restClient = new RestClient
            {
                BaseUrl = new Uri("https://api.login.yahoo.com/oauth2/get_token"),
                Authenticator = new HttpBasicAuthenticator(ClientID, ClientSecret)
            };
            //restClient.AddHandler("text/plain", new JsonDeserializer());
            restClient.AddHandler("application/json", new JsonDeserializer());

            //security protocol?
            var request = new RestRequest();
            request.AddParameter("redirect_uri", "oob");
            //request.AddParameter("grant_type", "authorization_code");
            //request.AddParameter("code", ApplicationAccessCode);
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", "AAkFyFjSiX12SJglBIkT6hRNnBl5XgPBLAX4ku5kXB5VFauy");

            var response = restClient.ExecuteAsPost<GetTokenResponse>(request, "POST");

        }
    }
    public class GetTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string xoauth_yahoo_guid { get; set; }
    }
}
