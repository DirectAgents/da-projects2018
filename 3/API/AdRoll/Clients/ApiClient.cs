using System;
//using System.Collections.Generic;
//using System.IO;
using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;

namespace AdRoll.Clients
{
    abstract public class ApiClient
    {
        //protected const string Domain = "app.adroll.com/api";
        protected const string Domain = "api.adroll.com";
        protected const string ReportingDomain = "app.adroll.com/uhura";
        protected readonly string BaseUrl;

        private string Username { get; set; }
        private string Password { get; set; }

        protected ApiClient(int version, string service, string method, bool reporting = false)
        {
            string domain = (reporting ? ReportingDomain : Domain);
            BaseUrl = "https://" + domain + "/v" + version + "/" + service + "/" + method;
        }

        public void SetCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }

        // --- Logging ---
        public void SetLogging(Action<string> logInfo, Action<string> logError)
        {
            _LogInfo = logInfo;
            _LogError = logError;
        }

        private Action<string> _LogInfo;
        private Action<string> _LogError;

        private void LogInfo(string message)
        {
            if (_LogInfo != null)
                _LogInfo(message);
        }

        private void LogError(string message)
        {
            if (_LogError != null)
                _LogError(message);
        }

        // ---

        //public T ExecuteX<T>(ApiRequest apiRequest) where T : new()
        //{
        //    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        //    var up = Username.Replace("@", "%40") + ":" + Password.Replace("@", "%40");
        //    var requestUri = BaseUrl.Replace("https://", "https://" + up + "@") + "v1/organization/get";

        //    HttpWebRequest http = (HttpWebRequest)WebRequest.Create(requestUri);
        //    http.KeepAlive = false;
        //    WebResponse response = http.GetResponse();
        //    var stream = response.GetResponseStream();
        //    StreamReader sr = new StreamReader(stream);
        //    string content = sr.ReadToEnd();

        //    return default(T);
        //}

        //public T ExecuteY<T>(ApiRequest apiRequest) where T : new()
        //{
        //    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        //    HttpClient client = new HttpClient();
        //    var up = Username.Replace("@", "%40") + ":" + Password.Replace("@", "%40");
        //    //var up = Username + ":" + Password;
        //    //client.BaseAddress = new Uri(BaseUrl.Replace("https://", "https://" + up + "@"));
        //    //client.BaseAddress = new Uri(BaseUrl);
        //    client.DefaultRequestHeaders.Accept.Clear();
        //    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        //    //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // test/plain ??
        //    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
        //    //    Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(
        //    //        string.Format("{0}:{1}", Username, Password))));
        //            //string.Format("{0}:{1}", Username.Replace("@", "%40"), Password.Replace("@", "%40")))));

        //    var urlParameters = apiRequest.ParametersAsString(includeQuestionMark: true);

        //    var requestUri = BaseUrl.Replace("https://", "https://" + up + "@") + "v1/organization/get";
        //    var request = new HttpRequestMessage()
        //    {
        //        RequestUri = new Uri(requestUri),
        //        Method = HttpMethod.Get
        //    };
        //    //request.Headers.Add("Connection", new string[] { "Keep-Alive" });
        //    //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
        //    var task = client.SendAsync(request);

        //    //var task = client.GetAsync(requestUri);
        //    var response = task.Result;
        //    //HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
        //    if (response.IsSuccessStatusCode)
        //    {
        //        // Parse the response body. Blocking!
        //        var data = response.Content.ReadAsAsync<T>().Result;
        //        return data;
        //    }
        //    else
        //    {
        //        LogError(string.Format("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase));
        //        return default(T);
        //    }
        //}

        public T Execute<T>(ApiRequest apiRequest) where T : new()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            var restRequest = new RestRequest();
            //restRequest.AddHeader("Accept", "*/*");
            //restRequest.Timeout = 2000;

            apiRequest.AddParametersTo(restRequest);

            var restClient = new RestClient
            {
                BaseUrl = new Uri(BaseUrl),
                Authenticator = new HttpBasicAuthenticator(Username, Password)
            };
            restClient.AddHandler("text/plain", new JsonDeserializer());
            //restClient.MaxRedirects = 10;

            var response = restClient.ExecuteAsGet<T>(restRequest, "GET");

            if (response.ErrorException != null)
            {
                LogError(response.ErrorException.Message);
                return default(T);
            }

            return response.Data;
        }
    }
}
