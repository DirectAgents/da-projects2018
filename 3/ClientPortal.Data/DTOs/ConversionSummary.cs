using System;

namespace ClientPortal.Data.DTOs
{
    // Used to summarize conversions over a period of time
    public class ConversionSummary
    {
        public int? AdvertiserId { get; set; }
        public int OfferId { get; set; }
        public string OfferName { get; set; }
        public string Format { get; set; }

        public int Count { get; set; }
        public decimal Revenue { get; set; }

        //public decimal ConValTotal { get; set; }

        public string Currency
        {
            set { Culture = OfferInfo.CurrencyToCulture(value); }
        }
        public string Culture { get; set; }
    }
}
