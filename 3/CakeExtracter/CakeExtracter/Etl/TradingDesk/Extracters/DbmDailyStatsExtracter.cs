using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using CakeExtracter.Common;
using CsvHelper;
using CsvHelper.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Storage.v1;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class DbmDailyStatsExtracter : Extracter<DailyLocationStatRow>
    {
        private readonly string bucket_DailyLocationStats;
        private readonly DateTime date;

        public DbmDailyStatsExtracter(string bucket_DailyLocationStats, DateTime date)
        {
            this.bucket_DailyLocationStats = bucket_DailyLocationStats;
            this.date = date;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting DbmDailyStats for {0:d}", date);
            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<DailyLocationStatRow> EnumerateRows()
        {
            var credential = DbmCloudStorageExtracter.CreateCredential();
            var service = DbmCloudStorageExtracter.CreateStorageService(credential);

            var request = service.Objects.List(bucket_DailyLocationStats);
            var bucketObjects = request.Execute();

            string dateString = date.ToString("yyyy-MM-dd");
            var reportObject = bucketObjects.Items.Where(i => i.Name.Contains(dateString)).FirstOrDefault();

            if (reportObject != null)
            {
                var stream = DbmCloudStorageExtracter.GetStreamForCloudStorageObject(reportObject, credential);
                using (var reader = new StreamReader(stream))
                {
                    foreach (var row in EnumerateRowsStatic(reader))
                        yield return row;
                }
            }
        }

        public static IEnumerable<DailyLocationStatRow> EnumerateRowsStatic(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                csv.Configuration.SkipEmptyRecords = true;
                csv.Configuration.IgnoreHeaderWhiteSpace = true;
                csv.Configuration.RegisterClassMap<DailyLocationStatRowMap>();

                while (csv.Read())
                {
                    DailyLocationStatRow row;
                    try
                    {
                        row = csv.GetRecord<DailyLocationStatRow>();
                    }
                    //catch (CsvHelperException, FormatException)
                    catch (Exception ex)
                    {
                        if (ex is CsvHelperException || ex is FormatException)
                        {
                            if (csv.CurrentRecord[0].Contains("Report Time"))
                                break; // end of report (ignore summary rows)
                            else
                            {
                                Logger.Warn("Exception converting row {0} (1-based)", csv.Row);
                                continue; // could not convert the row
                            }
                        }
                        throw;
                    }
                    yield return row;
                }
            }
        }
    }

    public sealed class DailyLocationStatRowMap : CsvClassMap<DailyLocationStatRow>
    {
        public DailyLocationStatRowMap()
        {
            Map(m => m.Date);
            Map(m => m.InsertionOrder);
            Map(m => m.InsertionOrderID);
            Map(m => m.City);
            Map(m => m.CityID);
            Map(m => m.Region);
            Map(m => m.RegionID);
            Map(m => m.Country);
            Map(m => m.DMACode);
            Map(m => m.DMAName);
            Map(m => m.Impressions);
            Map(m => m.Clicks);
            Map(m => m.TotalConversions);
            Map(m => m.Revenue).Name("Revenue(AdvCurrency)");
            Map(m => m.TotalMediaCost).Name("TotalMediaCost(AdvertiserCurrency)");
        }
    }
    public class DailyLocationStatRow
    {
        public DateTime Date { get; set; }
        public string InsertionOrder { get; set; }
        public int InsertionOrderID { get; set; }
        public string City { get; set; }
        public string CityID { get; set; }
        public string Region { get; set; }
        public string RegionID { get; set; }
        public string Country { get; set; }
        public string DMACode { get; set; }
        public string DMAName { get; set; }
        // Advertiser Currency
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public float TotalConversions { get; set; }
        // Post-Click Conversions, Post-View Conversions
        public decimal Revenue { get; set; }
        // Media Cost
        public decimal TotalMediaCost { get; set; }

        public int GetCityID()
        {
            int cityID;
            if (int.TryParse(this.CityID, out cityID))
                return cityID;
            else
                return -1; // unknown
        }
    }
}
