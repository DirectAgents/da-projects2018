using System.Collections.Generic;

namespace DirectAgents.Domain.Entities.AB
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<SpendBucket> SpendBuckets { get; set; }
    }
}
