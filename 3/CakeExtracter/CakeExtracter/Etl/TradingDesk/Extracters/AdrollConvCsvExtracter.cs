using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class AdrollConvCsvExtracter : Extracter<AdrollConvRow>
    {
        private readonly string csvFilePath;
        private readonly StreamReader streamReader;
        // if streamReader is not null, use it. otherwise use csvFilePath.

        public AdrollConvCsvExtracter(string csvFilePath, StreamReader streamReader)
        {
            this.csvFilePath = csvFilePath;
            this.streamReader = streamReader;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting Conversions from {0}", csvFilePath ?? "StreamReader");
            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<AdrollConvRow> EnumerateRows()
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

        private IEnumerable<AdrollConvRow> EnumerateRowsInner(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                csv.Configuration.IgnoreHeaderWhiteSpace = true;
                csv.Configuration.WillThrowOnMissingField = false;
                csv.Configuration.SkipEmptyRecords = true;
                csv.Configuration.RegisterClassMap<AdrollConvRowMap>();
                var csvRows = csv.GetRecords<AdrollConvRow>().ToList();
                for (int i = 0; i < csvRows.Count; i++)
                {
                    yield return csvRows[i];
                }
            }
        }
    }

    public sealed class AdrollConvRowMap : CsvClassMap<AdrollConvRow>
    {
        public AdrollConvRowMap()
        {

                Map(m => m.ConvTime).Name("ConversionTime");
                Map(m => m.ConvType).Name("ConversionType");

                Map(m => m.Campaign);
                Map(m => m.AdGroup);
                Map(m => m.Ad);
                //Map(m => m.Segment);

                Map(m => m.ConvVal).Name("ConversionValue");
                Map(m => m.Country);
                Map(m => m.City);
                //Map(m => m.FinalEvent).Name("FinalEvent");
                //Map(m => m.FinalEventTimestamp).Name("FinalEventTimestamp");

                Map(m => m.ExternalData);
                //Map(m => m.ext_data_user_id);
                Map(m => m.ext_data_order_id);
        }
    }

    public class AdrollConvRow
    {
        public DateTime ConvTime { get; set; }
        public string ConvType { get; set; }

        public string Campaign { get; set; }
        public string AdGroup { get; set; }
        public string Ad { get; set; }
        //public string Segment { get; set; }

        public string ConvVal { get; set; } // decimal
        public string Country { get; set; }
        public string City { get; set; }
        //public string FinalEvent { get; set; }
        //public string FinalEventTimestamp { get; set; }

        public string ExternalData { get; set; }
        //public string ext_data_user_id { get; set; }
        public string ext_data_order_id { get; set; }
    }
}
