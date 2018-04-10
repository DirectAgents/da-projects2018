using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon
{
    public class GetTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }

        public string refresh_token { get; set; }
    }

    public class AmazonApiReportParams
    {
        public string campaignType { get; set; }
        public string segment { get; set; }
        public string reportDate { get; set; }
        public string metrics { get; set; }
    }

    public class ReportParams
    {
        public string campaignType { get; set; }
        public string segment { get; set; }
        public string reportDate { get; set; }
        public string Metrics { get; set; }

        public string[] dimensions { get; set; }
        public string[] metrics { get; set; }
        public object filter { get; set; }
        public Paging paging { get; set; }

        public string Interface { get; set; }//used profile, campaigns, etc..different api header objects

    }
    //public class ReportFilter
    //{
    //    public Dates date { get; set; }
    //    public int[] client { get; set; }
    //    public string[] media { get; set; }
    //}
    public class Dates
    {
        public string from { get; set; }
        public string to { get; set; }
    }
    public class Paging
    {
        public int offset { get; set; }
        public int limit { get; set; }
    }

    public class ReportResponse
    {
        public ReportData reportData { get; set; }
        //public int totalRowCount { get; set; } // not being supplied by the API
        public string correlationCode { get; set; }
    }
    public class ReportRequestResponse
    {
        public string reportId { get; set; }
        public string recordType { get; set; }
        public string status { get; set; }
        public string statusDetails { get; set; }
    }
    public class ReportResponseDownloadInfo
    {
        public string reportId { get; set; }
        
        public string status { get; set; }
        public string statusDetails { get; set; }
        public string location { get; set; }
        public int fileSize { get; set; }
    }
    public class ReportData
    {
        public List<string> columnHeaders { get; set; }
        public object columns { get; set; }
        public List<List<object>> rows { get; set; }
        // totals?

        public Dictionary<string, int> CreateColumnLookup()
        {
            var columnLookup = new Dictionary<string, int>();
            for (int i = 0; i < this.columnHeaders.Count; i++)
                columnLookup[this.columnHeaders[i]] = i;
            return columnLookup;
        }
    }

    public class AmazonSummary
    {
        public DateTime Date { get; set; }
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public decimal Cost { get; set; }

        public string Campaign { get; set; }
        public string LineItem { get; set; }
        public string Banner { get; set; }

        public string AdInteractionType { get; set; }
        public int Conversions { get; set; }
        public decimal Sales { get; set; }
    }
    //public class AmazonDailySummary
    //{
    //    public string campaignId { get; set; }
    //    public string clicks { get; set; }
    //    public string impressions { get; set; }

    //    public string cost { get; set; }
    //}

}
