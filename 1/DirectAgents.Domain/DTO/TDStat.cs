using System;
using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.Entities.CPProg;
using DirectAgents.Domain.Entities.RevTrack;

namespace DirectAgents.Domain.DTO
{
    public class TDMediaStatWithBudget : TDMediaStat, ITDLineItem
    {
        //public DateTime Date { get; set; } // the month of the budget
        private TDMediaBudget _budget;
        public ITDBudget Budget
        {
            get { return _budget; }
        }

        //public IEnumerable<TDStat> ExtAccountStats { get; set; }

        public TDMediaStatWithBudget(IEnumerable<DailySummary> dSums, BudgetInfoVals budgetVals)
            : base(dSums, budgetVals)
        {
            _budget = new TDMediaBudget { MediaSpend = budgetVals.MediaSpend };
        }
        public TDMediaStatWithBudget(IEnumerable<ProgSummary> pSums, BudgetInfoVals budgetVals)
            : base(pSums, budgetVals)
        {
            _budget = new TDMediaBudget { MediaSpend = budgetVals.MediaSpend };
        }

        //public TDStatWithBudget(TDStat tdStat, BudgetInfo budgetInfo)
        //{
        //    CopyFrom(tdStat);
        //    Budget.MediaSpend = budgetInfo.MediaSpend;
        //}

        public double FractionReached()
        {
            if (Budget.ClientCost == 0)
                return 0;
            return (double)(this.MediaSpend() / _budget.MediaSpend);
        }

        public decimal ClientCost
        {
            get { return MediaSpend(); }
        }
        public bool MoneyValsOnly { get; set; } // probably always false
    }
    public class TDMediaBudget : ITDBudget
    {
        public decimal MediaSpend { get; set; }
        public decimal ClientCost
        {
            get { return MediaSpend; }
        }
    }

    //Allows for the computation of MediaSpend, MgmtFee, TotalRevenue, Margin...
    public class TDMediaStat : TDRawStat
    {
        public override void CopyFrom(TDRawStat stat)
        {
            base.CopyFrom(stat);
            if (stat is TDMediaStat)
            {
                this.MgmtFeePct = ((TDMediaStat)stat).MgmtFeePct;
                this.MarginPct = ((TDMediaStat)stat).MarginPct;
            }
        }
        private MarginFeeVals MFVals { get; set; }

        public decimal MgmtFeePct
        {
            get { return MFVals.MgmtFeePct; }
            set { MFVals.MgmtFeePct = value; }
        }
        public decimal? MarginPct
        {
            get { return MFVals.MarginPct; }
            set { MFVals.MarginPct = (value.HasValue ? value.Value : 0); }
        }

        public TDMediaStat()
        {
            SetMarginFees(null); // initializes this.MFVals
        }
        //public TDMediaStat(decimal mgmtFeePct, decimal marginPct = 0, decimal mediaSpend = 0)
        //{
        //    this.MgmtFeePct = mgmtFeePct;
        //    this.MarginPct = marginPct;
        //    SetMediaSpend(mediaSpend);
        //}
        public TDMediaStat(IEnumerable<DailySummary> dSums, MarginFeeVals marginFees)
            : base(dSums)
        {
            SetMarginFees(marginFees);
        }
        public TDMediaStat(IEnumerable<ProgSummary> pSums, MarginFeeVals marginFees)
            : base(pSums)
        {
            SetMarginFees(marginFees);
        }

        // this.MFVals will always be instantiated after this method is called
        public void SetMarginFees(MarginFeeVals marginFees)
        {
            this.MFVals = new MarginFeeVals(marginFees);
        }

        public void SetMoneyVals(decimal cost, decimal mgmtFeePct, decimal marginPct)
        {
            this.Cost = cost;
            this.MgmtFeePct = mgmtFeePct;
            this.MarginPct = marginPct;
        }

        // Compute and set Cost based on the specified MediaSpend
        public void SetMediaSpend(decimal mediaSpend)
        {
            if (MFVals.CostGoesThruDA())
                this.Cost = mediaSpend;
            else
                this.Cost = mediaSpend * MFVals.MediaSpendToRevMultiplier * MFVals.RevToCostMultiplier;
        }

        public decimal TotalRevenue
        {
            get { return MFVals.CostToTotalRevenue(this.Cost); }
        }

        public decimal MediaSpend()
        {
            return MFVals.CostToClientCost(this.Cost);
        }

        public decimal MgmtFee
        {
            get { return MFVals.CostToMgmtFee(this.Cost); }
        }

        public decimal DACost
        {
            get
            {
                if (!MFVals.CostGoesThruDA())
                    return 0;
                else
                    return Cost;
            }
        }

        public decimal Margin
        {
            get { return TotalRevenue - DACost; }
        }

