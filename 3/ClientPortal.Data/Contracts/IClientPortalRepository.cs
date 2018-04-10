using System;
using System.Collections.Generic;
using System.Linq;
using ClientPortal.Data.Contexts;
using ClientPortal.Data.DTOs;

namespace ClientPortal.Data.Contracts
{
    public interface IClientPortalRepository : IDisposable
    {
        void SaveChanges();

        IQueryable<Offer> Offers(int? advertiserId, DateTime? start = null, DateTime? end = null);
        IQueryable<Offer> Offers(int? accountManagerId, int? advertiserId, bool cpmOnly, int? minCampaigns = null);
        Offer GetOffer(int id);
        IQueryable<Campaign> Campaigns(int? offerId, bool cpmOnly);
        Campaign GetCampaign(int id);

        IQueryable<Creative> Creatives(int? offerId);
        Creative GetCreative(int id);
        bool SaveCreative(Creative creative, bool saveChanges = false);
        void FillExtended_Creative(Creative creative);
        IQueryable<CreativeFile> CreativeFiles(int? creativeId);

        IQueryable<CPMReport> CPMReports(int? offerId);
        CPMReport GetCPMReport(int id, bool includeAdvertiser = false);
        void SaveCPMReport(CPMReport inReport, bool saveChanges = false);
        bool AddDropToCPMReport(int cpmReportId, int campaignDropId, bool saveChanges = false);
        bool RemoveDropFromCPMReport(int cpmReportId, int campaignDropId, bool saveChanges = false);
        void FillExtended_CPMReport(CPMReport inReport);
        Offer DeleteCPMReport(int reportId, bool saveChanges = false);
        IQueryable<CampaignDrop> CampaignDropsNotInReport(int reportId);

        IQueryable<CampaignDrop> CampaignDrops(int? offerId, int? campaignId, bool includeCopies = false);
        CampaignDrop GetCampaignDrop(int id);
        CampaignDrop AddCampaignDrop(int campaignId, DateTime date, bool saveChanges = false);
        bool AddCampaignDrop(CampaignDrop campaignDrop, int? creativeId, bool saveChanges = false);
        bool SaveCampaignDrop(CampaignDrop campaignDrop, bool saveChanges = false);
        void FillExtended_CampaignDrop(CampaignDrop campaignDrop);
        Campaign DeleteCampaignDrop(int campaignDropId, bool saveChanges = false);
        void DeleteCampaignDropCopy(int campaignDropId);
        bool DuplicateDropIfNecessary(CampaignDrop drop);

        bool AddCreativeStat(int campaignDropId, int creativeId, bool saveChanges = false);
        bool AddCreativeStat(CreativeStat creativeStat, bool saveChanges = false);
        int? DeleteCreativeStat(int creativeStatId, bool saveChanges = false);
        bool UpdateCreativeStatFromSummaries(int creativeStatId, bool saveChanges = false);
        void UpdateCreativeStatsFromSummaries(IEnumerable<CreativeStat> creativeStats, bool saveChanges = false);

        IQueryable<Advertiser> Advertisers { get; }
        IQueryable<Contact> Contacts { get; }
        void AddAdvertiser(Advertiser entity);
        void AddContact(Contact entity);
        Advertiser GetAdvertiser(int id);
        Contact GetContact(int id);
        Contact GetContact(string search);

        IQueryable<FileUpload> GetFileUploads(int? advertiserId);
        FileUpload GetFileUpload(int id);
        void AddFileUpload(FileUpload fileUpload, bool saveChanges = false);
        void DeleteFileUpload(FileUpload fileUpload, bool saveChanges = false);

        IQueryable<SimpleReport> SimpleReports { get; }
        IQueryable<SimpleReport> SearchSimpleReports { get; }
        SimpleReport GetSimpleReport(int id);
        bool InitializeSimpleReport(int id, bool saveChanges = false);
        void FillExtended_SimpleReport(SimpleReport report);

        IQueryable<Goal> Goals { get; }
        IQueryable<Goal> GetGoals(int advertiserId);
        Goal GetGoal(int id);
        void AddGoal(Goal goal, bool saveChanges = false);
        bool DeleteGoal(int id, int? advertiserId);

        IQueryable<ClientInfo> ClientInfos();
        ClientInfo GetClientInfo(int id);
        void AddClientInfo(ClientInfo clientInfo, bool saveChanges = false);

        IQueryable<UserProfile> UserProfiles();
        UserProfile GetUserProfile(int userId);
        UserProfile GetUserProfile(string userName);

        IQueryable<UserEvent> UserEvents { get; }
        void AddUserEvent(UserEvent userEvent, bool saveChanges = false);
        void AddUserEvent(string userName, string eventString, bool saveChanges = false);
        void AddUserEvent(int userId, string eventString, bool saveChanges = false);

        // Stats
        //IQueryable<DailySummary> GetDailySummaries(DateTime? start, DateTime? end, int? advertiserId, int? offerId, out string currency);
        DateRangeSummary GetDateRangeSummary(DateTime? start, DateTime? end, int? advertiserId, int? offerId, bool includeConversionData);

