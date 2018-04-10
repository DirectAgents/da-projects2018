using System.Collections.Generic;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class OfferSummaryReportResponse
    {
        public bool Success { get; set; }
        public int RowCount { get; set; }
        public List<OfferSummary> Offers { get; set; }
    }

    public class OfferSummary
    {
        public Offer1 Offer { get; set; }
        public Advertiser1 Advertiser { get; set; }
        public AccountManager1 AccountManager { get; set; }
        public int Views { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public int Paid { get; set; }
        public int Sellable { get; set; }
        public decimal Cost { get; set; }
        public decimal Revenue { get; set; }
        // ClickThruPercentage, Events...
        // Pending, Rejected, Approved, Returned, Orders...
    }
}
