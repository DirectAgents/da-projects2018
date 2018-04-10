using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class TDadSummaryExtracter : Extracter<TDadSummary>
    {
        // if streamReader is not null, use it. otherwise use csvFilePath.
        private readonly StreamReader streamReader;
        private readonly string csvFilePath;

        private readonly ColumnMapping columnMapping;

        public TDadSummaryExtracter(ColumnMapping columnMapping, StreamReader streamReader = null, string csvFilePath = null)
        {
            this.columnMapping = columnMapping;
            this.streamReader = streamReader;
            this.csvFilePath = csvFilePath;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting TDadSummaries from {0}", csvFilePath ?? "StreamReader");
            var items = EnumerateRows();
            Add(items);
            End();
        }

        public IEnumerable<TDadSummary> EnumerateRows()
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

        private IEnumerable<TDadSummary> EnumerateRowsInner(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                TDDailySummaryExtracter.SetupCSVReaderConfig(csv);

                var classMap = CreateCsvClassMap(this.columnMapping);
                csv.Configuration.RegisterClassMap(classMap);

                List<TDadSummary> csvRows = null;
                try
                {
                    csvRows = csv.GetRecords<TDadSummary>().ToList();
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

        private IEnumerable<TDadSummary> GroupAndEnumerate(List<TDadSummary> csvRows)
        {
            // if TDadEid's aren't all filled in...
            if (csvRows.Any(r => string.IsNullOrWhiteSpace(r.TDadEid)))
            {
                var groupedRows = csvRows.GroupBy(r => new { r.Date, r.TDadEid, r.TDadName });
                foreach (var group in groupedRows)
                {
                    var sum = new TDadSummary
                    {
                        Date = group.Key.Date,
                        TDadEid = group.Key.TDadEid,
                        TDadName = group.Key.TDadName,
                        //,Width = group.First().Width
                    };
                    sum.SetStats(group);
                    yield return sum;
                }
            }
            else // if all TDadEid's are filled in...
            {
                var groupedRows = csvRows.GroupBy(r => new { r.Date, r.TDadEid });
                foreach (var group in groupedRows)
                {
                    string tdAdName = null;
                    if (group.Count() == 1)
                        tdAdName = group.First().TDadName;
                    var sum = new TDadSummary
                    {
                        Date = group.Key.Date,
                        TDadEid = group.Key.TDadEid,
                        TDadName = tdAdName,
                        //,Width = group.First().Width
                    };
                    sum.SetStats(group);
                    yield return sum;
                }
            }
        }

        private CsvClassMap CreateCsvClassMap(ColumnMapping colMap)
        {
            var classMap = new DefaultCsvClassMap<TDadSummary>();
            Type classType = typeof(TDadSummary);
            TDDailySummaryExtracter.AddBasicPropertyMaps(classMap, classType, colMap);
            TDDailySummaryExtracter.CheckAddPropertyMap(classMap, classType, "TDadName", colMap.TDadName);
            TDDailySummaryExtracter.CheckAddPropertyMap(classMap, classType, "TDadEid", colMap.TDadEid);
            return classMap;
        }
    }
}