        IQueryable<MonthlyInfo> GetMonthlyInfosFromDaily(DateTime? start, DateTime? end, int advertiserId, int? offerId);
        IQueryable<OfferInfo> GetOfferInfos(DateTime? start, DateTime? end, int? advertiserId);
        IQueryable<DailyInfo> GetDailyInfos(DateTime? start, DateTime? end, int? advertiserId, int? offerId = null);
        IQueryable<ConversionInfo> GetConversionInfos(DateTime? start, DateTime? end, int? advertiserId, int? offerId);
        IQueryable<ConversionSummary> GetConversionSummaries(DateTime? start, DateTime? end, int? advertiserId, int? offerId);
        IQueryable<AffiliateSummary> GetAffiliateSummaries(DateTime? start, DateTime? end, int? advertiserId, int? offerId);
        IQueryable<MonthlyInfo> GetMonthlyInfos(string type, DateTime? start, DateTime? end, int? advertiserId);

        IQueryable<Conversion> GetConversions(DateTime? start, DateTime? end, int? advertiserId, int? offerId);
        IList<DeviceClicks> GetClicksByDeviceName(DateTime? start, DateTime? end, int? advertiserId, int? offerId);
        IList<ConversionsByRegion> GetConversionCountsByRegion(DateTime start, DateTime end, int advertiserId);

        void AddConversionData(ConversionData entity);
        IQueryable<ConversionData> ConversionData { get; }

        // Search
        IQueryable<SearchProfile> SearchProfiles(bool includeSearchAccounts = false);
        int MaxSearchProfileId();
        SearchProfile GetSearchProfile(int searchProfileId);
        bool SaveSearchProfile(SearchProfile searchProfile);
        bool CreateSearchProfile(SearchProfile searchProfile);

        IQueryable<SearchAccount> SearchAccounts(int? searchProfileId);
        SearchAccount GetSearchAccount(int searchAccountId);
        bool SaveSearchAccount(SearchAccount searchAccount);
        bool CreateSearchAccount(SearchAccount searchAccount);

        SearchCampaign GetSearchCampaign(int searchCampaignId);

        bool InitializeSearchProfileSimpleReport(int searchProfileId, string email = null);

        // Search Stats
        SearchStat GetSearchStats(SearchProfile sp, DateTime? start, DateTime? end, bool? includeToday, string campaignNameInclude = null, string campaignNameExclude = null);
        void FillSearchAccountStatsRange(SearchAccount searchAccount);
        IQueryable<SearchDailySummary> GetSearchDailySummaries(int? searchProfileId = null, int? searchAccountId = null, DateTime? start = null, DateTime? end = null, bool includeToday = true);
        IQueryable<SearchConvType> GetConversionTypesForWeekStats(SearchProfile sp, int? numWeeks, DateTime? startDate, DateTime? endDate);
        IQueryable<SearchConvType> GetConversionTypesForMonthStats(SearchProfile sp, int? numMonths, DateTime? start, DateTime? end);
        IQueryable<SearchConvType> GetSearchConvTypes(int searchProfileId, int? searchCampaignId = null, DateTime? startDate = null, DateTime? endDate = null);
        SearchConvType GetSearchConvType(int id);
        Dictionary<string, int> MinConvTypeIdLookupByAlias();
        IEnumerable<IDictionary<string, object>> FillInConversionTypeStats(int searchProfileId, IEnumerable<SearchStat> searchStats, Dictionary<string, int> aliasToIdLookup = null);
        IQueryable<SearchStat> GetWeekStats(SearchProfile sp, int? numWeeks, DateTime? startDate, DateTime? endDate, string campaignNameInclude = null, string campaignNameExclude = null);
        IQueryable<WeeklySearchStat> GetCampaignWeekStats2(SearchProfile sp, DateTime startDate, DateTime endDate);
        IQueryable<SearchStat> GetDailyStats(SearchProfile sp, DateTime? start, DateTime? end);
        IQueryable<SearchStat> GetMonthStats(SearchProfile sp, int? numMonths, DateTime? start, DateTime? end, bool yoy = false, string campaignNameInclude = null, string campaignNameExclude = null);
        IQueryable<SearchStat> GetDeviceStats(SearchProfile sp, DateTime start, DateTime end, bool showingCassConvs);
        IQueryable<SearchStat> GetChannelStats(SearchProfile sp, int numWeeks, bool includeToday, bool includeAccountBreakdown, bool includeSearchChannels);
        IQueryable<SearchStat> GetCampaignStats(SearchProfile sp, string channel, DateTime? start, DateTime? end, bool breakdown, bool showingCassConvs, string campaignNameInclude = null, string campaignNameExclude = null);
        IQueryable<SearchStat> GetCampaignStats(SearchProfile sp, int searchAccountId, DateTime? start, DateTime? end, bool breakdown, bool showingCassConvs, string campaignNameInclude = null, string campaignNameExclude = null);
        bool DecreaseCampaignOrders(int searchCampaignId, DateTime start, DateTime end, int by = 1);
    }
}
