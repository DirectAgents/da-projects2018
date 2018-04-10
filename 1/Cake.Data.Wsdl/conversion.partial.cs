namespace Cake.Data.Wsdl.ReportsService
{
    public partial class conversion
    {
        public int Affiliate_Id
        {
            get
            {
                return this.affiliate.affiliate_id;
            }
        }

        public int Offer_Id
        {
            get
            {
                return this.offer.offer_id;
            }
        }

        public int Advertiser_Id
        {
            get
            {
                return this.advertiser.advertiser_id;
            }
        }

        public int Creative_Id
        {
            get
            {
                return this.creative.creative_id;
            }
        }

        public string CreativeName
        {
            get
            {
                return this.creative.creative_name;
            }
        }

        public decimal PricePaid
        {
            get
            {
                return this.paid.amount;
            }
        }

        public int PricePaidCurrencyId
        {
            get
            {
                return this.paid.currency_id;
            }
        }

        public string PricePaidFormattedAmount
        {
            get
            {
                return this.paid.formatted_amount;
            }
        }

        public decimal PriceReceived
        {
            get
            {
                return this.received.amount;
            }
        }

        public int PriceReceivedCurrencyId
        {
            get
            {
                return this.received.currency_id;
            }
        }

        public string PriceReceivedFormattedAmount
        {
            get
            {
                return this.received.formatted_amount;
            }
        }
    }
}
