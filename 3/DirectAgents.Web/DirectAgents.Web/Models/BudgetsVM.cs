using DirectAgents.Domain.Entities.Cake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DirectAgents.Web.Models
{
    public class BudgetsVM
    {
        public IEnumerable<Advertiser> Advertisers { get; set; }
        public IEnumerable<Offer> Offers { get; set; }
        public Contact AccountManager { get; set; }
        public bool ShowAll { get; set; }

        public string Sort { get; set; }
        public bool SortDesc { get; set; }

        // filters
        public int? AcctMgrId { get; set; }
        public int? AdvId { get; set; }
        public bool? WithBudget { get; set; }
        public int? MinPercent { get; set; }
        public bool IncludeInactive { get; set; }
    }
}