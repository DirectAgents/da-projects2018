using System;
using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities;
using DirectAgents.Domain.Entities.CPSearch;

namespace DirectAgents.Domain.Abstract
{
    public interface ICPSearchRepository : IDisposable
    {
        void SaveChanges();

        IQueryable<SearchProfile> SearchProfiles(DateTime? activeSince = null);
        bool SaveSearchProfile(SearchProfile searchProfile, bool saveIfExists = true, bool createIfDoesntExist = false);
        IEnumerable<SearchAccount> StatsGaugesByChannel();
        IQueryable<SearchAccount> SearchAccounts(int? spId = null, string channel = null, bool includeGauges = false);
        SearchAccount GetSearchAccount(int id);
        bool SaveSearchAccount(SearchAccount searchAccount, bool createIfDoesntExist = false);
        IQueryable<SearchCampaign> SearchCampaigns(int? spId = null, int? searchAccountId = null, string channel = null, bool includeGauges = false);
        SearchCampaign GetSearchCampaign(int id);

        IQueryable<SearchDailySummary> DailySummaries(int? spId = null, int? searchAccountId = null, int? searchCampaignId = null);
        IQueryable<SearchConvSummary> ConvSummaries(int? spId = null, int? searchAccountId = null, int? searchCampaignId = null);
        IQueryable<CallDailySummary> CallSummaries(int? spId = null, int? searchAccountId = null, int? searchCampaignId = null);

        IQueryable<ClientReport> ClientReports();
    }
}
