namespace CakeExtracter.CakeMarketingApi.Entities
{
    public class Payout
    {
        public bool IsPercentage { get; set; }
        public decimal Amount { get; set; }
        public string FormattedAmount { get; set; }
    }
}
