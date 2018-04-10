using System.Collections.Generic;
using System.Linq;
using CakeExtracter.CakeMarketingApi;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.Extracters
{
    public class AffiliatesExtracter : Extracter<Affiliate>
    {
        private readonly IEnumerable<int> affiliateIds;

        public AffiliatesExtracter(IEnumerable<int> affiliateIds = null)
        {
            this.affiliateIds = affiliateIds;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting Affiliates, {0} AffiliateIds specified",
                (affiliateIds == null) ? "no" : affiliateIds.Count().ToString());
            if (affiliateIds == null)
            {
                var affiliates = CakeMarketingUtility.Affiliates();
                Add(affiliates);
            }
            else
            {
                foreach (int affiliateId in affiliateIds)
                {
                    var affiliates = CakeMarketingUtility.Affiliates(affiliateId: affiliateId);
                    Add(affiliates);
                }
            }
            End();
        }
    }
}
