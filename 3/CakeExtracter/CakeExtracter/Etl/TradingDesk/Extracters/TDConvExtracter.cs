using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{

    public class TDConvExtracter : Extracter<ConvRow>
    {
        private readonly string csvFilePath;
        private readonly StreamReader streamReader;
        private readonly string platformCode;
        // if streamReader is not null, use it. otherwise use csvFilePath.

        public TDConvExtracter(string csvFilePath, StreamReader streamReader, string platCode)
        {
            this.csvFilePath = csvFilePath;
            this.streamReader = streamReader;
            this.platformCode = platCode;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting Conversions from {0}", csvFilePath ?? "StreamReader");
            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<ConvRow> EnumerateRows()
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

        private IEnumerable<ConvRow> EnumerateRowsInner(StreamReader reader)
        {
            using (CsvReader csv = new CsvReader(reader))
            {
                csv.Configuration.IgnoreHeaderWhiteSpace = true;
                csv.Configuration.WillThrowOnMissingField = false;
                csv.Configuration.SkipEmptyRecords = true;
                switch(platformCode) {
                    case Platform.Code_AdRoll:
                        csv.Configuration.RegisterClassMap<TestAdrollConvRowMap>();
                        break;
                    case Platform.Code_DBM:
                        csv.Configuration.RegisterClassMap<ConvRowMap>();
                        break;
                    default:
                        csv.Configuration.RegisterClassMap<TestAdrollConvRowMap>();
                        break;
                }
                var csvRows = csv.GetRecords<ConvRow>().ToList();
                for (int i = 0; i < csvRows.Count; i++)
                {
                    yield return csvRows[i];
                }
            }
        }
    }

    //temporary mappings until we start using PlatColMappings

    public sealed class ConvRowMap : CsvClassMap<ConvRow>
    {
        public ConvRowMap()
        {
            Map(m => m.Campaign).Name("InsertionOrder");
            Map(m => m.ConvTime).Name("Date");
            Map(m => m.ext_data_order_id).Name("OrderID");
            Map(m => m.ConvVal).Name("DCMPost-ViewRevenue");
            Map(m => m.PostClickConvs).Name("Post-ClickConversions");

             /*
            Map(m => m.AdGroup);
            Map(m => m.Ad);
            //Map(m => m.Segment);

            Map(m => m.ConvVal).Name("ConversionValue");
            Map(m => m.Country);
            Map(m => m.City);
            //Map(m => m.FinalEvent).Name("FinalEvent");
            //Map(m => m.FinalEventTimestamp).Name("FinalEventTimestamp");

            Map(m => m.ExternalData);*/
        }
    }

    public sealed class TestAdrollConvRowMap : CsvClassMap<ConvRow>
    {
        public TestAdrollConvRowMap()
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
    
    public class ConvRow
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

        public string Platform { get; set; }
        public int PostClickConvs { get; set; }
    }
}
