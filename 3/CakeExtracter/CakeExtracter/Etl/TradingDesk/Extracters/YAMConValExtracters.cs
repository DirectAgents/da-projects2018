using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CakeExtracter.Common;
using CsvHelper;
using CsvHelper.Configuration;
using DirectAgents.Domain.Entities.CPProg;
using Yahoo;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public abstract class YAMConValExtracter<T> : Extracter<T>
    {
        protected readonly YAMUtility _yamUtility;
        protected readonly DateRange dateRange;
        protected readonly int yamAdvertiserId;

        public YAMConValExtracter(YAMUtility yamUtility, DateRange dateRange, ExtAccount account)
        {
            this._yamUtility = yamUtility;
            this.dateRange = dateRange;
            this.yamAdvertiserId = int.Parse(account.ExternalId);
        }

        public const string ConValPattern = @"gv=(\d*\.?\d*)";
        public static decimal GetConVal(string pixelParameter)
        {
            if (pixelParameter != null)
            {
                var match = Regex.Match(pixelParameter, ConValPattern);
                if (match.Success)
                {
                    decimal conval;
                    if (decimal.TryParse(match.Groups[1].Value, out conval))
                        return conval;
                }
            }
            return 0;
        }
    }

    public class YAMDailyConValExtracter : YAMConValExtracter<DailySummary>
    {
        public YAMDailyConValExtracter(YAMUtility yamUtility, DateRange dateRange, ExtAccount account)
            : base(yamUtility, dateRange, account)
        { }

        protected override void Extract()
        {
            var payload = _yamUtility.CreateReportRequestPayload(dateRange.FromDate, dateRange.ToDate, this.yamAdvertiserId, byPixelParameter: true);
            var reportUrl = _yamUtility.GenerateReport(payload);
            if (!string.IsNullOrWhiteSpace(reportUrl))
            {
                var streamReader = YAMApiExtracter<DailySummary>.CreateStreamReaderFromUrl(reportUrl);
                var items = EnumerateRows(streamReader);
                Add(items);
            }
            End();
        }
        private IEnumerable<DailySummary> EnumerateRows(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                TDDailySummaryExtracter.SetupCSVReaderConfig(csv);
                csv.Configuration.RegisterClassMap<YAMRowMap>();

                List<YAMRow> csvRows = null;
                try
                {
                    csvRows = csv.GetRecords<YAMRow>().ToList();
                }
                catch (CsvHelperException ex)
                {
                    Logger.Error(ex);
                }
                if (csvRows != null)
                {
                    var dates = new HashSet<DateTime>(dateRange.Dates);

                    foreach (var ds in GroupByDateAndEnumerate(csvRows))
                    {
                        dates.Remove(ds.Date);
                        yield return ds;
                    }
                    // Create empty DailySummaries for any dates that weren't covered
                    foreach (var date in dates)
                    {
                        var ds = new DailySummary { Date = date };
                        yield return ds;
                    }
                }
            }
        }
        private IEnumerable<DailySummary> GroupByDateAndEnumerate(List<YAMRow> csvRows)
        {
            var groupedRows = csvRows.GroupBy(x => x.Day);
            foreach (var dayGroup in groupedRows)
            {
                var daySum = new DailySummary { Date = dayGroup.Key };
                var clickThrus = dayGroup.Where(x => x.ClickThruConvs > 0);
                foreach (var row in clickThrus)
                {
                    daySum.PostClickRev += GetConVal(row.PixelParameter);
                }
                var viewThrus = dayGroup.Where(x => x.ViewThruConvs > 0);
                foreach (var row in viewThrus)
                {
                    daySum.PostViewRev += GetConVal(row.PixelParameter);
                }
                yield return daySum;
            }
        }
    }

    public class YAMStrategyConValExtracter : YAMConValExtracter<StrategySummary>
    {
        protected readonly string[] existingStrategyNames;

        public YAMStrategyConValExtracter(YAMUtility yamUtility, DateRange dateRange, ExtAccount account, string[] existingStrategyNames = null)
            : base(yamUtility, dateRange, account)
        {
            this.existingStrategyNames = existingStrategyNames ?? new string[] { };
        }

        protected override void Extract()
        {
            var payload = _yamUtility.CreateReportRequestPayload(dateRange.FromDate, dateRange.ToDate, this.yamAdvertiserId, byPixelParameter: true, byLine: true);
            var reportUrl = _yamUtility.GenerateReport(payload);
            if (!string.IsNullOrWhiteSpace(reportUrl))
            {
                var streamReader = YAMApiExtracter<DailySummary>.CreateStreamReaderFromUrl(reportUrl);
                var items = EnumerateRows(streamReader);
                Add(items);
            }
            End();
        }
        private IEnumerable<StrategySummary> EnumerateRows(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                TDDailySummaryExtracter.SetupCSVReaderConfig(csv);
                csv.Configuration.RegisterClassMap<YAMRowMap_WithLine>();

                List<YAMRow> csvRows = null;
                try
                {
                    csvRows = csv.GetRecords<YAMRow>().ToList();
                }
                catch (CsvHelperException ex)
                {
                    Logger.Error(ex);
                }
                if (csvRows != null)
                {
                    // The dictionary key is the strategy name; The value, a HashSet, is used to see if all dates are represented for that strategy
                    var strategyHashes = new Dictionary<string, HashSet<DateTime>>();
                    foreach (var ss in GroupByDateAndLineAndEnumerate(csvRows))
                    {
                        if (!strategyHashes.ContainsKey(ss.StrategyName))
                            strategyHashes[ss.StrategyName] = new HashSet<DateTime>(dateRange.Dates);
                        strategyHashes[ss.StrategyName].Remove(ss.Date);
                        yield return ss;
                    }
                    // Create empty StrategySummaries for any dates that weren't covered - for each strategy found
                    foreach (var stratName in strategyHashes.Keys)
                    {
                        var dateHash = strategyHashes[stratName];
                        foreach (var date in dateHash)
                        {
                            var ss = new StrategySummary { Date = date, StrategyName = stratName };
                            yield return ss;
                        }
                    }
                    // Handle any strategies with SS's in the db that weren't represented in the extracted stats
                    var missingStrategies = existingStrategyNames.Where(x => !strategyHashes.Keys.Contains(x));
                    foreach (var stratName in missingStrategies)
                    {
                        foreach (var date in dateRange.Dates)
                        {
                            var ss = new StrategySummary { Date = date, StrategyName = stratName };
                            yield return ss;
                        }
                    }
                }
            }
        }
        private IEnumerable<StrategySummary> GroupByDateAndLineAndEnumerate(List<YAMRow> csvRows)
        {
            var groupedRows = csvRows.GroupBy(x => new { x.Day, x.LineName });
            foreach (var grp in groupedRows)
            {
                var stratSum = new StrategySummary { Date = grp.Key.Day, StrategyName = grp.Key.LineName };
                var clickThrus = grp.Where(x => x.ClickThruConvs > 0);
                foreach (var row in clickThrus)
                {
                    stratSum.PostClickRev += GetConVal(row.PixelParameter);
                }
                var viewThrus = grp.Where(x => x.ViewThruConvs > 0);
                foreach (var row in viewThrus)
                {
                    stratSum.PostViewRev += GetConVal(row.PixelParameter);
                }
                yield return stratSum;
            }
        }
    }

    public sealed class YAMRowMap : CsvClassMap<YAMRow>
    {
        public YAMRowMap()
        {
            Map(x => x.Day);
            Map(x => x.PixelParameter).Name("Pixel Query String");
            Map(x => x.ClickThruConvs).Name("Click Through Conversion");
            Map(x => x.ViewThruConvs).Name("View Through Conversion");
        }
    }
    public sealed class YAMRowMap_WithLine : CsvClassMap<YAMRow>
    {
        public YAMRowMap_WithLine()
        {
            Map(x => x.Day);
            Map(x => x.PixelParameter).Name("Pixel Query String");
            Map(x => x.ClickThruConvs).Name("Click Through Conversion");
            Map(x => x.ViewThruConvs).Name("View Through Conversion");
            Map(x => x.LineName).Name("Line");
            Map(x => x.LineID).Name("Line Id");
        }
    }
    public class YAMRow
    {
        public DateTime Day { get; set; }
        public string PixelParameter { get; set; }
        public int ClickThruConvs { get; set; }
        public int ViewThruConvs { get; set; }

        public string LineName { get; set; }
        public string LineID { get; set; }
    }
}
