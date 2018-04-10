using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Domain.Entities.RevTrack
{
    public class ProgClient
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }

        public int? ABClientId { get; set; }

        public virtual ICollection<ProgCampaign> ProgCampaigns { get; set; }
    }

    public class ProgCampaign
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public int ProgClientId { get; set; }
        public virtual ProgClient ProgClient { get; set; }

        public string Name { get; set; }
        public BudgetInfoVals DefaultBudgetInfo { get; set; }

        public virtual ICollection<ProgBudgetInfo> ProgBudgetInfos { get; set; }
        public virtual ICollection<ProgVendorBudgetInfo> ProgVendorBudgetInfos { get; set; }
        public virtual ICollection<ProgSummary> ProgSummaries { get; set; }
        public virtual ICollection<ProgExtraItem> ProgExtraItems { get; set; }

        // if no VBI for specific month/platform, bubbles up to the parent(month) or default(for the campaign)
        public BudgetInfoVals ProgVendorBudgetInfoFor(DateTime date, int progVendorId, bool useParentValsIfNone)
        {
            BudgetInfoVals biVals = ProgVendorBudgetInfoForInner(date, progVendorId);
            if (biVals == null && useParentValsIfNone)
            {
                biVals = new BudgetInfoVals();
                var parentBI = ProgBudgetInfoFor(date, useDefaultIfNone: true);
                if (parentBI != null)
                {
                    biVals.MgmtFeePct = parentBI.MgmtFeePct;
                    biVals.MarginPct = parentBI.MarginPct;
                } // Ignore the MediaSpend(Budget) in this case; it applies to the campaign overall
            }
            return biVals;
        }
        private ProgVendorBudgetInfo ProgVendorBudgetInfoForInner(DateTime date, int vendorId)
        {
            if (ProgVendorBudgetInfos == null)
                return null;
            var firstOfMonth = new DateTime(date.Year, date.Month, 1);
            var vbis = ProgVendorBudgetInfos.Where(vbi => vbi.Date == firstOfMonth && vbi.ProgVendorId == vendorId);
            return vbis.Any() ? vbis.First() : null;
        }

        public BudgetInfoVals ProgBudgetInfoFor(DateTime date, bool useDefaultIfNone)
        {
            BudgetInfoVals progBudgetInfo = ProgBudgetInfoFor(date);
            if (progBudgetInfo == null && useDefaultIfNone)
                progBudgetInfo = this.DefaultBudgetInfo;
            return progBudgetInfo;
        }
        public ProgBudgetInfo ProgBudgetInfoFor(DateTime date)
        {
            if (ProgBudgetInfos == null)
                return null;
            var firstOfMonth = new DateTime(date.Year, date.Month, 1);
            var budgets = ProgBudgetInfos.Where(b => b.Date == firstOfMonth);
            return budgets.Any() ? budgets.First() : null;
        }
    }

    // --- BudgetInfos ---

    public class ProgBudgetInfo : BudgetInfoVals
    {
        public int ProgCampaignId { get; set; }
        public DateTime Date { get; set; }

        public virtual ProgCampaign ProgCampaign { get; set; }
    }
    public class ProgVendorBudgetInfo : BudgetInfoVals
    {
        public int ProgCampaignId { get; set; }
        public int ProgVendorId { get; set; }
        public DateTime Date { get; set; }

        public virtual ProgCampaign ProgCampaign { get; set; }
        public virtual ProgVendor ProgVendor { get; set; }
    }

    // --- Vendor ---

    public class ProgVendor
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        [MaxLength(50)]
        public string Code { get; set; }
    }

}
