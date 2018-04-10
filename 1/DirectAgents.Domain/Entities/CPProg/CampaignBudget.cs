using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DirectAgents.Domain.Entities.CPProg
{
    public class Campaign
    {
        public int Id { get; set; }
        public int AdvertiserId { get; set; }
        public virtual Advertiser Advertiser { get; set; }

        public string Name { get; set; }
        public decimal BaseFee { get; set; }

        public virtual ICollection<ExtAccount> ExtAccounts { get; set; }
        public virtual ICollection<BudgetInfo> BudgetInfos { get; set; }
        public BudgetInfoVals DefaultBudgetInfo { get; set; }
        public virtual ICollection<PlatformBudgetInfo> PlatformBudgetInfos { get; set; }

        public IEnumerable<PlatformBudgetInfo> PlatformBudgetInfosFor(DateTime month)
        {
            return PlatformBudgetInfos.Where(pbi => pbi.Date == month);
        }

        // if no PBI for specific month/platform, bubbles up to the parent(month) or default(for the campaign)
        public BudgetInfoVals PlatformBudgetInfoFor(DateTime date, int platformId, bool useParentValsIfNone)
        {
            BudgetInfoVals biVals = PlatformBudgetInfoForInner(date, platformId);
            if (biVals == null && useParentValsIfNone)
            {
                biVals = new BudgetInfoVals();
                var parentBI = BudgetInfoFor(date, useDefaultIfNone: true);
                if (parentBI != null)
                {
                    biVals.MgmtFeePct = parentBI.MgmtFeePct;
                    biVals.MarginPct = parentBI.MarginPct;
                } // Ignore the MediaSpend(Budget) in this case; it applies to the campaign overall
            }
            return biVals;
        }
        private PlatformBudgetInfo PlatformBudgetInfoForInner(DateTime date, int platformId)
        {
            if (PlatformBudgetInfos == null)
                return null;
            var firstOfMonth = new DateTime(date.Year, date.Month, 1);
            var pbis = PlatformBudgetInfos.Where(pbi => pbi.Date == firstOfMonth && pbi.PlatformId == platformId);
            return pbis.Any() ? pbis.First() : null;
        }

        public BudgetInfoVals BudgetInfoFor(DateTime date, bool useDefaultIfNone)
        {
            BudgetInfoVals budgetInfo = BudgetInfoFor(date);
            if (budgetInfo == null && useDefaultIfNone)
                budgetInfo = this.DefaultBudgetInfo;
            return budgetInfo;
        }
        public BudgetInfo BudgetInfoFor(DateTime date)
        {
            if (BudgetInfos == null)
                return null;
            var firstOfMonth = new DateTime(date.Year, date.Month, 1);
            var budgets = BudgetInfos.Where(b => b.Date == firstOfMonth);
            return budgets.Any() ? budgets.First() : null;
        }

        //TODO: have an arg... if want to check PlatBudgInfos too
        public BudgetInfo EarliestBudgetInfo()
        {
            if (BudgetInfos == null || !BudgetInfos.Any())
                return null;
            return BudgetInfos.OrderBy(bi => bi.Date).First();
        }
        public BudgetInfo LatestBudgetInfo()
        {
            if (BudgetInfos == null || !BudgetInfos.Any())
                return null;
            return BudgetInfos.OrderByDescending(bi => bi.Date).First();
        }

        // returns months in reverse chronological order
        public DateTime[] MonthsWithoutBudgetInfos(int monthsToCheck = 12)
        {
            bool anyNonFutureMonths = false; // include at least one non-future month
            var months = new List<DateTime>();
            var iMonth = DateTime.Today.AddMonths(1);
            iMonth = new DateTime(iMonth.Year, iMonth.Month, 1); // start with next month (the 1st of)
            for (int i = 0; i < monthsToCheck || !anyNonFutureMonths; i++)
            {
                if (BudgetInfos == null || !BudgetInfos.Any(b => b.Date == iMonth))
                {
                    months.Add(iMonth);
                    if (i > 0)
                        anyNonFutureMonths = true;
                }
                iMonth = iMonth.AddMonths(-1);
            }
            return months.ToArray();
        }

        // flight dates, goal...
    }

    public class BudgetInfo : BudgetInfoVals
    {
        public int CampaignId { get; set; }
        public DateTime Date { get; set; }
        public virtual Campaign Campaign { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // used for SSRS report

        // Constructor
        public BudgetInfo(int campaignId, DateTime date, BudgetInfoVals valuesToSet = null)
        {
            CampaignId = campaignId;
            Date = date;
            if (valuesToSet != null)
                SetFrom(valuesToSet);
        }
        public BudgetInfo() { } // required for EF
        //NOTE: making this private caused lazy-loading issues - e.g. BudgetInfos/Edit
    }

    public class PlatformBudgetInfo : BudgetInfoVals
    {
        public int CampaignId { get; set; }
        public int PlatformId { get; set; }
        public DateTime Date { get; set; }

        public virtual Campaign Campaign { get; set; }
        public virtual Platform Platform { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // used for SSRS report

        // Constructor
        public PlatformBudgetInfo(int campaignId, int platformId, DateTime date, BudgetInfoVals valuesToSet = null)
        {
            CampaignId = campaignId;
            PlatformId = platformId;
            Date = date;
            if (valuesToSet != null)
                SetFrom(valuesToSet);
        }
        public PlatformBudgetInfo() { } // required for EF
        //NOTE: making this private caused lazy-loading issues - e.g. Campaigns/Edit
    }

    public class BudgetInfoVals : MarginFeeVals
    {
        //The key values that define a budget...
        public decimal MediaSpend { get; set; }

        //Computed budget vals
        public decimal MgmtFee()
        {
            return MediaSpend * MgmtFeePct / 100;
        }
        public decimal TotalRevenue()
        {
            return MediaSpend * MediaSpendToRevMultiplier;
        }
        public decimal DACost()
        {
            return TotalRevenue() * RevToCostMultiplier;
        }
        public decimal Margin()
        {
            return TotalRevenue() * MarginPct / 100;
        }

        public void SetFrom(BudgetInfoVals source)
        {
            MediaSpend = source.MediaSpend;
            MgmtFeePct = source.MgmtFeePct;
            MarginPct = source.MarginPct;
        }
    }
    public class MarginFeeVals
    {
        public decimal MgmtFeePct { get; set; }
        public decimal MarginPct { get; set; }

        // Constructor
        public MarginFeeVals(MarginFeeVals mfVals = null)
        {
            if (mfVals != null)
            {
                this.MgmtFeePct = mfVals.MgmtFeePct;
                this.MarginPct = mfVals.MarginPct;
            }
        }

        public bool CostGoesThruDA()
        {
            return (MarginPct != 100);
        }

        //note: is 0 if MarginPct==100
        public decimal RevToCostMultiplier
        {
            get { return (1 - MarginPct / 100); }
        }
        public decimal MediaSpendToRevMultiplier
        {
            get { return (1 + MgmtFeePct / 100); }
        }

        // i.e. marked up media-spend
        public decimal CostToClientCost(decimal cost)
        {
            if (CostGoesThruDA())
                return cost / RevToCostMultiplier / MediaSpendToRevMultiplier;
            else
                return cost;
        }
        public decimal CostToTotalRevenue(decimal cost)
        {
            if (CostGoesThruDA())
                return cost / RevToCostMultiplier;
            else
                return cost * MgmtFeePct / 100;
        }
        public decimal CostToMgmtFee(decimal cost)
        {
            var revenue = CostToTotalRevenue(cost);
            if (CostGoesThruDA())
                //return revenue - CostToClientCost(cost); //TODO: make more efficient
                //return revenue - revenue / MediaSpendToRevMultiplier;
                return revenue * MgmtFeePct / (100 + MgmtFeePct);
            else
                return revenue;
        }
    }

}
