using System.Collections.Generic;
using System.Linq;

namespace DirectAgents.Domain.Entities.Wiki
{
    public partial class Country
    {
        //[NotMapped]
        //public IEnumerable<Campaign> ActiveCampaigns
        //{
        //    get
        //    {
        //        return Campaigns.Where(c => c.StatusId != Status.Inactive);
        //    }
        //}

        public IEnumerable<Campaign> FilteredCampaigns(string[] excludeInName, bool excludeHidden, bool excludeInactive)
        {
            var campaigns = this.Campaigns.AsEnumerable();
            if (excludeInName != null)
            {
                foreach (string excludeString in excludeInName)
                {
                    campaigns = campaigns.Where(c => !c.Name.ToUpper().Contains(excludeString.ToUpper()));
                }
            }
            if (excludeHidden)
            {
                campaigns = campaigns.Where(c => !c.Hidden);
            }
            if (excludeInactive)
            {
                campaigns = campaigns.Where(c => c.StatusId != Status.Inactive);
            }
            return campaigns;
        }
    }
}
