using System;
using System.Collections.Generic;

namespace Apple
{
    public class ReportRequest
    {
        public string startTime { get; set; }
        public string endTime { get; set; }
        // timeZone
        // groupBy
        public Selector selector { get; set; }
        public string granularity { get; set; } // HOURLY, DAILY, WEEKLY, MONTHLY
        //public bool returnRowTotals { get; set; }
        public bool returnRecordsWithNoMetrics { get; set; }
    }
    public class Selector
    {
        //conditions
        //fields
        public Sel_OrderBy[] orderBy { get; set; }
        public Sel_Pagination pagination { get; set; }
    }
    public class Sel_OrderBy
    {
        public string field { get; set; } //e.g. campaignName
        public string sortOrder { get; set; } // "ASCENDING" or "DESCENDING"
    }
    public class Sel_Pagination
    {
        public int limit { get; set; }
        public int offset { get; set; }
    }

    public class AppleResponse
    {
        public object data { get; set; }
        public Pagination pagination { get; set; }
        public object error { get; set; }
    }
    public class AppleReportResponse
    {
        public ReportData data { get; set; }
        public Pagination pagination { get; set; }
        public object error { get; set; }
    }
    public class Pagination
    {
        public int totalResults { get; set; }
        public int startIndex { get; set; }
        public int itemsPerPage { get; set; }
    }

    public class ReportData
    {
        public ReportingResponse reportingDataResponse { get; set; }
    }
    public class ReportingResponse
    {
        public List<AppleStatGroup> row { get; set; }
    }
    public class AppleStatGroup
    {
        public List<AppleStat> granularity { get; set; }
        public Metadata metadata { get; set; }
        public object total { get; set; }
        public object other { get; set; } // true/false
    }

    public class AppleStat : AppleStatBase
    {
        public DateTime date { get; set; }
    }
    public class AppleStatBase
    {
        public int impressions { get; set; }
        public int taps { get; set; }
        public int conversions { get; set; }
        // (decimal) ttr
        // (CurrAmount) avgCPA
        // (CurrAmount) avgCPT
        public CurrAmount localSpend { get; set; }
        // (decimal) conversionRate
    }
    public class CurrAmount
    {
        public decimal amount { get; set; }
        public string currency { get; set; }
    }

    public class Metadata
    {
        public string campaignId { get; set; }
        public string campaignName { get; set; }
        // (CurrAmount) totalBudget
        // (CurrAmount) dailyBudget
        // campaignStatus
        // servingStatus
        // servingStateReasons
        // storefront
        // app -> { appName, adamId }
        // (DateTime) modificationTime
        // displayStatus
    }
}
