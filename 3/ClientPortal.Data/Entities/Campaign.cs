using ClientPortal.Data.Contexts;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ClientPortal.Data.Contexts
{
    public partial class Campaign
    {
        [NotMapped]
        public IEnumerable<CampaignDrop> CampaignDrops_Originals
        {
            get
            {
                return CampaignDrops.Where(cd => cd.CopyOf == null);
            }
        }
    }
}
