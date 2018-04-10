using System.Configuration;
using System.IO;
using System.Net;
using CakeExtracter.Common;
using DirectAgents.Domain.Entities.CPProg;
using Yahoo;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public abstract class YAMApiExtracter<T> : Extracter<T>
    {
        protected readonly YAMUtility _yamUtility;
        protected readonly DateRange dateRange;
        protected readonly ColumnMapping columnMapping;
        protected readonly int yamAdvertiserId;

        public YAMApiExtracter(YAMUtility yamUtility, DateRange dateRange, ExtAccount account)
        {
            this._yamUtility = yamUtility;
            this.dateRange = dateRange;
            this.yamAdvertiserId = int.Parse(account.ExternalId);
            this.columnMapping = account.Platform.PlatColMapping;
            if (columnMapping != null)
            { //These override the colMappings b/c the API has different col headers than the UI-generated reports
                string mapVal = ConfigurationManager.AppSettings["YAMMap_PostClickConv"];
                if (mapVal != null)
                    columnMapping.PostClickConv = mapVal;
                mapVal = ConfigurationManager.AppSettings["YAMMap_PostViewConv"];
                if (mapVal != null)
                    columnMapping.PostViewConv = mapVal;
                mapVal = ConfigurationManager.AppSettings["YAMMap_PostClickRev"];
                if (mapVal != null)
                    columnMapping.PostClickRev = mapVal;
                mapVal = ConfigurationManager.AppSettings["YAMMap_PostViewRev"];
                if (mapVal != null)
                    columnMapping.PostViewRev = mapVal;
                mapVal = ConfigurationManager.AppSettings["YAMMap_StrategyName"];
                if (mapVal != null)
                    columnMapping.StrategyName = mapVal;
                mapVal = ConfigurationManager.AppSettings["YAMMap_CreativeName"];
                if (mapVal != null)
                    columnMapping.TDadName = mapVal;
            }
        }

        public static StreamReader CreateStreamReaderFromUrl(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            var streamReader = new StreamReader(responseStream);
            return streamReader;
        }
    }

    public class YAMDailySummaryExtracter : YAMApiExtracter<DailySummary>
    {
        public YAMDailySummaryExtracter(YAMUtility yamUtility, DateRange dateRange, ExtAccount account)
            : base(yamUtility, dateRange, account)
        { }

        protected override void Extract()
        {
            var payload = _yamUtility.CreateReportRequestPayload(dateRange.FromDate, dateRange.ToDate, this.yamAdvertiserId);
            var reportUrl = _yamUtility.GenerateReport(payload);

            if (!string.IsNullOrWhiteSpace(reportUrl))
            {
                var streamReader = CreateStreamReaderFromUrl(reportUrl);
                var tdExtracter = new TDDailySummaryExtracter(this.columnMapping, streamReader: streamReader);
                var items = tdExtracter.EnumerateRows();
                Add(items);
            }
            End();
        }
    }

    public class YAMStrategySummaryExtracter : YAMApiExtracter<StrategySummary>
    {
        public YAMStrategySummaryExtracter(YAMUtility yamUtility, DateRange dateRange, ExtAccount account)
            : base(yamUtility, dateRange, account)
        { }

        protected override void Extract()
        {
            var payload = _yamUtility.CreateReportRequestPayload(dateRange.FromDate, dateRange.ToDate, this.yamAdvertiserId, byLine: true);
            var reportUrl = _yamUtility.GenerateReport(payload);

            if (!string.IsNullOrWhiteSpace(reportUrl))
            {
                var streamReader = CreateStreamReaderFromUrl(reportUrl);
                var tdExtracter = new TDStrategySummaryExtracter(this.columnMapping, streamReader: streamReader);
                var items = tdExtracter.EnumerateRows();
                Add(items);
            }
            End();
        }
    }

    public class YAMTDadSummaryExtracter : YAMApiExtracter<TDadSummary>
    {
        public YAMTDadSummaryExtracter(YAMUtility yamUtility, DateRange dateRange, ExtAccount account)
            : base(yamUtility, dateRange, account)
        { }

        protected override void Extract()
        {
            var payload = _yamUtility.CreateReportRequestPayload(dateRange.FromDate, dateRange.ToDate, this.yamAdvertiserId, byAd: true);
            var reportUrl = _yamUtility.GenerateReport(payload);

            if (!string.IsNullOrWhiteSpace(reportUrl))
            {
                var streamReader = CreateStreamReaderFromUrl(reportUrl);
                var tdExtracter = new TDadSummaryExtracter(this.columnMapping, streamReader: streamReader);
                var items = tdExtracter.EnumerateRows();
                Add(items);
            }
            End();
        }
    }
}
