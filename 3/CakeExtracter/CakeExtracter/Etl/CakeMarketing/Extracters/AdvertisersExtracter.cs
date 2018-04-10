using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class AdvertisersExtracter : Extracter<Advertiser>
    {
        private readonly int advertiserId; // 0 for all advertisers

        public AdvertisersExtracter(int advertiserId)
        {
            this.advertiserId = advertiserId;
        }

        protected override void Extract()
        {
            Add(CakeMarketingUtility.Advertisers(advertiserId));
            End();
        }
    }
}
