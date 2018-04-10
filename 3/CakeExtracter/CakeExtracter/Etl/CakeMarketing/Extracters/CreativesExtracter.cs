using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class CreativesExtracter : Extracter<Creative>
    {
        private readonly int offerId;

        public CreativesExtracter(int offerId)
        {
            this.offerId = offerId;
        }

        protected override void Extract()
        {
            Add(CakeMarketingUtility.Creatives(offerId));
            End();
        }
    }
}
