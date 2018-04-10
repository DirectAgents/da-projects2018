using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class AdrollCsvExtracter : Extracter<AdrollRow>
    {
        private readonly string csvFilePath;
        private readonly StreamReader streamReader;
        // if streamReader is not null, use it. otherwise use csvFilePath.

        public AdrollCsvExtracter(string csvFilePath, StreamReader streamReader)
        {
            this.csvFilePath = csvFilePath;
            this.streamReader = streamReader;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting DailySummaries from {0}", csvFilePath ?? "StreamReader");
            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<AdrollRow> EnumerateRows()
        {
            if (streamReader != null)
            {
                foreach (var row in EnumerateRowsInner(streamReader))
                    yield return row;
            }
            else
            {
                using (StreamReader reader = File.OpenText(csvFilePath))
                {
                    foreach (var row in EnumerateRowsInner(reader))
                        yield return row;
                }
            }
        }

        private IEnumerable<AdrollRow> EnumerateRowsInner(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                csv.Configuration.SkipEmptyRecords = true;
                csv.Configuration.RegisterClassMap<AdrollRowMap>();

                var csvRows = csv.GetRecords<AdrollRow>().ToList();
                for (int i = 0; i < csvRows.Count && !String.IsNullOrWhiteSpace(csvRows[i].AdName); i++)
                {
                    yield return csvRows[i];
                }
            }
        }
    }

    // Todo: use this and rm spaces in Name values
    // csv.Configuration.IgnoreHeaderWhiteSpace = true;

    public sealed class AdrollRowMap : CsvClassMap<AdrollRow>
    {
        public AdrollRowMap()
        {
            Map(m => m.Date);
            Map(m => m.AdName).Name("Ad Name");
            Map(m => m.Size);
            Map(m => m.Type);
            Map(m => m.CreateDate).Name("Created Date");
            Map(m => m.Spend).Name("Spend over Period");
            Map(m => m.Impressions);
            Map(m => m.Clicks);
            Map(m => m.TotalConversions).Name("Total Conv.");
        }
    }

    public class AdrollRow
    {
        public string Date { get; set; }

        public string AdName { get; set; }
        public string Size { get; set; }
        public string Type { get; set; }
        public string CreateDate { get; set; }

        public string Spend { get; set; }
        public string Impressions { get; set; }
        public string Clicks { get; set; }
        public string TotalConversions { get; set; }
    }
}
