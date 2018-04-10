using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DirectAgents.Domain.Entities.CPProg
{
    public class StatsSummary
    {
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int AllClicks { get; set; }
        public int PostClickConv { get; set; }
        public int PostViewConv { get; set; }
        public decimal Cost { get; set; }

        //TotalConv

        public virtual bool AllZeros()
        {
            return (Impressions == 0 && Clicks == 0 && PostClickConv == 0 && PostViewConv == 0 && Cost == 0);
        }
        public virtual void SetStats(StatsSummary stat)
        {
            Impressions = stat.Impressions;
            Clicks = stat.Clicks;
            AllClicks = stat.AllClicks;
            PostClickConv = stat.PostClickConv;
            PostViewConv = stat.PostViewConv;
            Cost = stat.Cost;
        }
        public void SetStats(IEnumerable<StatsSummary> stats)
        {
            SetBasicStats(stats);
        } // (to avoid naming conflict in derived class)
        protected void SetBasicStats(IEnumerable<StatsSummary> stats)
        {
            Impressions = stats.Sum(x => x.Impressions);
            Clicks = stats.Sum(x => x.Clicks);
            AllClicks = stats.Sum(x => x.AllClicks);
            PostClickConv = stats.Sum(x => x.PostClickConv);
            PostViewConv = stats.Sum(x => x.PostViewConv);
            Cost = stats.Sum(x => x.Cost);
        }
    }

    public interface IDatedObject
    {
        DateTime Date { get; }
    }

    public class DatedStatsSummary : StatsSummary, IDatedObject
    {
        public DateTime Date { get; set; }
    }
    public class DatedStatsSummaryWithRev : DatedStatsSummary
    {
        public decimal PostClickRev { get; set; }
        public decimal PostViewRev { get; set; }

        public override bool AllZeros()
        {
            return base.AllZeros() && PostClickRev == 0 && PostViewRev == 0;
        }
        public override void SetStats(StatsSummary stat)
        {
            base.SetStats(stat);
            if (stat is DatedStatsSummaryWithRev)
            {
                PostClickRev = ((DatedStatsSummaryWithRev)stat).PostClickRev;
                PostViewRev = ((DatedStatsSummaryWithRev)stat).PostViewRev;
            }
        }
        public void SetStats(IEnumerable<DatedStatsSummaryWithRev> stats)
        {
            SetBasicStats(stats);
            PostClickRev = stats.Sum(x => x.PostClickRev);
            PostViewRev = stats.Sum(x => x.PostViewRev);
        }
    }

    public class DailySummary : DatedStatsSummaryWithRev
    {
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual ExtAccount ExtAccount { get; set; }
    }

    // DailySummary for a Strategy
    public class StrategySummary : DatedStatsSummaryWithRev
    {
        public int StrategyId { get; set; }
        public virtual Strategy Strategy { get; set; }

        [NotMapped]
        public string StrategyName { get; set; }
        [NotMapped]
        public string StrategyEid { get; set; } // external id
    }

    // DailySummary for an AdSet
    public class AdSetSummary : DatedStatsSummaryWithRev
    {
        public int AdSetId { get; set; }
        public virtual AdSet AdSet { get; set; }

        [NotMapped]
        public string AdSetName { get; set; }
        [NotMapped]
        public string AdSetEid { get; set; } // external id
        [NotMapped]
        public string StrategyName { get; set; }
        [NotMapped]
        public string StrategyEid { get; set; } // external id
    }

    // DailySummary for a "TD ad"
    public class TDadSummary : DatedStatsSummary
    {
        public int TDadId { get; set; }
        public virtual TDad TDad { get; set; }

        [NotMapped]
        public string TDadName { get; set; }
        [NotMapped]
        public string TDadEid { get; set; } // external id
        [NotMapped]
        public int Width { get; set; }
        //[NotMapped]
    }

    // DailySummary for a Site / ExtAccount
    public class SiteSummary : DatedStatsSummary
    {
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual ExtAccount ExtAccount { get; set; }

        [NotMapped]
        public string SiteName
        {
            get { return _sitename; }
            set { _sitename = (value == null) ? null : value.ToLower(); }
        }
        private string _sitename;
    }

    public class Site
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

}
