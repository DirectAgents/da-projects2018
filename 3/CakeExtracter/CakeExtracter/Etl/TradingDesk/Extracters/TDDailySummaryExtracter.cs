using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class TDDailySummaryExtracter : Extracter<DailySummary>
    {
        // if streamReader is not null, use it. otherwise use csvFilePath.
        private readonly StreamReader streamReader;
        private readonly string csvFilePath;

        private readonly ColumnMapping columnMapping;

        public TDDailySummaryExtracter(ColumnMapping columnMapping, StreamReader streamReader = null, string csvFilePath = null)
        {
            this.columnMapping = columnMapping;
            this.streamReader = streamReader;
            this.csvFilePath = csvFilePath;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting DailySummaries from {0}", csvFilePath ?? "StreamReader");
            var items = EnumerateRows();
            Add(items);
            End();
        }

        public IEnumerable<DailySummary> EnumerateRows()
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

        public static void SetupCSVReaderConfig(CsvReader csv)
        {
            csv.Configuration.SkipEmptyRecords = true; //ShouldSkipRecord is checked first. if false, will check if it's empty (all fields)
            csv.Configuration.ShouldSkipRecord = delegate(string[] fields)
            { // assume 0 == the column number for the date field
                if (String.IsNullOrWhiteSpace(fields[0]))
                    return true;
                //if (fields[0] != null && fields[0].ToUpper().StartsWith("TOTAL"))
                //    return true; // should be handled by the ReadingExceptionCallback
                return false;
            };
            csv.Configuration.WillThrowOnMissingField = false;
            csv.Configuration.IgnoreReadingExceptions = true; // This is at the row level
            csv.Configuration.ReadingExceptionCallback = (ex, row) =>
            {
                Logger.Error(ex);
            };
        }
        private IEnumerable<DailySummary> EnumerateRowsInner(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                SetupCSVReaderConfig(csv);

                var classMap = CreateCsvClassMap(this.columnMapping);
                csv.Configuration.RegisterClassMap(classMap);

                List<DailySummary> csvRows = null;
                try
                {
                    csvRows = csv.GetRecords<DailySummary>().ToList();
                }
                catch (CsvHelperException ex)
                {
                    Logger.Error(ex);
                }
                if (csvRows != null)
                {
                    foreach (var ds in GroupByDateAndEnumerate(csvRows))
                        yield return ds;
                }
            }
        }

        private IEnumerable<DailySummary> GroupByDateAndEnumerate(List<DailySummary> csvRows)
        {
            var groupedRows = csvRows.GroupBy(r => r.Date);
            foreach (var dayGroup in groupedRows)
            {
                var ds = new DailySummary { Date = dayGroup.Key };
                ds.SetStats(dayGroup);
                yield return ds;
            }
        }

        private CsvClassMap CreateCsvClassMap(ColumnMapping colMap)
        {
            var classMap = new DefaultCsvClassMap<DailySummary>();
            AddBasicPropertyMaps(classMap, typeof(DailySummary), colMap);
            return classMap;
        }

        public static void AddBasicPropertyMaps(CsvClassMap classMap, Type classType, ColumnMapping colMap)
        {
            CheckAddPropertyMap(classMap, classType, "Date", colMap.Date);
            CheckAddPropertyMap(classMap, classType, "Cost", colMap.Cost);
            CheckAddPropertyMap(classMap, classType, "Impressions", colMap.Impressions);
            CheckAddPropertyMap(classMap, classType, "Clicks", colMap.Clicks);
            CheckAddPropertyMap(classMap, classType, "PostClickConv", colMap.PostClickConv);
            CheckAddPropertyMap(classMap, classType, "PostViewConv", colMap.PostViewConv);
            CheckAddPropertyMap(classMap, classType, "PostClickRev", colMap.PostClickRev);
            CheckAddPropertyMap(classMap, classType, "PostViewRev", colMap.PostViewRev);
        }
        public static void CheckAddPropertyMap(CsvClassMap classMap, Type classType, string propName, string colName)
        {
            if (!string.IsNullOrWhiteSpace(colName))
            {
                var csvPropertyMap = CreatePropertyMap(classType, propName, colName);
                if (csvPropertyMap != null)
                    classMap.PropertyMaps.Add(csvPropertyMap);
            }
        }
        private static CsvPropertyMap CreatePropertyMap(Type classType, string propName, string colName)
        {
            var propertyInfo = classType.GetProperty(propName);
            if (propertyInfo == null) // the property doesn't exist
                return null;

            var propMap = new CsvPropertyMap(propertyInfo);
            propMap.Name(colName);
            if (propertyInfo.PropertyType == typeof(int) ||
                propertyInfo.PropertyType == typeof(decimal))
            {
                propMap.TypeConverterOption(NumberStyles.Currency | NumberStyles.AllowExponent);
                propMap.Default(0);
            }
            return propMap;
        }
    }
}
