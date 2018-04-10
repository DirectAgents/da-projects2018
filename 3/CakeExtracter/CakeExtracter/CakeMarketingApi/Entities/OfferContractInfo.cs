namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class OfferContractInfo
    {
        public int OfferContractId { get; set; }
        public string OfferContractName { get; set; }
        public PriceFormat PriceFormat { get; set; }
        public Received1 Received { get; set; }
    }

    public class Received1
    {
        public bool IsPercentage { get; set; }
        public decimal Amount { get; set; }
        public string FormattedAmount { get; set; }
    }
}
