using System;
using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.Entities.CPProg;
using DirectAgents.Domain.Entities.RevTrack;

namespace DirectAgents.Domain.DTO
{
    public interface ITDLineItem : ITDRawLineItem, ITDClickStats
    {
        Platform Platform { get; }
        ProgVendor ProgVendor { get; }

        ITDBudget Budget { get; }
        double FractionReached();

        bool MoneyValsOnly { get; } // i.e. no click stats

        decimal CPM { get; }
        decimal CPC { get; }
        decimal CPA { get; }
    }
    public interface ITDRawLineItem
    {
        // CopyFrom(), AllZeros() ?

        decimal DACost { get; }
        decimal ClientCost { get; } // may or may not go through us
        decimal MgmtFee { get; }
        decimal TotalRevenue { get; }
        decimal Margin { get; }
        decimal? MarginPct { get; }
    }
    public interface ITDClickStats
    {
        int Impressions { get; }
        int Clicks { get; }
        int PostClickConv { get; }
        int PostViewConv { get; }

        int TotalConv { get; }
        double CTR { get; }
        double ConvRate { get; }
    }
    public interface ITDBudget
    {
        decimal ClientCost { get; }
    }

    public class TDRawLineItem : ITDRawLineItem
    {
        //public virtual void CopyFrom(TDRawLineItem stat)
        //{
        //    this.DACost = stat.DACost;
        //    this.ClientCost = stat.ClientCost;
        //    this.MgmtFee = stat.MgmtFee;
        //    this.TotalRevenue = stat.TotalRevenue;
        //}
        public virtual bool AllZeros(bool includeClientCost = true)
        {
            bool allZeros = (DACost == 0 && MgmtFee == 0 && TotalRevenue == 0);
            if (includeClientCost)
                return (allZeros && ClientCost == 0);
            else
                return allZeros;
        }

        public DateTime? Date { get; set; }

        public decimal DACost { get; set; }
        public decimal ClientCost { get; set; } // may or may not go through us
        public decimal MgmtFee { get; set; }
        public decimal TotalRevenue { get; set; }

        public decimal Margin
        {
            get { return (TotalRevenue - DACost); }
        }
        public decimal? MarginPct
        {
            get { return (TotalRevenue == 0) ? (decimal?)null : (100 * Margin / TotalRevenue); }
        }

        // Constructors
        public TDRawLineItem() { }
        public TDRawLineItem(IEnumerable<ITDLineItem> statsToSum)
        {
            DACost = statsToSum.Sum(s => s.DACost);
            ClientCost = statsToSum.Sum(s => s.ClientCost);
            MgmtFee = statsToSum.Sum(s => s.MgmtFee);
            TotalRevenue = statsToSum.Sum(s => s.TotalRevenue);
        }
        public TDRawLineItem(IEnumerable<ExtraItem> itemsToSum)
        {
            DACost = itemsToSum.Sum(i => i.Cost);
            ClientCost = itemsToSum.Sum(i => i.Revenue);
            // (no fee)
            TotalRevenue = ClientCost;
        }
        public TDRawLineItem(IEnumerable<ProgExtraItem> itemsToSum)
        {
            DACost = itemsToSum.Sum(i => i.Cost);
            ClientCost = itemsToSum.Sum(i => i.Revenue);
            // (no fee?)
            TotalRevenue = ClientCost;
        }

        //public TDRawLineItem(decimal rawCost = 0, decimal mgmtFeePct = 0, decimal marginPct = 0)
        //{
        //    SetMoneyVals(rawCost, mgmtFeePct, marginPct);
        //}
        //public void SetMoneyVals(decimal rawCost, decimal mgmtFeePct = 0, decimal marginPct = 0)
        //{
        //    if (marginPct == 100)
        //    {
        //        CostGoesThruDA = false;
        //        DACost = 0;
        //        ClientCost = rawCost;
        //        TotalRevenue = rawCost * mgmtFeePct / 100;
        //    }
        //    else
        //    {
        //        CostGoesThruDA = true;
        //        DACost = rawCost;
        //        TotalRevenue = rawCost / RevToCostMultiplier(marginPct);
        //        ClientCost = TotalRevenue / ClientCostToRevMultiplier(mgmtFeePct);
        //    }
        //}
        //private decimal RevToCostMultiplier(decimal marginPct)
        //{
        //    return (1 - marginPct / 100);
        //}
        //private decimal ClientCostToRevMultiplier(decimal mgmtFeePct)
        //{
        //    return (1 + mgmtFeePct / 100);
        //}
    }
}
