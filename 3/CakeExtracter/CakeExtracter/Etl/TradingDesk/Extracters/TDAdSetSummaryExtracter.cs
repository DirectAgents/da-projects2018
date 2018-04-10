using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class TDAdSetSummaryExtracter : Extracter<AdSetSummary>
    {
        // if streamReader is not null, use it. otherwise use csvFilePath.
        private readonly StreamReader streamReader;
        private readonly string csvFilePath;

        private readonly ColumnMapping columnMapping;

        public TDAdSetSummaryExtracter(ColumnMapping columnMapping, StreamReader streamReader = null, string csvFilePath = null)
        {
            this.columnMapping = columnMapping;
            this.streamReader = streamReader;
            this.csvFilePath = csvFilePath;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting AdSetSummaries from {0}", csvFilePath ?? "StreamReader");
            var items = EnumerateRows();
            Add(items);
            End();
        }

        public IEnumerable<AdSetSummary> EnumerateRows()
        {
            if (this.streamReader != null)
            {
                foreach (var row in EnumerateRowsInner(this.streamReader))
                    yield return row;
            }
            else if (File.Exists(this.csvFilePath))
            {
                using (StreamReader reader = File.OpenText(this.csvFilePath))
                {
                    foreach (var row in EnumerateRowsInner(reader))
                        yield return row;
                }
            }
        }

        private IEnumerable<AdSetSummary> EnumerateRowsInner(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                TDDailySummaryExtracter.SetupCSVReaderConfig(csv);

                var classMap = CreateCsvClassMap(this.columnMapping);
                csv.Configuration.RegisterClassMap(classMap);

                List<AdSetSummary> csvRows = null;
                try
                {
                    csvRows = csv.GetRecords<AdSetSummary>().ToList();
                }
                catch (CsvHelperException ex)
                {
                    Logger.Error(ex);
                }
                if (csvRows != null)
                {
                    foreach (var sum in GroupAndEnumerate(csvRows))
                        yield return sum;
                }
            }
        }

        private IEnumerable<AdSetSummary> GroupAndEnumerate(List<AdSetSummary> csvRows)
        {
            // if AdSetEid's aren't all filled in...
            if (csvRows.Any(r => string.IsNullOrWhiteSpace(r.AdSetEid)))
            {
                var groupedRows = csvRows.GroupBy(r => new { r.Date, r.AdSetEid, r.AdSetName });
                foreach (var group in groupedRows)
                {
                    var sum = new AdSetSummary
                    {
                        Date = group.Key.Date,
                        AdSetEid = group.Key.AdSetEid,
                        AdSetName = group.Key.AdSetName
                    };
                    sum.SetStats(group);
                    yield return sum;
                }
            }
            else // if all AdSetEid's are filled in...
            {
                var groupedRows = csvRows.GroupBy(r => new { r.Date, r.AdSetEid });
                foreach (var group in groupedRows)
                {
                    var sum = new AdSetSummary
                    {
                        Date = group.Key.Date,
                        AdSetEid = group.Key.AdSetEid,
                        AdSetName = AdSetNameIfSame(group),
                        StrategyEid = StrategyEidIfSame(group),
                        StrategyName = StrategyNameIfSame(group)
                    };
                    sum.SetStats(group);
                    yield return sum;
                }
            }
        }
        public static string AdSetNameIfSame(IEnumerable<AdSetSummary> stats)
        {
            var adsetNames = stats.Select(x => x.AdSetName).Distinct().ToList();
            if (adsetNames.Count() == 1)
                return adsetNames.First();
            return null;
        }
        public static string StrategyEidIfSame(IEnumerable<AdSetSummary> stats)
        {
            var stratEids = stats.Select(x => x.StrategyEid).Distinct().ToList();
            if (stratEids.Count() == 1)
                return stratEids.First();
            return null;
        }
        public static string StrategyNameIfSame(IEnumerable<AdSetSummary> stats)
        {
            var stratNames = stats.Select(x => x.StrategyName).Distinct().ToList();
            if (stratNames.Count() == 1)
                return stratNames.First();
            return null;
        }

        private CsvClassMap CreateCsvClassMap(ColumnMapping colMap)
        {
            var classMap = new DefaultCsvClassMap<AdSetSummary>();
            Type classType = typeof(AdSetSummary);
            TDDailySummaryExtracter.AddBasicPropertyMaps(classMap, classType, colMap);
            TDDailySummaryExtracter.CheckAddPropertyMap(classMap, classType, "StrategyName", colMap.StrategyName); //optional ?
            TDDailySummaryExtracter.CheckAddPropertyMap(classMap, classType, "StrategyEid", colMap.StrategyEid); //optional ?
            TDDailySummaryExtracter.CheckAddPropertyMap(classMap, classType, "AdSetName", colMap.AdSetName);
            TDDailySummaryExtracter.CheckAddPropertyMap(classMap, classType, "AdSetEid", colMap.AdSetEid);
            return classMap;
        }
    }
}
