using System;

namespace ClientPortal.Data.Entities.TD.AdRoll
{
    public class AdDailySummary
    {
        public DateTime Date { get; set; }
        public int AdRollAdId { get; set; }
        public virtual AdRollAd AdRollAd { get; set; }

        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public decimal Spend { get; set; }
    }
}
