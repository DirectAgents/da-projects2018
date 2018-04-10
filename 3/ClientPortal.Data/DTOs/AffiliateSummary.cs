using System;

namespace ClientPortal.Data.DTOs
{
    public class AffiliateSummary
    {
        public DateTime Date { get; set; }
        public int AffId { get; set; }
        public int OfferId { get; set; }
        public string Offer { get; set; }

        public int Clicks { get; set; }
        public int Convs { get; set; } //Conversions

        public float ConvRate
        {
            get { return (Clicks == 0) ? 0 : (float)Math.Round((double)Convs / Clicks, 3); }
        }

        public decimal Price { get; set; }
        public int CurrencyId
        {
            set { Culture = OfferInfo.CurrencyIdToCulture(value); }
        }
        public string Currency
        {
            set { Culture = OfferInfo.CurrencyToCulture(value); }
        }
        public string Culture { get; set; }

        public string TransactionId { get; set; }
        public bool? Positive { get; set; }
    }
}
