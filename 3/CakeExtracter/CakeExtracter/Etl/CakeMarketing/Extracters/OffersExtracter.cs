using System.Collections.Generic;
using System.Linq;
using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class OffersExtracter : Extracter<Offer>
    {
        private readonly int advertiserId; // 0 for all advertisers
        private readonly IEnumerable<int> offerIds;

        public OffersExtracter(int advertiserId = 0, IEnumerable<int> offerIds = null)
        {
            this.advertiserId = advertiserId;
            this.offerIds = offerIds;
        }
        protected override void Extract()
        {
            Logger.Info("Extracting Offers, AdvId {0}, {1} OfferIds specified",
                advertiserId, (offerIds == null) ? "no" : offerIds.Count().ToString());
            if (offerIds == null)
            {
                var offers = CakeMarketingUtility.Offers(advertiserId: advertiserId);
                Add(offers);
            }
            else
            {
                foreach (int offerId in offerIds)
                {
                    var offers = CakeMarketingUtility.Offers(advertiserId: advertiserId, offerId: offerId);
                    Add(offers);
                }
            }
            End();
        }
    }
}
