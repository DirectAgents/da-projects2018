using System;
using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Web.Areas.ProgAdmin.Models
{
    public class TDStatsVM
    {
        public string Name { get; set; }
        public string PlatformCode { get; set; }
        public int? CampaignId { get; set; }
        public string ExternalId { get; set; }
        public int? AccountId { get; set; }
        public int? StratId { get; set; }
        public int? AdSetId { get; set; }

        public DateTime Month { set { MonthString = value.ToShortDateString(); } }
        public string MonthString { get; set; }

        public IEnumerable<TDRawStat> Stats { get; set; }
        public TDRawStat StatsTotal
        {
            get
            {
                var stat = new TDRawStat { Name = "TOTAL" };
                if (Stats != null)
                {
                    stat.Impressions = Stats.Sum(s => s.Impressions);
                    stat.AllClicks = Stats.Sum(s => s.AllClicks);
                    stat.Clicks = Stats.Sum(s => s.Clicks);
                    stat.PostClickConv = Stats.Sum(s => s.PostClickConv);
                    stat.PostClickRev = Stats.Sum(s => s.PostClickRev);
                    stat.PostViewConv = Stats.Sum(s => s.PostViewConv);
                    stat.PostViewRev = Stats.Sum(s => s.PostViewRev);
                    stat.Cost = Stats.Sum(s => s.Cost);
                }
                return stat;
            }
        }
        public IEnumerable<Conv> Convs { get; set; }
    }
}