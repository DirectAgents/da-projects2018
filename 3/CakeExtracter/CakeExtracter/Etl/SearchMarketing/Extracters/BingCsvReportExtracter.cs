using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace CakeExtracter.Etl.SearchMarketing.Extracters
{
    public class BingCsvReportExtracter : Extracter<Dictionary<string, string>>
    {
        private readonly string csvFilePath;

        public BingCsvReportExtracter(string csvFilePath) //, string accountName)
        {
            this.csvFilePath = csvFilePath;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting SearchDailySummaries from {0}", csvFilePath);
            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<Dictionary<string, string>> EnumerateRows()
        {
            using (StreamReader reader = File.OpenText(csvFilePath))
            {
                for (int i = 0; i < 9; i++)
                    reader.ReadLine();

                using (CsvReader csv = new CsvReader(reader))
                {
                    csv.Configuration.SkipEmptyRecords = true;
                    csv.Configuration.RegisterClassMap<BingRowMap>();
                    while (csv.Read())
                    {
                        BingRow csvRow;
                        try
                        {
                            csvRow = csv.GetRecord<BingRow>();
                        }
                        catch (CsvHelperException)
                        {
                            continue; // if error converting the row
                        }
                        if (csvRow.GregorianDate.ToLower().Contains("microsoft"))
                            continue; // skip footer

                        var row = new Dictionary<string, string>();

                        // Use reflection to add values
                        var type = typeof(BingRow);
                        var properties = type.GetProperties();
                        foreach (var propertyInfo in properties)
                        {
                            row[propertyInfo.Name] = (string)propertyInfo.GetValue(csvRow);
                        }
                        yield return row;
                    }
                }
            }
        }

        private sealed class BingRowMap : CsvClassMap<BingRow>
        {
            public BingRowMap()
            {
                Map(m => m.GregorianDate).Name("Gregorian date");
                Map(m => m.Impressions);
                Map(m => m.Clicks);
                Map(m => m.Spend);
                Map(m => m.Conversions);
                Map(m => m.Revenue);
                Map(m => m.CampaignName).Name("Campaign name");
                Map(m => m.CampaignId).Name("Campaign ID");
            }
        }
        private class BingRow
        {
            public string GregorianDate { get; set; }

            public string Impressions { get; set; } // int
            public string Clicks { get; set; } // int
            public string Spend { get; set; } // decimal
            public string Conversions { get; set; } // int
            public string Revenue { get; set; } // decimal

            //[CsvField(Name = "Account name")]
            //public string AccountName { get; set; }
            //[CsvField(Name = "Account ID")]
            //public string AccountId { get; set; } // int
            //[CsvField(Name = "Account number")]
            //public string AccountNumber { get; set; }

            public string CampaignName { get; set; }
            public string CampaignId { get; set; } // int

            //[CsvField(Name="Currency code")]
            //public string CurrencyCode { get; set; }
        }
    }
}
