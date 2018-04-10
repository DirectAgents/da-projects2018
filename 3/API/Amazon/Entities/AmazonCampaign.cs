using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Entities
{
    public class AmazonCampaign
    {
        public Int64 campaignId { get; set; }
        public string name { get; set; }
        public string campaignType { get; set; }
        public string targetingType { get; set; }
        public bool premiumBidAdjustment { get; set; }
        public decimal dailyBudget { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string state { get; set; }

    }
}
