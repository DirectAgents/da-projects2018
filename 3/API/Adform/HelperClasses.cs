using System;
using System.Collections.Generic;

namespace Adform
{
    public class GetTokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
    }

    public class ReportParams
    {
        public string[] dimensions { get; set; }
        public string[] metrics { get; set; }
        public object filter { get; set; }
        public Paging paging { get; set; }
        public bool includeRowCount { get; set; }
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
        public int totalRowCount { get; set; }
        public string correlationCode { get; set; }
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

    //--- asynchronous operations ---
    public class CreateJobResponse
    {
        public string OperationLocation { get; set; }
        public string Location { get; set; } // not used?
    }

    public class AdformSummary
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
}
