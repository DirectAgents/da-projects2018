using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class AdrollSiteStatsCsvExtracter : Extracter<AdrollSiteStatsRow>
    {
        private readonly string csvFilePath;
        private readonly StreamReader streamReader;
        // if streamReader is not null, use it. otherwise use csvFilePath.

        public AdrollSiteStatsCsvExtracter(string csvFilePath, StreamReader streamReader)
        {
            this.csvFilePath = csvFilePath;
            this.streamReader = streamReader;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting Site Stats from {0}", csvFilePath ?? "StreamReader");
            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<AdrollSiteStatsRow> EnumerateRows()
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

        private IEnumerable<AdrollSiteStatsRow> EnumerateRowsInner(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                csv.Configuration.IgnoreHeaderWhiteSpace = true;
                csv.Configuration.SkipEmptyRecords = true;
                csv.Configuration.RegisterClassMap<AdrollSiteStatsRowMap>();

                var csvRows = csv.GetRecords<AdrollSiteStatsRow>().ToList();
                var siteGroups = csvRows.GroupBy(row => row.website);
                foreach (var siteGroup in siteGroups)
                {
                    var siteStats = new AdrollSiteStatsRow
                    {
                        website = siteGroup.Key,
                        click = siteGroup.Sum(row => row.click),
                        impression = siteGroup.Sum(row => row.impression),
                        cost = siteGroup.Sum(row => row.cost)
                    };
                    yield return siteStats;
                }
            }
        }
    }

 
    public sealed class AdrollSiteStatsRowMap : CsvClassMap<AdrollSiteStatsRow>
    {
        public AdrollSiteStatsRowMap()
        {
            Map(m => m.website).Name("Website");
            Map(m => m.size).Name("Size");
            Map(m => m.cost).Name("SpendoverPeriod").TypeConverterOption(NumberStyles.Currency);
            Map(m => m.impression).Name("Impressions").TypeConverterOption(NumberStyles.Number);
            Map(m => m.click).Name("Clicks").TypeConverterOption(NumberStyles.Number);
        }
    }

    public class AdrollSiteStatsRow
    {
        private string _website;
        public string website
        {
            get { return _website; }
            set { _website = value.ToLower(); }
        }
        public string size { get; set; }
        public decimal cost { get; set; }
        public int impression { get; set; }
        public int click { get; set; }

    }
}