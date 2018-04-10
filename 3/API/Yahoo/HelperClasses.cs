
namespace Yahoo
{
    public class GetTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string xoauth_yahoo_guid { get; set; }
    }

    public class CreateReportResponse
    {
        public string customerReportId { get; set; }
        public string status { get; set; }
    }
    public class GetReportResponse
    {
        public string customerReportId { get; set; }
        public string status { get; set; }
        public string url { get; set; }
    }

    public class ReportPayload
    {
        public ReportOption reportOption { get; set; }
        public int intervalTypeId { get; set; }
        public int dateTypeId { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
    }
    public class ReportOption
    {
        public string timezone { get; set; }
        public int currency { get; set; }
        public int[] accountIds { get; set; }
        public int[] dimensionTypeIds { get; set; }
        public int[] metricTypeIds { get; set; }
        // filterOptions, having, limitSpec
    }
}
