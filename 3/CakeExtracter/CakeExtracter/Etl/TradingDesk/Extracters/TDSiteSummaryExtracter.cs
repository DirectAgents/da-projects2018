using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class TDSiteSummaryExtracter : Extracter<SiteSummary>
    {
        // if streamReader is not null, use it. otherwise use csvFilePath.
        private readonly StreamReader streamReader;
        private readonly string csvFilePath;

        private readonly ColumnMapping columnMapping;
        private readonly DateTime? dateOverride;

        public TDSiteSummaryExtracter(ColumnMapping columnMapping, DateTime? dateOverride = null, StreamReader streamReader = null, string csvFilePath = null)
        {
            this.columnMapping = columnMapping;
            this.dateOverride = dateOverride;
            this.streamReader = streamReader;
            this.csvFilePath = csvFilePath;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting SiteSummaries from {0}", csvFilePath ?? "StreamReader");
            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<SiteSummary> EnumerateRows()
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

        private IEnumerable<SiteSummary> EnumerateRowsInner(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                csv.Configuration.SkipEmptyRecords = true;
                csv.Configuration.WillThrowOnMissingField = false;
                csv.Configuration.IgnoreReadingExceptions = true; // This is at the row level
                csv.Configuration.ReadingExceptionCallback = (ex, row) =>
                {
                    Logger.Error(ex);
                };

                var classMap = CreateCsvClassMap(this.columnMapping);
                csv.Configuration.RegisterClassMap(classMap);

                List<SiteSummary> csvRows = null;
                try
                {
                    csvRows = csv.GetRecords<SiteSummary>().ToList();
                }
                catch (CsvHelperException ex)
                {
                    Logger.Error(ex);
                }
                if (csvRows != null)
                {
                    foreach (var sum in ProcessAndEnumerate(csvRows))
                        yield return sum;
                }
            }
        }

        private IEnumerable<SiteSummary> ProcessAndEnumerate(List<SiteSummary> csvRows)
        {
            // NOTE: assume that if the Date field is assigned, it's set to the 1st of each month

            if (this.dateOverride.HasValue)
            {
                foreach (var row in csvRows)
                    row.Date = dateOverride.Value;
            }
            var groupedRows = csvRows.GroupBy(r => new { r.Date, r.SiteName });
            foreach (var group in groupedRows)
            {
                var sum = new SiteSummary
                {
                    Date = group.Key.Date,
                    SiteName = group.Key.SiteName
                };
                sum.SetStats(group);
                yield return sum;
            }
        }

        private CsvClassMap CreateCsvClassMap(ColumnMapping colMap)
        {
            var classMap = new DefaultCsvClassMap<SiteSummary>();
            Type classType = typeof(SiteSummary);
            TDDailySummaryExtracter.AddBasicPropertyMaps(classMap, classType, colMap);
            TDDailySummaryExtracter.CheckAddPropertyMap(classMap, classType, "SiteName", colMap.SiteName);
            TDDailySummaryExtracter.CheckAddPropertyMap(classMap, classType, "Date", colMap.Month);
            return classMap;
        }
    }
}
