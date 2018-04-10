using System.Collections.Generic;
using System.Linq;

namespace DirectAgents.Domain.Entities.AB
{
    public class CampaignWrap
    {
        private IGrouping<Campaign, SpendBucket> BucketGroup;

        public CampaignWrap(IGrouping<Campaign, SpendBucket> bucketGroup)
        {
            this.BucketGroup = bucketGroup;
        }

        public Campaign Campaign
        {
            get { return BucketGroup.Key; }
        }

        public IEnumerable<SpendBucket> SpendBuckets
        {
            get { return BucketGroup; }
        }
        public IEnumerable<int> SpendBucketIds
        {
            get { return BucketGroup.Select(x => x.Id); }
        }
        public IEnumerable<int> AcctIds
        {
            get { return BucketGroup.Select(x => x.AcctId); }
        }
    }
}
