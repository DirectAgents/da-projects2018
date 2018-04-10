using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.Entities.Cake;

namespace DirectAgents.Web.Models
{
    public class ScreensVM
    {
        public IEnumerable<StatsSummary> AdvStats { get; set; }
        public StatsSummary Total
        {
            get
            {
                var stats = new StatsSummary
                {
                    Views = AdvStats.Sum(a => a.Views),
                    Clicks = AdvStats.Sum(a => a.Clicks),
                    Conversions = AdvStats.Sum(a => a.Conversions),
                    Paid = AdvStats.Sum(a => a.Paid),
                    Sellable = AdvStats.Sum(a => a.Sellable),
                    Revenue = AdvStats.Sum(a => a.Revenue),
                    Cost = AdvStats.Sum(a => a.Cost)
                };
                return stats;
            }
        }
    }
}