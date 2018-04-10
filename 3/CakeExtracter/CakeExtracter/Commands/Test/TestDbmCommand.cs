using System;
using System.ComponentModel.Composition;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CakeExtracter.Common;
using DBM;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.DoubleClickBidManager.v1;
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using RestSharp.Deserializers;

namespace CakeExtracter.Commands.Test
{
    [Export(typeof(ConsoleCommand))]
    public class TestDbmCommand : ConsoleCommand
    {
        public int Mode { get; set; }
        public bool Download { get; set; }

        public override void ResetProperties()
        {
            Mode = 0;
            Download = false;
        }

        public TestDbmCommand()
        {
            IsCommand("testDbm");
            HasOption<int>("m|mode=", "Mode", c => Mode = c);
            HasOption<bool>("d|download=", "Download(true/false)", c => Download = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            if (Mode == 2)
                TestUtil();
            else if (Mode == 1)
                Test1();
            else
                Test0();
            return 0;
        }

        public void TestUtil()
        {
            var dbmUtility = new DBMUtility(m => Logger.Info(m), m => Logger.Warn(m));
            dbmUtility.TokenSets = new string[] { "|DBMDBM|1/VC8MQArCKHna2NmLFYg4GVcftxtgMo1p4lpw-ZeLXRo" };
            dbmUtility.Test();
        }

        public ClientSecrets GetClientSecrets()
        {
            string clientId = ConfigurationManager.AppSettings["GoogleAPI_ClientId"];
            string clientSecret = ConfigurationManager.AppSettings["GoogleAPI_ClientSecret"];
            return new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            };
        }
        public async Task<UserCredential> GetUserCredential()
        {
            //using (var stream = new FileStream("google_client_secret_0.json", FileMode.Open, FileAccess.Read))
            //{
            //    var secrets = GoogleClientSecrets.Load(stream).Secrets;
                var secrets = GetClientSecrets();
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets,
                    new[] { "https://www.googleapis.com/auth/doubleclickbidmanager" },
                    "user", CancellationToken.None);
            //}
        }
        public UserCredential GetUserCredential2()
        {
            var secrets = GetClientSecrets();
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = secrets,
                Scopes = new[] { "https://www.googleapis.com/auth/doubleclickbidmanager" }
            });
            var tokens = new TokenResponse
            {
                //AccessToken = "",
                RefreshToken = "1/VC8MQArCKHna2NmLFYg4GVcftxtgMo1p4lpw-ZeLXRo"
            };
            var credential = new UserCredential(flow, "user", tokens);
            return credential;
        }

        public ServiceAccountCredential GetServiceAccountCredential()
        {
            string serviceEmail = ConfigurationManager.AppSettings["GoogleAPI_ServiceEmail"];
            string certPath = ConfigurationManager.AppSettings["GoogleAPI_Certificate"];
            var certificate = new X509Certificate2(certPath, "notasecret", X509KeyStorageFlags.Exportable);
            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(serviceEmail)
                {
                    Scopes = new[] { "https://www.googleapis.com/auth/doubleclickbidmanager" }
                }.FromCertificate(certificate));

            return credential;
        }

        // Try DBM API
        public void Test0()
        {
            //var credential = GetServiceAccountCredential();
            //var credential = GetUserCredential().Result;
            var credential = GetUserCredential2();

            var service = new DoubleClickBidManagerService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "DA Client Portal"
            });
            var resource = new QueriesResource(service);
            var request = resource.Listqueries();
            var response = request.Execute();
        }

        public void Test1()
        {
            try
            {
                string serviceEmail = ConfigurationManager.AppSettings["GoogleAPI_ServiceEmail"];
                string certPath = ConfigurationManager.AppSettings["GoogleAPI_Certificate"];
                var certificate = new X509Certificate2(certPath, "notasecret", X509KeyStorageFlags.Exportable);

                var credential = new ServiceAccountCredential(
                    new ServiceAccountCredential.Initializer(serviceEmail)
                    {
                        Scopes = new[] { StorageService.Scope.DevstorageReadOnly }
                    }.FromCertificate(certificate));
                var service = new StorageService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "DA Client Portal"
                });

                //string bucketObjectName = "entity/20130430.0.Browser.json";
                //string bucketName = "gdbm-479-320231"; //"uspto-pair"; (ok) //"gdbm-public"; *forbidden*
                //string bucketName = "151075984680687222131403708138869_report";
                //string bucketName = "099700104058925463911409777269032_report";
                //string bucketName = "151075984680687222131409855651304_report"; // test123 (2:34)
                //string bucketName = "151075984680687222131409861541653_report"; // ui_created
                //string bucketName = "151075984680687222131410283081521_report"; // Betterment_creative

                ListBucket(service, credential);
                GetBucket(service, credential);

                //var request = service.Objects.List(bucketName);
                //var results = request.Execute();

                //var listRequest = service.BucketAccessControls.List(bucketName);
                //var bucketAccessControls = listRequest.Execute();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception caught: " + e.Message);
                Console.Write(e.StackTrace);
            }
        }
        private void ListBucket(StorageService service, ServiceAccountCredential credential)
        {
            string bucketName = "151075984680687222131440452270742_report"; // Crackle_creative

            // List
            var request = service.Objects.List(bucketName);
            var results = request.Execute();
            Logger.Info("Found {0} objects in the bucket.", results.Items.Count);
            string dateString = DateTime.Today.ToString("yyyy-MM-dd");
            var reportObject = results.Items.Where(i => i.Name.Contains(dateString)).FirstOrDefault();

            if (this.Download) TestDownload(credential, reportObject);
        }
        private void GetBucket(StorageService service, ServiceAccountCredential credential)
        {
            string bucketName = "gdbm-479-320231";
            //string bucketName = "gdbm-479-320231/entity/";

            //var listRequest = service.Objects.List(bucketName + "/entity/");
            var listRequest = service.Objects.List(bucketName);
            var results = listRequest.Execute();
            Logger.Info("Found {0} objects in the bucket.", results.Items.Count);
            string dateString = DateTime.Today.ToString("yyyyMMdd");
            var x = results.Items.Where(i => i.Name.Contains(dateString));

            var bucketRequest = service.Buckets.Get(bucketName);
            var bucketResult = bucketRequest.Execute();

            var getRequest = service.Objects.Get(bucketName, "entity/20160523.0.Creative.json");
            var reportObject = getRequest.Execute();

            if (this.Download) TestDownload(credential, reportObject);
        }
        private void TestDownload(ServiceAccountCredential credential, Google.Apis.Storage.v1.Data.Object reportObject)
        {
            //var x = service.Objects.Get(bucketName, reportObject.Name).Execute();
            HttpWebRequest req = createRequest(reportObject.MediaLink, credential);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            // Handle redirects manully to ensure that the Authorization header is present if
            // our request is redirected.
            if (resp.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                req = createRequest(resp.Headers["Location"], credential);
                resp = (HttpWebResponse)req.GetResponse();
            }

            Stream stream = resp.GetResponseStream();
            StreamReader reader = new StreamReader(stream);
            String data = reader.ReadToEnd();
        }

        /// <summary>
        /// Generate a HttpWebRequest for the given URL with the appropriate OAuth2 authorization
        /// header applied.  The HttpWebRequest object returned has its AllowAutoRedirect option
        /// disabled to allow us to manually handle redirects.
        /// </summary>
        private static HttpWebRequest createRequest(string url, ServiceAccountCredential credential)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Bearer " + credential.Token.AccessToken);
            request.AllowAutoRedirect = false;
            return request;
        }

    }
}
