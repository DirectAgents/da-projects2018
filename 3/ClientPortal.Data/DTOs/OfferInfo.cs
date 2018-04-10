using System;

namespace ClientPortal.Data.DTOs
{
    public class OfferInfo
    {
        public int OfferId { get; set; }
        public string AdvertiserId { get; set; }
        public int? AdvertiserId_Int
        {
            set { this.AdvertiserId = value.ToString(); }
        }

        public string Name { get; set; }
        public string Format { get; set; }

        public int Clicks { get; set; }
        public int Conversions { get; set; }

        public float ConvRate
        {
            get { return (Clicks == 0) ? 0 : (float)Math.Round((double)Conversions / Clicks, 3); }
        }

        public decimal Revenue { get; set; }
        public decimal Price
        {
            get { return (Conversions == 0) ? 0 : Math.Round(Revenue / Conversions, 2); }
        }
        public string Currency
        {
            set { Culture = CurrencyToCulture(value); }
        }
        public string Culture { get; set; }

        public static string CurrencyIdToCulture(int currencyId)
        {
            switch (currencyId)
            {
                case 2:
                    return "de-DE";
                case 3:
                    return "en-GB";
                default:
                    return "en-US";
            }
        }
        public static string CurrencyToCulture(string currency)
        {
            switch (currency)
            {
                case "EUR":
                    return "de-DE";
                case "GBP":
                    return "en-GB";
                case "AUD":
                    return "en-AU";
                default:
                    return "en-US";
            }
        }
    }
}
