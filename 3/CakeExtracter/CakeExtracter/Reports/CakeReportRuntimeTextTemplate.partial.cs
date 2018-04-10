namespace CakeExtracter.Reports
{
    public partial class CakeReportRuntimeTextTemplate : CakeReportRuntimeTextTemplateBase
    {
        public string AdvertiserName { get; set; }

        public string Week { get; set; }

        public int? Clicks { get; set; }

        public int? Leads { get; set; }

        public double? Rate { get; set; }

        public decimal? Spend { get; set; }

        public string Conv { get; set; }

        public string ConversionValueName { get; set; }

        public string AcctMgrName { get; set; }

        public string AcctMgrEmail { get; set; }

        public string Currency(decimal? val)
        {
            return (val != null) ? val.Value.ToString("C") : "";
        }
        public string Number(int? val)
        {
            return (val != null) ? val.Value.ToString("N0") : "";
        }
    }
}
