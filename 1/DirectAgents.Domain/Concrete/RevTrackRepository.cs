using System;
using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities.RevTrack;

namespace DirectAgents.Domain.Concrete
{
    public partial class RevTrackRepository : IRevTrackRepository, IDisposable
    {
        private RevTrackContext context;

        public RevTrackRepository(RevTrackContext context)
        {
            this.context = context;
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        // ---

        public ProgClient ProgClient(int id)
        {
            return context.ProgClients.Find(id);
        }
        public IQueryable<ProgClient> ProgClients(int? ABClientId = null)
        {
            var progClients = context.ProgClients.AsQueryable();
            if (ABClientId.HasValue)
                progClients = progClients.Where(c => c.ABClientId == ABClientId.Value);

            return progClients;
        }

        public ProgCampaign ProgCampaign(int id)
        {
            return context.ProgCampaigns.Find(id);
        }
        public IQueryable<ProgCampaign> ProgCampaigns()
        {
            return context.ProgCampaigns;
        }

        public ProgVendor ProgVendor(int id)
        {
            return context.ProgVendors.Find(id);
        }
        public IQueryable<ProgVendor> ProgVendors()
        {
            return context.ProgVendors;
        }

        // --- Stats ---

        public IQueryable<ProgSummary> ProgSummaries(DateTime? startDate, DateTime? endDate, int? clientId = null, int? campaignId = null, int? vendorId = null)
        {
            var pSums = context.ProgSummaries.AsQueryable();
            if (startDate.HasValue)
                pSums = pSums.Where(s => s.Date >= startDate.Value);
            if (endDate.HasValue)
                pSums = pSums.Where(s => s.Date <= endDate.Value);
            if (clientId.HasValue)
                pSums = pSums.Where(s => s.ProgCampaign.ProgClientId == clientId.Value);
            if (campaignId.HasValue)
                pSums = pSums.Where(s => s.ProgCampaignId == campaignId.Value);
            if (vendorId.HasValue)
                pSums = pSums.Where(s => s.ProgVendorId == vendorId.Value);

            return pSums;
        }

        public ProgExtraItem ProgExtraItem(int id)
        {
            return context.ProgExtraItems.Find(id);
        }
        public IQueryable<ProgExtraItem> ProgExtraItems(DateTime? startDate, DateTime? endDate, int? clientId = null, int? campaignId = null, int? vendorId = null)
        {
            var items = context.ProgExtraItems.AsQueryable();
            if (startDate.HasValue)
                items = items.Where(i => i.Date >= startDate.Value);
            if (endDate.HasValue)
                items = items.Where(i => i.Date <= endDate.Value);
            if (clientId.HasValue)
                items = items.Where(i => i.ProgCampaign.ProgClientId == clientId.Value);
            if (campaignId.HasValue)
                items = items.Where(i => i.ProgCampaignId == campaignId.Value);
            if (vendorId.HasValue)
                items = items.Where(i => i.ProgVendorId == vendorId.Value);
            return items;
        }

        // Get stats for one client, with one lineitem for each vendor (plus extraitems)
        public ProgClientStats GetProgClientStats(DateTime monthStart, int clientId)
        {
            var progClient = ProgClient(clientId);
            return GetProgClientStats(monthStart, progClient);
        }
        public ProgClientStats GetProgClientStats(DateTime monthStart, ProgClient progClient)
        {
            if (progClient == null)
                return null; // ?new ProgClientStats - blank?

            var vendorLineItems = new List<ITDLineItem>();
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Get Media Stats
            var progSums = ProgSummaries(monthStart, monthEnd, clientId: progClient.Id);
            var progCampaigns = progSums.Select(s => s.ProgCampaign).Distinct().ToList();
            var progVendors = progSums.Select(s => s.ProgVendor).Distinct().OrderBy(v => v.Name).ToList();
            foreach (var progVendor in progVendors)
            {
                // Note: Often there's just one campaign, but this will handle when there's more.
                foreach (var progCampaign in progCampaigns)
                {
                    var campVendorProgSums = progSums.Where(s => s.ProgCampaignId == progCampaign.Id && s.ProgVendorId == progVendor.Id);
                    var budgetInfoVals = progCampaign.ProgVendorBudgetInfoFor(monthStart, progVendor.Id, useParentValsIfNone: true);
                    var stat = new TDMediaStatWithBudget(campVendorProgSums, budgetInfoVals)
                    {
                        // ?assign campaign/vendor ?budget
                        ProgVendor = progVendor
                    };
                    vendorLineItems.Add(stat);
                }
            }

            // Get Extra Items
            var extraItems = ProgExtraItems(monthStart, monthEnd, clientId: progClient.Id);
            progCampaigns = extraItems.Select(i => i.ProgCampaign).Distinct().ToList();
            progVendors = extraItems.Select(i => i.ProgVendor).Distinct().OrderBy(v => v.Name).ToList();
            foreach (var progVendor in progVendors)
            {
                foreach (var progCampaign in progCampaigns)
                {
                    var campVendorExtraItems = extraItems.Where(i => i.ProgCampaignId == progCampaign.Id && i.ProgVendorId == progVendor.Id);
                    var budgetInfoVals = progCampaign.ProgVendorBudgetInfoFor(monthStart, progVendor.Id, useParentValsIfNone: true);
                    var lineItem = new TDLineItem(campVendorExtraItems, (budgetInfoVals != null ? budgetInfoVals.MediaSpend : (decimal?)null))
                    {
                        // ?assign campaign/vendor ?budget
                        ProgVendor = progVendor,
                        MoneyValsOnly = true
                    };
                    vendorLineItems.Add(lineItem);
                }
            }

            var progClientStats = new ProgClientStats(progClient, vendorLineItems, monthStart);
            return progClientStats;
        }

        // * INCOMPLETE *
        public string GetProgCampStats(DateTime monthStart, int campId)
        {
            var progCampaign = ProgCampaign(campId);
            if (progCampaign == null)
                return null; // ?new ProgCampStats - blank?

            var vendorStats = new List<ITDLineItem>();
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Get Media Stats
            var progSums = ProgSummaries(monthStart, monthEnd, campaignId: campId);
            var progVendors = progSums.Select(s => s.ProgVendor).Distinct().OrderBy(v => v.Name).ToList();
            foreach (var progVendor in progVendors)
            {
                var vendorProgSums = progSums.Where(s => s.ProgVendorId == progVendor.Id);
                var budgetInfoVals = progCampaign.ProgVendorBudgetInfoFor(monthStart, progVendor.Id, useParentValsIfNone: true);
                var vendorStat = new TDMediaStatWithBudget(vendorProgSums, budgetInfoVals)
                {
                    //ProgVendor = progVendor
                };
                vendorStats.Add(vendorStat);
            }

            // Get Extra Items

            // Wrap them up (into a ProgCampStat?)

            return null;
        }

        // ---

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                    context.Dispose();
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
