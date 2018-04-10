using System;

namespace AdRoll.Entities
{
    public class StatSummary
    {
        public int impressions { get; set; }
        // paid_impressions
        public int click_through_conversions { get; set; }
        public int view_through_conversions { get; set; }
        public int clicks { get; set; }
        public double cost { get; set; }
        public int prospects { get; set; }

        public virtual bool AllZeros(bool includeProspects = false)
        {
            bool allZeros = (impressions == 0 && clicks == 0 && click_through_conversions == 0 && view_through_conversions == 0 && cost == 0);
            if (includeProspects)
                return (allZeros && prospects == 0);
            else
                return allZeros;
        }
    }

    // used for Advertisable daily report
    public class AdrollDailySummary : StatSummary
    {
        public DateTime date { get; set; } // not always returned from the api
    }
    //TODO: add date to StatSummary and rename to AdrollStatSummary?

    // used for Advertisable report
    public class AdvertisableSummary : StatSummary
    {
        public string eid { get; set; }
        public string advertisable { get; set; }
        // status, created_date
    }

    // used for Campaign daily report
    public class CampaignSummary : AdrollDailySummary
    {
        public string eid { get; set; }
        public string campaign { get; set; } // campaign name
        public string advertiser { get; set; } // advertisable name
        public string type { get; set; } // e.g. "Retargeting"
        public string status { get; set; } // e.g. "approved"
        public DateTime created_date { get; set; }
        public DateTime start_date { get; set; }
        public DateTime? end_date { get; set; }
        public double budget_USD { get; set; }
        public double adjusted_attributed_click_through_rev { get; set; }
        public double adjusted_attributed_view_through_rev { get; set; }

        public override bool AllZeros(bool includeProspects = false)
        {
            return base.AllZeros(includeProspects: includeProspects) && adjusted_attributed_click_through_rev == 0 && adjusted_attributed_view_through_rev == 0;
        }
    }

    // used for Ad daily report
    public class AdSummary : AdrollDailySummary
    {
        public string eid { get; set; }
        public string ad { get; set; } // ad name
        public int height { get; set; }
        public int width { get; set; }
        public DateTime created_date { get; set; }
        public string type { get; set; }
        public string src { get; set; }
    }
}
