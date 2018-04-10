using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Web.Areas.ProgAdmin.Models
{
    public class StatsGaugeVM
    {
        public string PlatformCode { get; set; }
        public int? CampaignId { get; set; }

        public IEnumerable<TDStatsGauge> StatsGauges { get; set; }

        // The top-level StatsGauges are platform summaries; so if set, the child StatsGauges have the individual ExtAccounts.
        public IEnumerable<ExtAccount> ExtAccounts(bool syncableOnly = false)
        {
            var extAccts = StatsGauges.Where(x => x.Children != null).SelectMany(x => x.Children).Where(x => x.ExtAccount != null).Select(x => x.ExtAccount);
            if (syncableOnly)
            {
                var codesSyncable = Platform.Codes_Syncable();
                extAccts = extAccts.Where(x => codesSyncable.Contains(x.Platform.Code));
            }
            return extAccts;
        }
    }
}