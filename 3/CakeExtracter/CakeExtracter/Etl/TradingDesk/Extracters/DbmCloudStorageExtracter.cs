using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Storage.v1;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class DbmCloudStorageExtracter : Extracter<DbmRowBase>
    {
        // if specified, dateFilter is used to select objects (by name) within the specified buckets
        private readonly DateTime? dateFilter;
        private readonly IEnumerable<string> bucketNames;
        private readonly int? ioFilter;

        private readonly bool byLineItem;
        private readonly bool byCreative;
        private readonly bool bySite;
        // Note: only set at most *one* of these to true

        public int ImpressionThreshold { get; set; } // used only for site stats

        public DbmCloudStorageExtracter(DateTime? dateFilter, IEnumerable<string> bucketNames, bool byLineItem = false, bool byCreative = false, bool bySite = false, int? ioFilter = null)
        {
            this.dateFilter = dateFilter;
            this.bucketNames = bucketNames;
            this.byLineItem = byLineItem;
            this.byCreative = byCreative;
            this.bySite = bySite;
            this.ioFilter = ioFilter;
        }

        protected override void Extract()
        {
            string by = "";
            if (bySite) by = " by site";
            if (byCreative) by = " by creative";
            if (byLineItem) by = " by lineitem"; // takes precedence
            string datePart = "";
            if (dateFilter != null)
                datePart = string.Format(" - report date {0:d}", dateFilter.Value);
            Logger.Info("Extracting DailySummary reports{0} from {1} buckets{2}", by, bucketNames.Count(), datePart);

            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<DbmRowBase> EnumerateRows()
        {
            var credential = CreateCredential();
            var service = CreateStorageService(credential);

            foreach (var bucketName in bucketNames)
            {
                var request = service.Objects.List(bucketName);
                var bucketObjects = request.Execute();

                IEnumerable<Google.Apis.Storage.v1.Data.Object> reportObjects = bucketObjects.Items;
                if (dateFilter != null)
                {
                    string dateString = dateFilter.Value.ToString("yyyy-MM-dd");
                    reportObjects = reportObjects.Where(o => o.Name.Contains(dateString));
                }

                foreach (var reportObject in reportObjects) // usually should be just one
                {
                    var stream = GetStreamForCloudStorageObject(reportObject, credential);
                    using (var reader = new StreamReader(stream))
                    {
                        if (!bySite)
                        {
                            foreach (var row in DbmCsvExtracter.EnumerateRowsStatic(reader, byLineItem: byLineItem, byCreative: byCreative, bySite: bySite))
                            {
                                if (!ioFilter.HasValue || ioFilter.Value == row.InsertionOrderID)
                                    yield return row;
                            }
                        }
                        else
                        {   // for site stats: do filtering
                            foreach (var row in DbmCsvExtracter.EnumerateRowsStatic(reader, byLineItem: byLineItem, byCreative: byCreative, bySite: bySite))
                            {
                                int impressions = int.Parse(row.Impressions);
                                int conversions = (int)decimal.Parse(row.TotalConversions);
                                if (impressions >= ImpressionThreshold || conversions > 0)
                                {
                                    if (!ioFilter.HasValue || ioFilter.Value == row.InsertionOrderID)
                                        yield return row;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static Stream GetStreamForCloudStorageObject(Google.Apis.Storage.v1.Data.Object cloudStorageObject, ServiceAccountCredential credential)
        {
            HttpWebRequest req = CreateRequest(cloudStorageObject.MediaLink, credential);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            // Handle redirects manully to ensure that the Authorization header is present if
            // our request is redirected.
            if (resp.StatusCode == HttpStatusCode.TemporaryRedirect)
            {
                req = CreateRequest(resp.Headers["Location"], credential);
                resp = (HttpWebResponse)req.GetResponse();
            }
            return resp.GetResponseStream();
        }

        public static ServiceAccountCredential CreateCredential()
        {
            string serviceEmail = ConfigurationManager.AppSettings["GoogleAPI_ServiceEmail"];
            string certPath = ConfigurationManager.AppSettings["GoogleAPI_Certificate"];
            var certificate = new X509Certificate2(certPath, "notasecret", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

            var credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(serviceEmail)
                {
                    Scopes = new[] { StorageService.Scope.DevstorageReadOnly }
                }.FromCertificate(certificate));

            return credential;
        }
        public static StorageService CreateStorageService(ServiceAccountCredential credential)
        {
            var service = new StorageService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "DA Client Portal"
            });
            return service;
        }

        /// <summary>
        /// Generate a HttpWebRequest for the given URL with the appropriate OAuth2 authorization
        /// header applied.  The HttpWebRequest object returned has its AllowAutoRedirect option
        /// disabled to allow us to manually handle redirects.
        /// </summary>
        public static HttpWebRequest CreateRequest(string url, ServiceAccountCredential credential)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Bearer " + credential.Token.AccessToken);
            request.AllowAutoRedirect = false;
            return request;
        }
    }
}
