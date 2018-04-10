using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Entities
{
    public class StatSummary
    {
        public decimal cost { get; set; }
        public int impressions { get; set; }
        public int clicks { get; set; }
        public int attributedConversions14d { get; set; }
        public decimal attributedSales14d { get; set; }

        public virtual bool AllZeros()
        {
            return (cost == 0 && impressions == 0 && clicks == 0 && attributedConversions14d == 0 && attributedSales14d == 0);
        }

        public void SetAllZeros()
        {
            cost = attributedSales14d = 0;
            impressions = clicks = attributedConversions14d = 0;

        }

        public void SetStatTotals(IEnumerable<StatSummary> stats)
        {
            if (stats != null && stats.Any())
            {
                cost = stats.Sum(x => x.cost);
                impressions = stats.Sum(x => x.impressions);
                clicks = stats.Sum(x => x.clicks);
                attributedConversions14d = stats.Sum(x => x.attributedConversions14d);
                attributedSales14d = stats.Sum(x => x.attributedSales14d);
            }
            else
            {
                SetAllZeros();
            }
        }
    }

    public class AmazonDailySummary : StatSummary
    {
        public Int64 campaignId { get; set; }
        public string campaignName { get; set; }
        public DateTime date { get; set; } 
    }

    public class AmazonAdDailySummary : StatSummary
    {
        public string adId { get; set; }
        public DateTime date { get; set; }
    }

    public class AmazonKeywordDailySummary : StatSummary
    {
        public string KeywordId { get; set; }
        public DateTime date { get; set; }
    }
}
