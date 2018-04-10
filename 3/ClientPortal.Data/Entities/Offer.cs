using ClientPortal.Data.Contexts;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ClientPortal.Data.Contexts
{
    public partial class Offer
    {
        [NotMapped]
        public string DisplayName
        {
            get { return OfferName + " (" + OfferId + ")"; }
        }

        [NotMapped]
        public Advertiser Advertiser { get; set; }

        public Offer ThisWithAdvertiserInfo(int advertiserId, string advertiserName, CakeContact accountManager)
        {
            this.Advertiser = new Advertiser
            {
                AdvertiserId = advertiserId,
                AdvertiserName = advertiserName,
                AccountManagerId = (accountManager != null) ? accountManager.CakeContactId : (int?)null,
                AccountManager = accountManager
            };
            return this;
        }

        public IOrderedEnumerable<Creative> CreativesByDate()
        {
            return this.Creatives.OrderByDescending(c => c.DateCreated);
        }

        // chronological: true==oldest-to-newest, false==newest-to-oldest, null==not-ordered
        public IEnumerable<CampaignDrop> AllCampaignDrops(bool? chronological)
        {
            var drops = Campaigns.SelectMany(c => c.CampaignDrops_Originals);
            if (chronological.HasValue)
            {
                if (chronological.Value)
                    drops = drops.OrderBy(cd => cd.Date);
                else
                    drops = drops.OrderByDescending(cd => cd.Date);
            }
            return drops;
        }
    }
}
