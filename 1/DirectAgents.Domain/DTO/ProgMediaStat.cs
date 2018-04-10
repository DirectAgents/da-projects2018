using System;
using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.Entities.CPProg;
using DirectAgents.Domain.Entities.RevTrack;

namespace DirectAgents.Domain.DTO
{
    //// ?Would need to create IProgLineItem ?
    //public class ProgMediaStatWithBudget : ProgMediaStat, ITDLineItem
    //{
    //    private TDMediaBudget _budget;
    //    public ITDBudget Budget
    //    {
    //        get { return _budget; }
    //    }

    //    public ProgMediaStatWithBudget(IEnumerable<ProgSummary> pSums, BudgetInfoVals budgetVals)
    //        : base(pSums, budgetVals)
    //    {
    //        _budget = new TDMediaBudget { MediaSpend = budgetVals.MediaSpend };
    //    }

    //    public double FractionReached()
    //    {
    //        if (Budget.ClientCost == 0)
    //            return 0;
    //        return (double)(this.MediaSpend() / _budget.MediaSpend);
    //    }

    //    public decimal ClientCost
    //    {
    //        get { return MediaSpend(); }
    //    }
    //    public bool MoneyValsOnly { get; set; } // probably always false
    //}

    //Allows for the computation of MediaSpend, MgmtFee, TotalRevenue, Margin...
    public class ProgMediaStat : ProgRawStat
    {
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

        public ProgMediaStat()
        {
            SetMarginFees(null); // initializes this.MFVals
        }
        public ProgMediaStat(IEnumerable<ProgSummary> pSums, MarginFeeVals marginFees)
            : base(pSums)
        {
            SetMarginFees(marginFees);
        }

        // this.MFVals will always be instantiated after this method is called
        public void SetMarginFees(MarginFeeVals marginFees)
        {
            this.MFVals = new MarginFeeVals(marginFees);
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
    }

    public class ProgRawStat
    {
        public decimal Cost { get; set; }

        // Constructors
        public ProgRawStat() { }
        public ProgRawStat(IEnumerable<ProgSummary> pSums)
        {
            SetStatsFrom(pSums);
        }

        private void SetStatsFrom(IEnumerable<ProgSummary> pSums, bool roundCost = false)
        {
            if (pSums != null && pSums.Any())
            {
                this.Cost = pSums.Sum(ps => ps.Cost);
                if (roundCost)
                    this.Cost = Math.Round(this.Cost, 2);
            }
        }
    }
}
