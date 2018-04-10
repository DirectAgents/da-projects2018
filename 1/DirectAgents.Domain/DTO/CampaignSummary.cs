using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DirectAgents.Domain.DTO
{
    public class CampaignSummary
    {
        public int Pid { get; set; }

        public string CampaignName { get; set; }

        public int RevenueCurrencyId { get; set; }

        public decimal Revenue { get; set; }

        public int CostCurrencyId { get; set; }

        public decimal Cost { get; set; }

        public decimal EPC
        {
            get
            {
                if (Clicks == 0)
                    return 0;
                else
                    return Cost / Clicks;
            }
        }

        public int Clicks { get; set; }
    }
}
