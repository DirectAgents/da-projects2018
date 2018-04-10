using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.TypeConversion;

namespace CakeExtracter.Etl.SearchMarketing.Extracters
{
    public class BingDailySummaryExtracter : BingExtracterBase
    {
        private readonly bool includeShopping; // (shopping campaigns)
        private readonly bool includeNonShopping;

        public BingDailySummaryExtracter(long accountId, DateTime startDate, DateTime endDate, bool includeShopping = true, bool includeNonShopping = true)
            : base(accountId, startDate, endDate)
        {
            this.includeShopping = includeShopping;
            this.includeNonShopping = includeNonShopping;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting SearchDailySummaries for {0} from {1} to {2}", accountId, startDate, endDate);
            if (includeNonShopping)
            {
                var items = ExtractAndEnumerateRows(forShoppingCampaigns: false);
                Add(items);
            }
            if (includeShopping)
            {
                var items = ExtractAndEnumerateRows(forShoppingCampaigns: true);
                Add(items);
            }
            End();
        }

        private IEnumerable<Dictionary<string, string>> ExtractAndEnumerateRows(bool forShoppingCampaigns = false)
        {
            var bingUtility = new BingAds.BingUtility(m => Logger.Info(m), m => Logger.Warn(m));
            var filepath = bingUtility.GetReport_DailySummaries(accountId, startDate, endDate, forShoppingCampaigns: forShoppingCampaigns);
            if (filepath == null)
                yield break;

            IEnumerable<BingRow> bingRows;
            if (forShoppingCampaigns)
                bingRows = GroupAndEnumerateBingRows(filepath, throwOnMissingField: false);
            else
                bingRows = BingExtracterBase.EnumerateRowsGeneric<BingRow>(filepath, throwOnMissingField: true);

            foreach (var row in EnumerateRowsAsDictionaries(bingRows))
                yield return row;
        }
    }

    public class BingConvSummaryExtracter : BingExtracterBase
    {
        public BingConvSummaryExtracter(long accountId, DateTime startDate, DateTime endDate)
            : base(accountId, startDate, endDate) { }

        protected override void Extract()
        {
            Logger.Info("Extracting SearchConvSummaries for {0} from {1} to {2}", accountId, startDate, endDate);
            var items = ExtractAndEnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<Dictionary<string, string>> ExtractAndEnumerateRows()
        {
            var bingUtility = new BingAds.BingUtility(m => Logger.Info(m), m => Logger.Warn(m));
            var filepath = bingUtility.GetReport_DailySummariesByGoal(accountId, startDate, endDate);
            if (filepath == null)
                yield break;

            var bingRowsWithGoal = EnumerateRowsGeneric<BingRowWithGoal>(filepath, throwOnMissingField: false);

            foreach (var row in EnumerateRowsAsDictionaries(bingRowsWithGoal))
                yield return row;
        }
    }

    // --- Base class ---

    public abstract class BingExtracterBase : Extracter<Dictionary<string, string>>
    {
        protected readonly long accountId;
        protected readonly DateTime startDate;
        protected readonly DateTime endDate;

        public BingExtracterBase(long accountId, DateTime startDate, DateTime endDate)
        {
            this.accountId = accountId;
            this.startDate = startDate;
            this.endDate = endDate;
        }

        protected IEnumerable<Dictionary<string, string>> EnumerateRowsAsDictionaries<T>(IEnumerable<T> rows)
        {
            foreach (var row in rows)
            {
                var dict = new Dictionary<string, string>();

                // Use reflection to add values
                var type = typeof(T);
                var properties = type.GetProperties();
                foreach (var propertyInfo in properties)
                {
                    if (propertyInfo.Name == "AccountId" && String.IsNullOrWhiteSpace((string)propertyInfo.GetValue(row)))
                        dict["AccountId"] = this.accountId.ToString();
                    else if (propertyInfo.PropertyType == typeof(int))
                        dict[propertyInfo.Name] = ((int)propertyInfo.GetValue(row)).ToString();
                    else if (propertyInfo.PropertyType == typeof(decimal))
                        dict[propertyInfo.Name] = ((decimal)propertyInfo.GetValue(row)).ToString();
                    else
                        dict[propertyInfo.Name] = (string)propertyInfo.GetValue(row);

                    //TODO: Have the extracter return objects with typed properties (and have the loader handle that)
                }
                yield return dict;
            }
        }

        // ?Should this be in BingDailySummaryExtracter?
        protected IEnumerable<BingRow> GroupAndEnumerateBingRows(string filepath, bool throwOnMissingField)
        {
            var groups = EnumerateRowsGeneric<BingRow>(filepath, throwOnMissingField)
                .GroupBy(b => new { b.GregorianDate, b.AccountId, b.AccountName, b.AccountNumber, b.CampaignId, b.CampaignName });
            foreach (var g in groups)
            {
                var bingRow = new BingRow
                {
                    GregorianDate = g.Key.GregorianDate,
                    AccountId = g.Key.AccountId,
                    AccountName = g.Key.AccountName,
                    AccountNumber = g.Key.AccountNumber,
                    CampaignId = g.Key.CampaignId,
                    CampaignName = g.Key.CampaignName,
                    Impressions = g.Sum(r => r.Impressions),
                    Clicks = g.Sum(r => r.Clicks),
                    Conversions = g.Sum(r => r.Conversions),
                    Spend = g.Sum(r => r.Spend),
                    Revenue = g.Sum(r => r.Revenue)
                };
                yield return bingRow;
            }
        }
        protected static IEnumerable<T> EnumerateRowsGeneric<T>(string filepath, bool throwOnMissingField)
        {
            TypeConverterOptionsFactory.GetOptions(typeof(decimal)).NumberStyle = NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint;

            using (StreamReader reader = File.OpenText(filepath))
            {
                for (int i = 0; i < 9; i++)
                    reader.ReadLine();

                using (CsvReader csv = new CsvReader(reader))
                {
                    csv.Configuration.SkipEmptyRecords = true;
                    csv.Configuration.WillThrowOnMissingField = throwOnMissingField;

                    while (csv.Read())
                    {
                        T csvRow;
                        try
                        {
                            csvRow = csv.GetRecord<T>();
                        }
                        catch (CsvHelperException ex)
                        {
                            continue; // if error converting the row
                        }
                        yield return csvRow;
                    }
                }
            }
        }

        //public sealed class BingRowMap : CsvClassMap<BingRow>
        //{
        //    public BingRowMap() ...
        //      Map(m => m.Spend).TypeConverterOption(NumberStyles.Currency);
        //}
        //NOTE: setting globally (TypeConverterOptionsFactory options - above) instead

        protected class BingRow
        {
            public string GregorianDate { get; set; } // date
            public int Impressions { get; set; } // int
            public int Clicks { get; set; } // int
            public int Conversions { get; set; } // int
            public decimal Spend { get; set; } // decimal
            public decimal Revenue { get; set; } // decimal
            public string AccountId { get; set; } // int
            public string AccountName { get; set; } // string
            public string AccountNumber { get; set; } // string
            public string CampaignId { get; set; } // int
            public string CampaignName { get; set; } // string

            //public string MerchantProductId { get; set; }
            //public string CurrencyCode { get; set; }
        }

        protected class BingRowWithGoal : BingRow
        {
            public string GoalId { get; set; } // int?
            public string Goal { get; set; } // string
        }
    }

}
