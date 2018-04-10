using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities;
using DirectAgents.Domain.Entities.CPSearch;

namespace DirectAgents.Domain.Concrete
{
    public class CPSearchRepository : ICPSearchRepository, IDisposable
    {
        private ClientPortalSearchContext context;

        public CPSearchRepository(ClientPortalSearchContext context)
        {
            this.context = context;
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        public IQueryable<SearchProfile> SearchProfiles(DateTime? activeSince = null)
        {
            var searchProfiles = context.SearchProfiles.AsQueryable();
            if (activeSince.HasValue)
            {
                // First, filter out SPs that don't have any dailysummaries
                searchProfiles = searchProfiles.Where(sp => sp.SearchAccounts.Any(sa => sa.SearchCampaigns.Any(sc => sc.SearchDailySummaries.Any())));
                searchProfiles = searchProfiles.Where(sp =>
                    sp.SearchAccounts.SelectMany(sa => sa.SearchCampaigns).SelectMany(sc => sc.SearchDailySummaries)
                    .OrderByDescending(x => x.Date).FirstOrDefault().Date >= activeSince.Value);
            }
            return searchProfiles;
        }

        public bool SaveSearchProfile(SearchProfile searchProfile, bool saveIfExists = true, bool createIfDoesntExist = false)
        {
            if (context.SearchProfiles.Any(x => x.SearchProfileId == searchProfile.SearchProfileId))
            {
                if (saveIfExists)
                {
                    var entry = context.Entry(searchProfile);
                    entry.State = EntityState.Modified;
                    context.SaveChanges();
                    return true;
                }
            }
            else if (createIfDoesntExist)
            {
                context.SearchProfiles.Add(searchProfile);
                context.SaveChanges();
                return true;
            }
            return false; // not saved/created
        }

        public IEnumerable<SearchAccount> StatsGaugesByChannel()
        {
            var searchAccounts = new List<SearchAccount>();
            var allSDS = DailySummaries().Where(x => x.SearchCampaign.SearchAccountId.HasValue);
            var allSCS = ConvSummaries().Where(x => x.SearchCampaign.SearchAccountId.HasValue);
            var allCDS = CallSummaries().Where(x => x.SearchCampaign.SearchAccountId.HasValue);
            var channels = context.SearchAccounts.Select(sa => sa.Channel).Distinct().OrderBy(ch => ch);
            foreach (var channel in channels)
            {
                var searchAccount = new SearchAccount { Channel = channel };
                var sds = allSDS.Where(x => x.SearchCampaign.SearchAccount.Channel == channel);
                var scs = allSCS.Where(x => x.SearchCampaign.SearchAccount.Channel == channel);
                var cds = allCDS.Where(x => x.SearchCampaign.SearchAccount.Channel == channel);
                SetGauges(searchAccount, sds, scs, cds);
                searchAccounts.Add(searchAccount);
            }
            return searchAccounts;
        }

        public IQueryable<SearchAccount> SearchAccounts(int? spId = null, string channel = null, bool includeGauges = false)
        {
            var searchAccounts = context.SearchAccounts.AsQueryable();
            if (spId.HasValue)
                searchAccounts = searchAccounts.Where(x => x.SearchProfileId == spId.Value);
            if (!String.IsNullOrWhiteSpace(channel))
                searchAccounts = searchAccounts.Where(x => x.Channel == channel);
            if (includeGauges)
                searchAccounts = SetGauges(searchAccounts).AsQueryable();
            return searchAccounts;
        }
        private IEnumerable<SearchAccount> SetGauges(IEnumerable<SearchAccount> searchAccounts)
        {
            foreach (var sa in searchAccounts)
            {
                var sds = DailySummaries(searchAccountId: sa.SearchAccountId);
                var scs = ConvSummaries(searchAccountId: sa.SearchAccountId);
                var cds = CallSummaries(searchAccountId: sa.SearchAccountId);
                SetGauges(sa, sds, scs, cds);
            }
            return searchAccounts;
        }
        private void SetGauges(SearchAccount searchAccount, IQueryable<SearchDailySummary> sds, IQueryable<SearchConvSummary> scs, IQueryable<CallDailySummary> cds)
        {
            if (sds != null && sds.Any())
            {
                searchAccount.MinDaySum = sds.Min(x => x.Date);
                searchAccount.MaxDaySum = sds.Max(x => x.Date);
            }
            if (scs != null && scs.Any())
            {
                searchAccount.MinConvSum = scs.Min(x => x.Date);
                searchAccount.MaxConvSum = scs.Max(x => x.Date);
            }
            if (cds != null && cds.Any())
            {
                searchAccount.MinCallSum = cds.Min(x => x.Date);
                searchAccount.MaxCallSum = cds.Max(x => x.Date);
            }
        }

        public SearchAccount GetSearchAccount(int id)
        {
            return context.SearchAccounts.Find(id);
        }

        public bool SaveSearchAccount(SearchAccount searchAccount, bool createIfDoesntExist = false)
        {
            if (context.SearchAccounts.Any(sa => sa.SearchAccountId == searchAccount.SearchAccountId))
            {
                var entry = context.Entry(searchAccount);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            else if (createIfDoesntExist)
            {
                context.SearchAccounts.Add(searchAccount);
                context.SaveChanges();
                return true;
            }
            return false; // not saved/created
        }

        // ---

        public IQueryable<SearchCampaign> SearchCampaigns(int? spId = null, int? searchAccountId = null, string channel = null, bool includeGauges = false)
        {
            var sc = context.SearchCampaigns.AsQueryable();
            if (spId.HasValue)
                sc = sc.Where(x => x.SearchAccount.SearchProfileId == spId.Value);
            if (searchAccountId.HasValue)
                sc = sc.Where(x => x.SearchAccountId == searchAccountId.Value);
            if (!String.IsNullOrWhiteSpace(channel))
                sc = sc.Where(x => x.SearchAccount.Channel == channel);
            if (includeGauges)
                sc = SetGauges(sc).AsQueryable();
            return sc;
        }
        private IEnumerable<SearchCampaign> SetGauges(IEnumerable<SearchCampaign> searchCampaigns)
        {
            foreach (var sc in searchCampaigns)
            {
                var sds = DailySummaries(searchCampaignId: sc.SearchCampaignId);
                bool any = sds.Any();
                sc.MinDaySum = any ? sds.Min(x => x.Date) : (DateTime?)null;
                sc.MaxDaySum = any ? sds.Max(x => x.Date) : (DateTime?)null;
                var scs = ConvSummaries(searchCampaignId: sc.SearchCampaignId);
                any = scs.Any();
                sc.MinConvSum = any ? scs.Min(x => x.Date) : (DateTime?)null;
                sc.MaxConvSum = any ? scs.Max(x => x.Date) : (DateTime?)null;
                var cds = CallSummaries(searchCampaignId: sc.SearchCampaignId);
                any = cds.Any();
                sc.MinCallSum = any ? cds.Min(x => x.Date) : (DateTime?)null;
                sc.MaxCallSum = any ? cds.Max(x => x.Date) : (DateTime?)null;
            }
            return searchCampaigns;
        }

        public SearchCampaign GetSearchCampaign(int id)
        {
            return context.SearchCampaigns.Find(id);
        }

        // ---

        public IQueryable<SearchDailySummary> DailySummaries(int? spId = null, int? searchAccountId = null, int? searchCampaignId = null)
        {
            var sds = context.SearchDailySummaries.AsQueryable();
            if (spId.HasValue)
            {
                var campIds = context.SearchProfiles.Where(x => x.SearchProfileId == spId.Value).SelectMany(x => x.SearchAccounts)
                                .SelectMany(x => x.SearchCampaigns).Select(x => x.SearchCampaignId).ToArray();
                sds = sds.Where(x => campIds.Contains(x.SearchCampaignId));

                // not sure if this would work or produce efficient sql (2 foreign keys are nullable):
                //sds = sds.Where(x => x.SearchCampaign.SearchAccount.SearchProfileId == spId.Value);
            }
            if (searchAccountId.HasValue)
            {
                var campIds = context.SearchAccounts.Where(x => x.SearchAccountId == searchAccountId.Value)
                                .SelectMany(x => x.SearchCampaigns).Select(x => x.SearchCampaignId).ToArray();
                sds = sds.Where(x => campIds.Contains(x.SearchCampaignId));
            }
            if (searchCampaignId.HasValue)
                sds = sds.Where(x => x.SearchCampaignId == searchCampaignId.Value);
            return sds;
        }
        public IQueryable<SearchConvSummary> ConvSummaries(int? spId = null, int? searchAccountId = null, int? searchCampaignId = null)
        {
            var scs = context.SearchConvSummaries.AsQueryable();
            if (spId.HasValue)
            {
                var campIds = context.SearchProfiles.Where(x => x.SearchProfileId == spId.Value).SelectMany(x => x.SearchAccounts)
                                .SelectMany(x => x.SearchCampaigns).Select(x => x.SearchCampaignId).ToArray();
                scs = scs.Where(x => campIds.Contains(x.SearchCampaignId));
            }
            if (searchAccountId.HasValue)
            {
                var campIds = context.SearchAccounts.Where(x => x.SearchAccountId == searchAccountId.Value)
                                .SelectMany(x => x.SearchCampaigns).Select(x => x.SearchCampaignId).ToArray();
                scs = scs.Where(x => campIds.Contains(x.SearchCampaignId));
            }
            if (searchCampaignId.HasValue)
                scs = scs.Where(x => x.SearchCampaignId == searchCampaignId.Value);
            return scs;
        }
        public IQueryable<CallDailySummary> CallSummaries(int? spId = null, int? searchAccountId = null, int? searchCampaignId = null)
        {
            var cds = context.CallDailySummaries.AsQueryable();
            if (spId.HasValue)
            {
                var campIds = context.SearchProfiles.Where(x => x.SearchProfileId == spId.Value).SelectMany(x => x.SearchAccounts)
                                .SelectMany(x => x.SearchCampaigns).Select(x => x.SearchCampaignId).ToArray();
                cds = cds.Where(x => campIds.Contains(x.SearchCampaignId));
            }
            if (searchAccountId.HasValue)
            {
                var campIds = context.SearchAccounts.Where(x => x.SearchAccountId == searchAccountId.Value)
                                .SelectMany(x => x.SearchCampaigns).Select(x => x.SearchCampaignId).ToArray();
                cds = cds.Where(x => campIds.Contains(x.SearchCampaignId));
            }
            if (searchCampaignId.HasValue)
                cds = cds.Where(x => x.SearchCampaignId == searchCampaignId.Value);
            return cds;
        }

        // ---

        public IQueryable<ClientReport> ClientReports()
        {
            return context.ClientReports;
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