        public override decimal CPM
        {
            get { return (Impressions == 0) ? 0 : Math.Round(1000 * MediaSpend() / Impressions, 2); }
        }
        public override decimal CPC
        {
            get { return (Clicks == 0) ? 0 : Math.Round(MediaSpend() / Clicks, 2); }
        }
        public override decimal CPA
        {
            get { return (TotalConv == 0) ? 0 : Math.Round(MediaSpend() / TotalConv, 2); }
        }
    }

    public class TDRawStat
    {
        // Possible ways to identify the stats...
        public string Name { get; set; }
        public Platform Platform { get; set; }
        public Campaign Campaign { get; set; }
        public ExtAccount ExtAccount { get; set; }
        public Strategy Strategy { get; set; }
        public TDad TDad { get; set; }
        public AdSet AdSet { get; set; }
        public Site Site { get; set; }
        public ActionType ActionType { get; set; }
        public ProgVendor ProgVendor { get; set; }

        public int Impressions { get; set; }
        public int AllClicks { get; set; }
        public int Clicks { get; set; }
        public int PostClickConv { get; set; }
        public decimal PostClickRev { get; set; }
        public int PostViewConv { get; set; }
        public decimal PostViewRev { get; set; }
        public decimal Cost { get; set; }
        //public int Prospects { get; set; }

        public int TotalConv
        {
            get { return PostClickConv + PostViewConv; }
        }
        public decimal TotalRev
        {
            get { return PostClickRev + PostViewRev; }
        }

        public virtual bool AllZeros()
        {
            return (Impressions == 0 && AllClicks == 0 && Clicks == 0 && PostClickConv == 0 && PostViewConv == 0 && Cost == 0);
        }

        public virtual void CopyFrom(TDRawStat stat)
        {
            this.Platform = stat.Platform;
            this.Name = stat.Name;
            this.Campaign = stat.Campaign;
            this.ExtAccount = stat.ExtAccount;

            this.Impressions = stat.Impressions;
            this.AllClicks = stat.AllClicks;
            this.Clicks = stat.Clicks;
            this.PostClickConv = stat.PostClickConv;
            this.PostViewConv = stat.PostViewConv;
        }

        // Constructors
        public TDRawStat() { }
        public TDRawStat(IEnumerable<StatsSummary> sSums)
        {
            SetStatsFrom(sSums);
        }
        private void SetStatsFrom(IEnumerable<StatsSummary> sSums, bool roundCost = false)
        {
            if (sSums != null && sSums.Any())
            {
                this.Impressions = sSums.Sum(x => x.Impressions);
                this.AllClicks = sSums.Sum(x => x.AllClicks);
                this.Clicks = sSums.Sum(x => x.Clicks);
                this.PostClickConv = sSums.Sum(x => x.PostClickConv);
                this.PostViewConv = sSums.Sum(x => x.PostViewConv);
                this.Cost = sSums.Sum(x => x.Cost);
                if (roundCost)
                    this.Cost = Math.Round(this.Cost, 2);
                if (sSums.First() is DatedStatsSummaryWithRev) // We're assuming they're all the same type
                {
                    this.PostClickRev = sSums.Sum(x => ((DatedStatsSummaryWithRev)x).PostClickRev);
                    this.PostViewRev = sSums.Sum(x => ((DatedStatsSummaryWithRev)x).PostViewRev);
                }
            }
        }

        public TDRawStat(IEnumerable<ActionStatsWithVals> aStats)
        {
            SetStatsFrom(aStats);
        }
        private void SetStatsFrom(IEnumerable<ActionStatsWithVals> aStats)
        {
            if (aStats != null && aStats.Any())
            {
                this.PostClickConv = aStats.Sum(x => x.PostClick);
                this.PostViewConv = aStats.Sum(x => x.PostView);
                this.PostClickRev = aStats.Sum(x => x.PostClickVal);
                this.PostViewRev = aStats.Sum(x => x.PostViewVal);
            }
        }

        public TDRawStat(IEnumerable<ProgSummary> pSums)
        {
            SetStatsFrom(pSums);
        }
        private void SetStatsFrom(IEnumerable<ProgSummary> pSums, bool roundCost = false)
        {
            if (pSums != null && pSums.Any())
            {
                this.Cost = pSums.Sum(x => x.Cost);
                if (roundCost)
                    this.Cost = Math.Round(this.Cost, 2);
            }
        }

        // Computed properties
        public double CTR
        {
            get { return (Impressions == 0) ? 0 : Math.Round((double)Clicks / Impressions, 4); }
        }
        public double ConvRate
        {
            get { return (Clicks == 0) ? 0 : Math.Round((double)TotalConv / Clicks, 4); }
        }

        public virtual decimal CPM
        {
            get { return (Impressions == 0) ? 0 : Math.Round(1000 * Cost / Impressions, 2); }
        }
        public virtual decimal CPC
        {
            get { return (Clicks == 0) ? 0 : Math.Round(Cost / Clicks, 2); }
        }
        public virtual decimal CPA
        {
            get { return (TotalConv == 0) ? 0 : Math.Round(Cost / TotalConv, 2); }
        }
    }
}
