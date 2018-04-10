using DirectAgents.Domain.Entities.Cake;
namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class Campaign
    {
        //NAME: Offer.OfferName [- OfferContract.OfferContractName] - OfferContract.PriceFormat.PriceFormatName - Payout.FormattedAmount
        // "Payout.FormattedAmount" could instead use: Currency.CurrencySymbol and Payout.Amount (X.XX) ?

        public int CampaignId { get; set; }
        //CampaignType
        public Affiliate1 Affiliate { get; set; }
        public Offer1 Offer { get; set; }
        public OfferContractInfo OfferContract { get; set; }
        //etc
        public Payout Payout { get; set; }
        public Currency Currency { get; set; }
        //etc

        // copy everything except PK
        public void CopyValuesTo(Camp camp)
        {
            camp.AffiliateId = (this.Affiliate != null) ? this.Affiliate.AffiliateId : 0;
            camp.OfferId = (this.Offer != null) ? this.Offer.OfferId : 0;
            camp.OfferContractId = (this.OfferContract != null) ? this.OfferContract.OfferContractId : 0;
            camp.PayoutAmount = (this.Payout != null) ? this.Payout.Amount : 0;
            camp.CurrencyAbbr = (this.Currency != null) ? this.Currency.CurrencyAbbr : null;
        }
    }
}
