using System;
using System.Collections.Generic;
using DirectAgents.Domain.Entities.Cake;

namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class CampaignSummaryResponse
    {
        public bool Success { get; set; }
        public int RowCount { get; set; }
        public List<CampaignSummary> Campaigns { get; set; }
    }

    public class CampaignSummary
    {
        public DateTime Date { get; set; }

        public Campaign1 Campaign { get; set; }
        public SourceAffiliate1 SourceAffiliate { get; set; }
        public SiteOffer1 SiteOffer { get; set; }
        public BrandAdvertiser1 BrandAdvertiser { get; set; }
        public AccountManager1 BrandAdvertiserManager { get; set; }
        public string PriceFormat { get; set; }
        // MediaType(e.g. Email)
        public int Views { get; set; }
        public int Clicks { get; set; }
        public decimal MacroEventConversions { get; set; }
        public decimal Paid { get; set; }
        public decimal Sellable { get; set; }
        public decimal Cost { get; set; }
        public decimal Revenue { get; set; }
        // ClickThruPercentage, Events...
        // Pending, Rejected, Approved, Returned, Orders...

        public bool AllZeros()
        {
            return (Views == 0 && Clicks == 0 && MacroEventConversions == 0 && Paid == 0 && Sellable == 0 && Cost == 0 && Revenue == 0);
        }

        public void CopyStatsFrom(CampaignSummary cs)
        {
            Views = cs.Views;
            Clicks = cs.Clicks;
            MacroEventConversions = cs.MacroEventConversions;
            Paid = cs.Paid;
            Sellable = cs.Sellable;
            Cost = cs.Cost;
            Revenue = cs.Revenue;
        }

        // copy everything expect the primary key
        public void CopyValuesTo(CampSum campSum)
        {
            campSum.OfferId = this.SiteOffer.SiteOfferId;
            campSum.AffId = this.SourceAffiliate.SourceAffiliateId;
            campSum.Views = this.Views;
            campSum.Clicks = this.Clicks;
            campSum.Conversions = this.MacroEventConversions;
            campSum.Paid = this.Paid;
            campSum.Sellable = this.Sellable;
            campSum.Revenue = this.Revenue;
            campSum.Cost = this.Cost;
            campSum.PriceFormat = this.PriceFormat;
            campSum.Units = (this.PriceFormat == "CPC" ? this.Sellable : this.Paid);
        }
    }
}
