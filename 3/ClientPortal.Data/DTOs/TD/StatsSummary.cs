using System;

namespace ClientPortal.Data.DTOs.TD
{
    public class StatsSummary : StatsSummaryBase
    {
        public DateTime Date { get; set; }
    }

    public class StatsSummaryBase
    {
        //public string Currency { get; set; }
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public decimal Spend { get; set; } // note: we're using 6 decimal places in the db

        // Computed properties
        public double CTR
        {
            get { return (Impressions == 0) ? 0 : Math.Round((double)Clicks / Impressions, 5); }
        }
        public double ConvRate
        {
            get { return (Clicks == 0) ? 0 : Math.Round((double)Conversions / Clicks, 4); }
        }

        public decimal CPM
        {
            get { return (Impressions == 0) ? 0 : Math.Round(1000 * Spend / Impressions, 3); }
        }
        public decimal CPC
        {
            get { return (Clicks == 0) ? 0 : Math.Round(Spend / Clicks, 3); }
        }
        public decimal CPA
        {
            get { return (Conversions == 0) ? 0 : Math.Round(Spend / Conversions, 3); }
        }
    }
}
