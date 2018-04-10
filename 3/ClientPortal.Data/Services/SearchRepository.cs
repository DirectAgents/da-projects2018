using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using ClientPortal.Data.Contexts;
using ClientPortal.Data.DTOs;

namespace ClientPortal.Data.Services
{
    public partial class ClientPortalRepository
    {
        public IQueryable<SearchProfile> SearchProfiles(bool includeSearchAccounts = false)
        {
            if (includeSearchAccounts)
                return context.SearchProfiles.Include(sp => sp.SearchAccounts);
            else
                return context.SearchProfiles;
        }

        public int MaxSearchProfileId()
        {
            int maxId = -1;
            if (context.SearchProfiles.Any())
                maxId = context.SearchProfiles.Max(sp => sp.SearchProfileId);
            return maxId;
        }

        public SearchProfile GetSearchProfile(int searchProfileId)
        {
            var searchProfile = context.SearchProfiles.Find(searchProfileId);
            return searchProfile;
        }
        public bool SaveSearchProfile(SearchProfile searchProfile)
        {
            if (context.SearchProfiles.Any(sp => sp.SearchProfileId == searchProfile.SearchProfileId))
            {
                var entry = context.Entry(searchProfile);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            else
                return false;
        }
        public bool CreateSearchProfile(SearchProfile searchProfile)
        {
            if (context.SearchProfiles.Any(sp => sp.SearchProfileId == searchProfile.SearchProfileId))
                return false;

            context.SearchProfiles.Add(searchProfile);
            context.SaveChanges();
            return true;
        }

        public IQueryable<SearchAccount> SearchAccounts(int? searchProfileId)
        {
            var searchAccounts = context.SearchAccounts.AsQueryable();
            if (searchProfileId.HasValue)
                searchAccounts = searchAccounts.Where(x => x.SearchProfileId == searchProfileId.Value);
            return searchAccounts;
        }
        public SearchAccount GetSearchAccount(int searchAccountId)
        {
            var searchAccount = context.SearchAccounts.Find(searchAccountId);
            return searchAccount;
        }
        public bool SaveSearchAccount(SearchAccount searchAccount)
        {
            if (context.SearchAccounts.Any(sa => sa.SearchAccountId == searchAccount.SearchAccountId))
            {
                var entry = context.Entry(searchAccount);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            else
                return false;
        }
        public bool CreateSearchAccount(SearchAccount searchAccount)
        {
            if (context.SearchAccounts.Any(sa => sa.SearchAccountId == searchAccount.SearchAccountId))
                return false;

            context.SearchAccounts.Add(searchAccount);
            context.SaveChanges();
            return true;
        }

        public SearchCampaign GetSearchCampaign(int searchCampaignId)
        {
            var searchCampaign = context.SearchCampaigns.Find(searchCampaignId);
            return searchCampaign;
        }

        // by default, report goes to first contact
        public bool InitializeSearchProfileSimpleReport(int searchProfileId, string email = null)
        {
            bool success = false;
            var searchProfile = GetSearchProfile(searchProfileId);
            if (searchProfile != null && !searchProfile.SimpleReports.Any())
            {
                if (String.IsNullOrWhiteSpace(email))
                {
                    var spContacts = searchProfile.SearchProfileContactsOrdered;
                    if (spContacts.Any())
                        email = spContacts.First().Contact.Email;
                }
                if (!String.IsNullOrWhiteSpace(email))
                {
                    var simpleReport = new SimpleReport
                    {
                        SearchProfile = searchProfile,
                        Email = email,
                        Enabled = false
                    };
                    simpleReport.InitializePeriodAndNextSend();
                    context.SimpleReports.Add(simpleReport);
                    context.SaveChanges();
                    success = true;
                }
            }
            return success;
        }

        // --- Search Stats ---

        // Returns one searchstat - for the specified SearchProfile and timeframe.
        public SearchStat GetSearchStats(SearchProfile sp, DateTime? start, DateTime? end, bool? includeToday, string campaignNameInclude = null, string campaignNameExclude = null)
        {
            return GetSearchStats(sp.SearchProfileId, start, end, includeToday, sp.UseAnalytics, sp.ShowCalls, sp.RevPerViewThru, campaignNameInclude, campaignNameExclude);
        }
        private SearchStat GetSearchStats(int searchProfileId, DateTime? start, DateTime? end, bool? includeToday, bool useAnalytics, bool includeCalls, decimal revPerViewThru, string campaignNameInclude = null, string campaignNameExclude = null)
        {
            if (!includeToday.HasValue)
                includeToday = true; // only relevant when end is null or end is >= today

            var searchCampaigns = GetSearchCampaigns(null, searchProfileId, null, null, null, campaignNameInclude, campaignNameExclude);
            var summaries = GetSearchDailySummaries(searchCampaigns, null, start, end, includeToday.Value);
            bool any = summaries.Any();
            var searchStat = new SearchStat
            {
                EndDate = end.Value,
                CustomByStartDate = start.Value,
                Impressions = !any ? 0 : summaries.Sum(s => s.Impressions),
                Clicks = !any ? 0 : summaries.Sum(s => s.Clicks),
                Cost = !any ? 0 : summaries.Sum(s => s.Cost),
                ViewThrus = !any ? 0 : summaries.Sum(s => s.ViewThrus),
                RevPerViewThru = revPerViewThru,
                CassConvs = !any ? 0 : summaries.Sum(s => s.CassConvs),
                CassConVal = !any ? 0 : summaries.Sum(s => s.CassConVal)
            };

            if (!useAnalytics)
            {   // Get Orders and Revenue as usual
                if (any)
                {
                    searchStat.Orders = summaries.Sum(s => s.Orders);
                    searchStat.Revenue = summaries.Sum(s => s.Revenue);
                }
            }
            else
            {   // Get Orders and Revenue from GA
                var gaSummaries = GetGoogleAnalyticsSummaries(searchCampaigns, start, end, includeToday.Value);
                if (gaSummaries.Any())
                {
                    searchStat.Orders = gaSummaries.Sum(s => s.Transactions);
                    searchStat.Revenue = gaSummaries.Sum(s => s.Revenue);
                }
            }

            if (includeCalls)
            {
                var callSummaries = GetCallDailySummaries(searchCampaigns, start, end, includeToday.Value);
                if (callSummaries.Any())
                {
                    searchStat.Calls = callSummaries.Sum(s => s.Calls);
                }
            }

            return searchStat;
        }

        private IQueryable<SearchCampaign> GetSearchCampaigns(int? advertiserId, int? searchProfileId, string channel, int? searchAccountId, string channelPrefix, string nameInclude = null, string nameExclude = null, int? searchCampaignId = null)
        {
            var searchCampaigns = context.SearchCampaigns.AsQueryable();

            if (advertiserId.HasValue)
                searchCampaigns = searchCampaigns.Where(c => c.SearchAccount.AdvertiserId == advertiserId.Value);
            if (searchProfileId.HasValue)
                searchCampaigns = searchCampaigns.Where(c => c.SearchAccount.SearchProfileId == searchProfileId.Value);
            if (channel != null)
                searchCampaigns = searchCampaigns.Where(c => c.SearchAccount.Channel == channel);
            if (searchAccountId.HasValue)
            {
                searchCampaigns = searchCampaigns.Where(c =>
                                    (!c.AltSearchAccountId.HasValue && c.SearchAccountId.HasValue && c.SearchAccountId.Value == searchAccountId.Value)
                                 || (c.AltSearchAccountId.HasValue && c.AltSearchAccountId.Value == searchAccountId.Value));
                // (if an AltSearchAccountId is specified, use that instead of the campaign's SearchAccountId)
            }
            if (!String.IsNullOrWhiteSpace(channelPrefix))
                searchCampaigns = searchCampaigns.Where(c => c.SearchCampaignName.StartsWith(channelPrefix));

            if (!String.IsNullOrWhiteSpace(nameInclude))
                searchCampaigns = searchCampaigns.Where(c => c.SearchCampaignName.Contains(nameInclude));
            if (!String.IsNullOrWhiteSpace(nameExclude))
                searchCampaigns = searchCampaigns.Where(c => !c.SearchCampaignName.Contains(nameExclude));

            if (searchCampaignId.HasValue)
                searchCampaigns = searchCampaigns.Where(c => c.SearchCampaignId == searchCampaignId.Value);

            return searchCampaigns;
        }

        public void FillSearchAccountStatsRange(SearchAccount searchAccount)
        {
            var sds = GetSearchDailySummaries(searchAccountId: searchAccount.SearchAccountId);
            if (sds.Any())
            {
                var selDate = sds.Select(x => x.Date);
                searchAccount.EarliestStat = selDate.Min();
                searchAccount.LatestStat = selDate.Max();
            }
        }

        public IQueryable<SearchDailySummary> GetSearchDailySummaries(int? searchProfileId = null, int? searchAccountId = null, DateTime? start = null, DateTime? end = null, bool includeToday = true)
        {
            return GetSearchDailySummaries(null, searchProfileId, null, searchAccountId, null, null, start, end, includeToday);
        }
        private IQueryable<SearchDailySummary> GetSearchDailySummaries(int? advertiserId, int? searchProfileId, string channel, int? searchAccountId, string channelPrefix, string device, DateTime? start, DateTime? end, bool includeToday)
        {
            var searchCampaigns = GetSearchCampaigns(advertiserId, searchProfileId, channel, searchAccountId, channelPrefix);
            return GetSearchDailySummaries(searchCampaigns, device, start, end, includeToday);
        }
        private IQueryable<SearchDailySummary> GetSearchDailySummaries(IQueryable<SearchCampaign> searchCampaigns, string device, DateTime? start, DateTime? end, bool includeToday)
        {
            var summaries = searchCampaigns.SelectMany(c => c.SearchDailySummaries);

            if (!String.IsNullOrEmpty(device))
                summaries = summaries.Where(s => s.Device == device);

            // Filter to start date, if present
            if (start.HasValue)
                summaries = summaries.Where(s => s.Date >= start);

            // When specifying, should the current day be included or just up until and including yesterday?
            if (!includeToday)
            {
                var yesterday = DateTime.Today.AddDays(-1);

                if (!end.HasValue || yesterday < end.Value) // if no end date is present OR end date is at least the current date
                    end = yesterday; // then set end date to yesterday's date
            }

            if (end.HasValue)
                summaries = summaries.Where(s => s.Date <= end);
            return summaries;
        }

        private IQueryable<GoogleAnalyticsSummary> GetGoogleAnalyticsSummaries(int? advertiserId, int? searchProfileId, string channel, int? searchAccountId, string channelPrefix, DateTime? start, DateTime? end, bool includeToday)
        {
            var searchCampaigns = GetSearchCampaigns(advertiserId, searchProfileId, channel, searchAccountId, channelPrefix);
            return GetGoogleAnalyticsSummaries(searchCampaigns, start, end, includeToday);
        }
        private IQueryable<GoogleAnalyticsSummary> GetGoogleAnalyticsSummaries(IQueryable<SearchCampaign> searchCampaigns, DateTime? start, DateTime? end, bool includeToday)
        {
            var summaries = searchCampaigns.SelectMany(c => c.GoogleAnalyticsSummaries);

            if (start.HasValue) summaries = summaries.Where(s => s.Date >= start);
            if (!includeToday)
            {
                var yesterday = DateTime.Today.AddDays(-1);
                if (!end.HasValue || yesterday < end.Value) end = yesterday;
            }
            if (end.HasValue)
                summaries = summaries.Where(s => s.Date <= end);

            return summaries;
        }

        private IQueryable<CallDailySummary> GetCallDailySummaries(int? advertiserId, int? searchProfileId, string channel, int? searchAccountId, string channelPrefix, DateTime? start, DateTime? end, bool includeToday)
        {
            var searchCampaigns = GetSearchCampaigns(advertiserId, searchProfileId, channel, searchAccountId, channelPrefix);
            return GetCallDailySummaries(searchCampaigns, start, end, includeToday);
        }
        private IQueryable<CallDailySummary> GetCallDailySummaries(IQueryable<SearchCampaign> searchCampaigns, DateTime? start, DateTime? end, bool includeToday)
        {
            var summaries = searchCampaigns.SelectMany(c => c.CallDailySummaries);
            return GetCallDailySummariesInner(summaries, start, end, includeToday);
        }
        private IQueryable<CallDailySummary> GetCallDailySummaries(int searchCampaignId, DateTime? start, DateTime? end, bool includeToday)
        {
            var summaries = context.CallDailySummaries.Where(cds => cds.SearchCampaignId == searchCampaignId);
            return GetCallDailySummariesInner(summaries, start, end, includeToday);
        }
        private IQueryable<CallDailySummary> GetCallDailySummariesInner(IQueryable<CallDailySummary> summaries, DateTime? start, DateTime? end, bool includeToday)
        {
            if (start.HasValue) summaries = summaries.Where(s => s.Date >= start);
            if (!includeToday)
            {
                var yesterday = DateTime.Today.AddDays(-1);
                if (!end.HasValue || yesterday < end.Value) end = yesterday;
            }
            if (end.HasValue)
                summaries = summaries.Where(s => s.Date <= end);

            return summaries;
        }

        private IQueryable<SearchConvSummary> GetSearchConvSummaries(int? searchProfileId, string channel, int? searchAccountId, string channelPrefix, DateTime? start, DateTime? end, bool includeToday, int? searchCampaignId = null)
        {
            var searchCampaigns = GetSearchCampaigns(null, searchProfileId, channel, searchAccountId, channelPrefix, searchCampaignId: searchCampaignId);
            return GetSearchConvSummaries(searchCampaigns, start, end, includeToday);
        }
        private IQueryable<SearchConvSummary> GetSearchConvSummaries(IQueryable<SearchCampaign> searchCampaigns, DateTime? start, DateTime? end, bool includeToday)
        {
            var summaries = searchCampaigns.SelectMany(c => c.SearchConvSummaries);

            if (start.HasValue) summaries = summaries.Where(s => s.Date >= start);
            if (!includeToday)
            {
                var yesterday = DateTime.Today.AddDays(-1);
                if (!end.HasValue || yesterday < end.Value) end = yesterday;
            }
            if (end.HasValue)
                summaries = summaries.Where(s => s.Date <= end);

            return summaries;
        }

        public IQueryable<SearchConvType> GetConversionTypesForWeekStats(SearchProfile sp, int? numWeeks, DateTime? startDate, DateTime? endDate)
        {
            StartEndDatesForWeekStats(ref startDate, ref endDate, (DayOfWeek)sp.StartDayOfWeek, numWeeks);
            return GetSearchConvTypes(sp.SearchProfileId, null, startDate, endDate);
        }
        public IQueryable<SearchConvType> GetConversionTypesForMonthStats(SearchProfile sp, int? numMonths, DateTime? start, DateTime? end)
        {
            StartEndDatesForMonthStats(ref start, ref end, numMonths);
            return GetSearchConvTypes(sp.SearchProfileId, null, start, end);
        }
        public IQueryable<SearchConvType> GetSearchConvTypes(int searchProfileId, int? searchCampaignId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var convSums = GetSearchConvSummaries(searchProfileId, null, null, null, startDate, endDate, true, searchCampaignId: searchCampaignId);
            return convSums.Select(cs => cs.SearchConvType).Distinct();
        }
        public SearchConvType GetSearchConvType(int id)
        {
            var searchConvType = context.SearchConvTypes.Find(id);
            return searchConvType;
        }

        // a lookup based on all convTypes in the db...
        public Dictionary<string, int> MinConvTypeIdLookupByAlias()
        {
            return context.SearchConvTypes.GroupBy(ct => ct.Alias).ToDictionary<IGrouping<string, SearchConvType>, string, int>(
                g => g.Key,
                g => g.Min(ct => ct.SearchConvTypeId)
                );
        }

        //...grouped by convType alias, with the minimum ConvType id that has that alias - as the group key
        private IDictionary<string, object> GetConversionTypeStats(int searchProfileId, int? searchCampaignId, DateTime? startDate, DateTime? endDate, Dictionary<string, int> aliasToIdLookup = null)
        {
            if (aliasToIdLookup == null)
                aliasToIdLookup = MinConvTypeIdLookupByAlias();
            var convDict = new Dictionary<string, object>();

            var convSums = GetSearchConvSummaries(searchProfileId, null, null, null, startDate, endDate, true, searchCampaignId: searchCampaignId);
            var ctAliasGroups = convSums.GroupBy(cs => cs.SearchConvType.Alias);
            foreach (var ctAliasGroup in ctAliasGroups)
            {
                var id = aliasToIdLookup[ctAliasGroup.Key];
                convDict["conv" + id] = ctAliasGroup.Sum(cs => cs.Conversions);
                convDict["cval" + id] = ctAliasGroup.Sum(cs => cs.ConVal);
            }
            return convDict;
        }
        public IEnumerable<IDictionary<string, object>> FillInConversionTypeStats(int searchProfileId, IEnumerable<SearchStat> searchStats, Dictionary<string, int> aliasToIdLookup = null)
        {
            if (aliasToIdLookup == null)
                aliasToIdLookup = MinConvTypeIdLookupByAlias();
            var ssDicts = new List<IDictionary<string, object>>();
            foreach (var stat in searchStats)
            {
                // For each searchStat, convert it to a dictionary and add the convType stats
                var statDict = stat.ToDictionary();
                var convStats = GetConversionTypeStats(searchProfileId, stat.CampaignId, stat.StartDate, stat.EndDate, aliasToIdLookup: aliasToIdLookup);
                foreach (var key in convStats.Keys)
                {
                    statDict[key] = convStats[key];
                }
                ssDicts.Add(statDict);
            }
            return ssDicts;
        }

        // Fill in startDate and endDate if null
        private void StartEndDatesForWeekStats(ref DateTime? startDate, ref DateTime? endDate, DayOfWeek startDayOfWeek, int? numWeeks)
        {
            if (!endDate.HasValue)
            {
                // Start with yesterday and see if it's the end of a week; if not, go back until it is
                endDate = DateTime.Today.AddDays(-1);
                while (endDate.Value.AddDays(1).DayOfWeek != startDayOfWeek)
                    endDate = endDate.Value.AddDays(-1);
            }   // We are now at the end of the most recent full week

            if (!startDate.HasValue)
            {
                startDate = endDate.Value;
                while (startDate.Value.DayOfWeek != startDayOfWeek) // go to the beginning of the week
                    startDate = startDate.Value.AddDays(-1);
                if (numWeeks.HasValue)
                    startDate = startDate.Value.AddDays(-7 * (numWeeks.Value - 1)); // then back additional weeks, as needed
            }
        }

        // if endDate is null, goes up to the latest complete week
        public IQueryable<SearchStat> GetWeekStats(SearchProfile sp, int? numWeeks, DateTime? startDate, DateTime? endDate, string campaignNameInclude = null, string campaignNameExclude = null)
        {
            return GetWeekStats(null, sp.SearchProfileId, null, null, null, null, (DayOfWeek)sp.StartDayOfWeek, numWeeks, startDate, endDate, sp.UseAnalytics, sp.ShowCalls, sp.RevPerViewThru, campaignNameInclude, campaignNameExclude);
        }
        private IQueryable<SearchStat> GetWeekStats(int? advertiserId, int? searchProfileId, string channel, int? searchAccountId, string channelPrefix, string device, DayOfWeek startDayOfWeek, int? numWeeks, DateTime? startDate, DateTime? endDate, bool useAnalytics, bool includeCalls, decimal revPerViewThru, string campaignNameInclude = null, string campaignNameExclude = null)
        {
            if (!String.IsNullOrWhiteSpace(device) && useAnalytics)
                throw new Exception("specifying a device and useAnalytics not supported");

            StartEndDatesForWeekStats(ref startDate, ref endDate, startDayOfWeek, numWeeks);

            var searchCampaigns =
                GetSearchCampaigns(advertiserId, searchProfileId, channel, searchAccountId, channelPrefix, campaignNameInclude, campaignNameExclude);

            var daySums =
                GetSearchDailySummaries(searchCampaigns, device, startDate, endDate, true)
                    .GroupBy(s => s.Date)
                    .Select(g => new SearchSummary
                    {
                        Date = g.Key,
                        Impressions = g.Sum(x => x.Impressions),
                        Clicks = g.Sum(x => x.Clicks),
                        Orders = g.Sum(x => x.Orders),
                        ViewThrus = g.Sum(x => x.ViewThrus),
                        CassConvs = g.Sum(x => x.CassConvs),
                        CassConVal = g.Sum(x => x.CassConVal),
                        Revenue = g.Sum(x => x.Revenue),
                        Cost = g.Sum(x => x.Cost)
                    })
                    .ToList();

            if (useAnalytics)
            {
                var gaSums = GetGoogleAnalyticsSummaries(searchCampaigns, startDate, endDate, true)
                    .GroupBy(s => s.Date)
                    .Select(g => new AnalyticsSummary
                    {
                        Date = g.Key,
                        Transactions = g.Sum(x => x.Transactions),
                        Revenue = g.Sum(x => x.Revenue)
                    }).ToList();
                // note: can we do this without ToList'ing?

                daySums = (from daySum in daySums
                           join ga in gaSums on daySum.Date equals ga.Date into gj_sums
                           from gaSum in gj_sums.DefaultIfEmpty() // left join to gaSums
                           select new SearchSummary
                           {
                               Date = daySum.Date,
                               Impressions = daySum.Impressions,
                               Clicks = daySum.Clicks,
                               Orders = (gaSum == null) ? 0 : gaSum.Transactions,
                               ViewThrus = daySum.ViewThrus,
                               CassConvs = daySum.CassConvs,
                               CassConVal = daySum.CassConVal,
                               Revenue = (gaSum == null) ? 0 : gaSum.Revenue,
                               Cost = daySum.Cost
                           }).ToList();
            }

            if (includeCalls)
            {
                var callSums = GetCallDailySummaries(searchCampaigns, startDate, endDate, true)
                    .GroupBy(s => s.Date)
                    .Select(g => new CallSummary
                    {
                        Date = g.Key,
                        Calls = g.Sum(x => x.Calls)
                    }).ToList();

                daySums = (from daySum in daySums
                           join ca in callSums on daySum.Date equals ca.Date into gj_sums
                           from callSum in gj_sums.DefaultIfEmpty() // left join to callSums
                           select new SearchSummary
                           {
                               Date = daySum.Date,
                               Impressions = daySum.Impressions,
                               Clicks = daySum.Clicks,
                               Orders = daySum.Orders,
                               ViewThrus = daySum.ViewThrus,
                               CassConvs = daySum.CassConvs,
                               CassConVal = daySum.CassConVal,
                               Revenue = daySum.Revenue,
                               Cost = daySum.Cost,
                               Calls = (callSum == null) ? 0 : callSum.Calls
                           }).ToList();
            }

            var title = channel;
            if (searchAccountId.HasValue)
            {
                var searchAccount = context.SearchAccounts.Find(searchAccountId.Value);
                if (searchAccount != null)
                {
                    title = (title == null) ? "" : title + " - ";
                    title += searchAccount.Name;
                }
            }
            if (!String.IsNullOrWhiteSpace(channelPrefix) || !String.IsNullOrWhiteSpace(device))
            {
                var searchChannel = GetSearchChannel(channelPrefix, device);
                if (title == channel) // no "channel" or searchAccount was specified
                    title = "";       // (don't include "channel" because the searchChannel's name will have it)
                else
                    title += " - "; // just in case both a searchAccount _and_ a searchChannel are specified
                title += (searchChannel != null) ? searchChannel.Name : (channelPrefix ?? ".") + "/" + (device ?? ".");
            }

            var adjuster = new YearWeekAdjuster
            {
                StartDayOfWeek = startDayOfWeek,
                CalendarWeekRule = CalendarWeekRule.FirstFullWeek
            };

            var stats = daySums
                    // Reproject with members that break date up into Year and Week
                    .Select(s => new
                    {
                        Date = s.Date,
                        Year = adjuster.GetYearAdjustedByWeek(s.Date),
                        Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(s.Date, adjuster.CalendarWeekRule, startDayOfWeek),
                        Impressions = s.Impressions,
                        Clicks = s.Clicks,
                        Orders = s.Orders,
                        ViewThrus = s.ViewThrus,
                        CassConvs = s.CassConvs,
                        CassConVal = s.CassConVal,
                        Revenue = s.Revenue,
                        Cost = s.Cost,
                        Calls = s.Calls
                    })

                    // Now group by Year and Week
                    .GroupBy(x => new { x.Year, x.Week })

                    // Order by Year then Week
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Week)

                    // Finally select new SearchStat objects
                    .Select((g, i) => new SearchStat
                    {
                        WeekStartDay = startDayOfWeek,
                        FillToLatest = endDate, // Acts as a boolean except for the most recent week
                        WeekByMaxDate = g.Max(s => s.Date), // Supply the latest date in the group of dates (which are all part of a week)
                        TitleIfNotNull = title, // (if null, Title is "Range")

                        // Re-aggregate since we had to re-group
                        Impressions = g.Sum(s => s.Impressions),
                        Clicks = g.Sum(s => s.Clicks),
                        Orders = g.Sum(s => s.Orders),
                        ViewThrus = g.Sum(s => s.ViewThrus),
                        RevPerViewThru = revPerViewThru,
                        CassConvs = g.Sum(s => s.CassConvs),
                        CassConVal = g.Sum(s => s.CassConVal),
                        Revenue = g.Sum(s => s.Revenue),
                        Cost = g.Sum(s => s.Cost),
                        Calls = g.Sum(s => s.Calls)
                    });

            return stats.AsQueryable();
        }

        // (used for Campaign Weekly report)
        public IQueryable<WeeklySearchStat> GetCampaignWeekStats2(SearchProfile sp, DateTime startDate, DateTime endDate)
        {
            return GetCampaignWeekStats2(sp.SearchProfileId, startDate, endDate, (DayOfWeek)sp.StartDayOfWeek, sp.UseAnalytics, sp.ShowCalls);
        }
        private IQueryable<WeeklySearchStat> GetCampaignWeekStats2(int searchProfileId, DateTime startDate, DateTime endDate, DayOfWeek startDayOfWeek, bool useAnalytics, bool includeCalls)
        {
            var weeks = CalenderWeek.Generate(startDate, endDate, startDayOfWeek);
            var searchCampaigns = GetSearchCampaigns(null, searchProfileId, null, null, null);
            var sums = GetSearchDailySummaries(searchCampaigns, null, startDate, endDate, true)
                .Select(s => new
                {
                    s.Date,
                    s.SearchCampaign.SearchAccount.Channel,
                    s.SearchCampaignId,
                    s.SearchCampaign.SearchCampaignName,
                    s.Orders,
                    s.Revenue,
                    s.Cost
                })
                .AsEnumerable()
                .GroupBy(s => new
                {
                    Week = weeks.First(w => w.EndDate >= s.Date),
                    s.Channel,
                    s.SearchCampaignId,
                    s.SearchCampaignName
                })
                .Select(g => new SearchSummary
                {
                    Week = g.Key.Week,
                    Channel = g.Key.Channel,
                    CampaignId = g.Key.SearchCampaignId,
                    CampaignName = g.Key.SearchCampaignName,
                    Orders = g.Sum(s => s.Orders),
                    Revenue = g.Sum(s => s.Revenue),
                    Cost = g.Sum(s => s.Cost)
                });

            if (useAnalytics)
            {
                var gaStats = GetGoogleAnalyticsSummaries(searchCampaigns, startDate, endDate, true)
                    .AsEnumerable()
                    .GroupBy(s => new
                    {
                        Week = weeks.First(w => w.EndDate >= s.Date),
                        s.SearchCampaignId
                    })
                    .Select(g => new AnalyticsSummary
                    {
                        CampaignId = g.Key.SearchCampaignId,
                        Week = g.Key.Week,
                        Transactions = g.Sum(s => s.Transactions),
                        Revenue = g.Sum(s => s.Revenue)
                    });
                sums = from sum in sums
                       join ga in gaStats on new { sum.CampaignId, sum.Week.StartDate } equals new { ga.CampaignId, ga.Week.StartDate } into gj_stats
                       from gaStat in gj_stats.DefaultIfEmpty() // left join to gaStats
                       select new SearchSummary
                       {
                           Week = sum.Week,
                           Channel = sum.Channel,
                           CampaignId = sum.CampaignId,
                           CampaignName = sum.CampaignName,
                           Orders = (gaStat == null) ? 0 : gaStat.Transactions,
                           Revenue = (gaStat == null) ? 0 : gaStat.Revenue,
                           Cost = sum.Cost
                       };
            }
            if (includeCalls)
            {
                var callStats = GetCallDailySummaries(searchCampaigns, startDate, endDate, true)
                    .AsEnumerable()
                    .GroupBy(s => new
                    {
                        Week = weeks.First(w => w.EndDate >= s.Date),
                        s.SearchCampaignId
                    })
                    .Select(g => new CallSummary
                    {
                        CampaignId = g.Key.SearchCampaignId,
                        Week = g.Key.Week,
                        Calls = g.Sum(s => s.Calls)
                    });
                sums = from sum in sums
                       join ca in callStats on new { sum.CampaignId, sum.Week.StartDate } equals new { ca.CampaignId, ca.Week.StartDate } into gj_stats
                       from callStat in gj_stats.DefaultIfEmpty() // left join to callStats
                       select new SearchSummary
                       {
                           Week = sum.Week,
                           Channel = sum.Channel,
                           CampaignId = sum.CampaignId,
                           CampaignName = sum.CampaignName,
                           Orders = sum.Orders,
                           Revenue = sum.Revenue,
                           Cost = sum.Cost,
                           Calls = (callStat == null) ? 0 : callStat.Calls
                       };
            }

            var stats = sums
                .OrderBy(s => s.Week.StartDate)
                .ThenBy(s => s.CampaignName)
                .Select(s => new WeeklySearchStat
                {
                    StartDate = s.Week.StartDate,
                    EndDate = s.Week.EndDate,
                    Channel = s.Channel,
                    Campaign = s.CampaignName,
                    ROAS = s.Cost == 0 ? 0 : (int)Math.Round(100 * s.Revenue / s.Cost),
                    CPL = (s.Orders + s.Calls == 0) ? 0 : Math.Round(s.Cost / (s.Orders + s.Calls), 2)
                });
            return stats.AsQueryable();
        }

        public IQueryable<SearchStat> GetDailyStats(SearchProfile sp, DateTime? start, DateTime? end)
        {
            return GetDailyStats(sp.SearchProfileId, start, end, sp.RevPerViewThru);
        }
        //TODO: useAnalytics, includeCalls
        public IQueryable<SearchStat> GetDailyStats(int searchProfileId, DateTime? start, DateTime? end, decimal revPerViewThru)
        {
            var searchCampaigns = GetSearchCampaigns(null, searchProfileId, null, null, null);
            var stats = GetSearchDailySummaries(searchCampaigns, null, start, end, true)
                .GroupBy(s => s.Date)
                .Select(g =>
                new SearchStat
                {
                    SingleDate = g.Key,
                    Impressions = g.Sum(s => s.Impressions),
                    Clicks = g.Sum(s => s.Clicks),
                    Orders = g.Sum(s => s.Orders),
                    ViewThrus = g.Sum(s => s.ViewThrus),
                    RevPerViewThru = revPerViewThru,
                    CassConvs = g.Sum(s => s.CassConvs),
                    CassConVal = g.Sum(s => s.CassConVal),
                    Revenue = g.Sum(s => s.Revenue),
                    Cost = g.Sum(s => s.Cost)
                })
                .ToList().AsQueryable();
            return stats.OrderBy(s => s.EndDate);
        }

        private void StartEndDatesForMonthStats(ref DateTime? startDate, ref DateTime? endDate, int? numMonths)
        {
            if (!endDate.HasValue)
                endDate = DateTime.Today.AddDays(-1);
            if (numMonths.HasValue)
                startDate = new DateTime(endDate.Value.Year, endDate.Value.Month, 1).AddMonths((numMonths.Value - 1) * -1);
        }

        //Note: Use one or the other... numMonths or start
        public IQueryable<SearchStat> GetMonthStats(SearchProfile sp, int? numMonths, DateTime? start, DateTime? end, bool yoy = false, string campaignNameInclude = null, string campaignNameExclude = null)
        {
            return GetMonthStats(sp.SearchProfileId, numMonths, start, end, sp.UseAnalytics, sp.ShowCalls, sp.RevPerViewThru, yoy, campaignNameInclude, campaignNameExclude);
        }
        private IQueryable<SearchStat> GetMonthStats(int searchProfileId, int? numMonths, DateTime? start, DateTime? end, bool useAnalytics, bool includeCalls, decimal revPerViewThru, bool yoy = false, string campaignNameInclude = null, string campaignNameExclude = null)
        {
            StartEndDatesForMonthStats(ref start, ref end, numMonths);

            DateTime? origStart = null;
            if (yoy && start.HasValue)
            {
                origStart = start;
                start = start.Value.AddMonths(-12);
            }
            var searchCampaigns = GetSearchCampaigns(null, searchProfileId, null, null, null, campaignNameInclude, campaignNameExclude);
            var stats = GetSearchDailySummaries(searchCampaigns, null, start, end, true)
                .GroupBy(s => new { s.Date.Year, s.Date.Month })
                .Select(g =>
                new SearchStat
                {
                    //Issue?: what if end is the last day of the month, but Max(Date) is not?
                    //        (e.g. no DailySummaries on the last day of the month for some reason)
                    //        RangeStat.Days and OrdersPerDay would be off
                    YoY = yoy,
                    MonthByMaxDate = g.Max(s => s.Date),
                    Impressions = g.Sum(s => s.Impressions),
                    Clicks = g.Sum(s => s.Clicks),
                    Orders = g.Sum(s => s.Orders),
                    ViewThrus = g.Sum(s => s.ViewThrus),
                    RevPerViewThru = revPerViewThru,
                    CassConvs = g.Sum(s => s.CassConvs),
                    CassConVal = g.Sum(s => s.CassConVal),
                    Revenue = g.Sum(s => s.Revenue),
                    Cost = g.Sum(s => s.Cost)
                })
                .ToList().AsQueryable();

            if (useAnalytics)
            {
                var gaStats = GetGoogleAnalyticsSummaries(searchCampaigns, start, end, true)
                    .GroupBy(s => new { s.Date.Year, s.Date.Month })
                    .ToList()
                    .Select(g => new SearchSummary
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Orders = g.Sum(s => s.Transactions),
                        Revenue = g.Sum(s => s.Revenue)
                    });
                stats = (from stat in stats
                         join ga in gaStats on stat.StartDate equals ga.Date into gj_stats
                         from gaStat in gj_stats.DefaultIfEmpty() // left join to gaStats
                         select new SearchStat
                         {
                             YoY = yoy,
                             MonthByMaxDate = stat.EndDate,
                             Impressions = stat.Impressions,
                             Clicks = stat.Clicks,
                             Orders = (gaStat == null) ? 0 : gaStat.Orders,
                             ViewThrus = stat.ViewThrus,
                             RevPerViewThru = revPerViewThru,
                             CassConvs = stat.CassConvs,
                             CassConVal = stat.CassConVal,
                             Revenue = (gaStat == null) ? 0 : gaStat.Revenue,
                             Cost = stat.Cost
                         });
            }

            if (includeCalls)
            {
                var callStats = GetCallDailySummaries(searchCampaigns, start, end, true)
                    .GroupBy(s => new { s.Date.Year, s.Date.Month })
                    .ToList()
                    .Select(g => new CallSummary
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Calls = g.Sum(s => s.Calls)
                    });
                stats = (from stat in stats
                         join ca in callStats on stat.StartDate equals ca.Date into gj_stats
                         from callStat in gj_stats.DefaultIfEmpty() // left join to callStats
                         select new SearchStat
                         {
                             YoY = yoy,
                             MonthByMaxDate = stat.EndDate,
                             Impressions = stat.Impressions,
                             Clicks = stat.Clicks,
                             Orders = stat.Orders,
                             ViewThrus = stat.ViewThrus,
                             RevPerViewThru = revPerViewThru,
                             CassConvs = stat.CassConvs,
                             CassConVal = stat.CassConVal,
                             Revenue = stat.Revenue,
                             Cost = stat.Cost,
                             Calls = (callStat == null) ? 0 : callStat.Calls
                         });
            }

            var orderedStats = stats.ToList().OrderBy(s => s.EndDate).ToList();
            List<SearchStat> finalStats;
            if (yoy)
            {
                finalStats = new List<SearchStat>();

                int iPrev = 0; // for locating previous year's stats
                for (int i = 0; i < orderedStats.Count; i++)
                {
                    if (origStart.HasValue && orderedStats[i].StartDate < origStart.Value)
                        continue;

                    DateTime start_OneYearPrior = orderedStats[i].StartDate.AddYears(-1);
                    orderedStats[i].Title = orderedStats[i].Title +
                        String.Format(" '{0:yy}/'{1:yy}", start_OneYearPrior, orderedStats[i].StartDate); // e.g. "Apr 14/15"

                    // Now attempt to find the previous year's stats
                    while (iPrev < orderedStats.Count && orderedStats[iPrev].StartDate < start_OneYearPrior)
                        iPrev++;
                    if (iPrev < orderedStats.Count && orderedStats[iPrev].StartDate == start_OneYearPrior)
                        orderedStats[i].Prev = new SearchStatVals(orderedStats[iPrev]);

                    finalStats.Add(orderedStats[i]);
                }
            }
            else
            {
                finalStats = orderedStats;
            }

            // check for partial month - if "end" is not the last of the month
            if (end.Value.AddDays(1).Day > 1 && finalStats.Any())
            {   // Essentially, check if there were any stats at all for the partial month
                var lastStat = finalStats.Last();
                if (lastStat.StartDate.Year == end.Value.Year && lastStat.StartDate.Month == end.Value.Month)
                    lastStat.Title = lastStat.Title + " (MTD)";
            }
            return finalStats.AsQueryable();
        }

        // Get a SearchStat summary for each device
        public IQueryable<SearchStat> GetDeviceStats(SearchProfile sp, DateTime start, DateTime end, bool showingCassConvs)
        {
            var searchCampaigns = GetSearchCampaigns(null, sp.SearchProfileId, null, null, null);
            var stats = GetSearchDailySummaries(searchCampaigns, null, start, end, true)
                .GroupBy(s => s.Device)
                .Select(g =>
                new SearchStat
                {
                    EndDate = end,
                    CustomByStartDate = start,
                    DeviceAndTitle = g.Key,
                    Impressions = g.Sum(s => s.Impressions),
                    Clicks = g.Sum(s => s.Clicks),
                    Orders = g.Sum(s => s.Orders),
                    ViewThrus = g.Sum(s => s.ViewThrus),
                    RevPerViewThru = sp.RevPerViewThru,
                    CassConvs = g.Sum(s => s.CassConvs),
                    CassConVal = g.Sum(s => s.CassConVal),
                    Revenue = g.Sum(s => s.Revenue),
                    Cost = g.Sum(s => s.Cost)
                })
                .ToList().Where(s => !s.AllZeros(showingCassConvs)).OrderBy(s => s.Title);

            // Assumption- that there aren't any campaigns that have calls but no other stats for this time period
            if (sp.ShowCalls)
            {
                IQueryable<SearchCampaign> googleCampaigns = null;
                foreach (var searchStat in stats)
                {
                    if (searchStat.Title == ".") // is non-Google
                    {
                        googleCampaigns = GetSearchCampaigns(null, sp.SearchProfileId, "Google", null, null);
                        var googleCampaignIds = googleCampaigns.Select(c => c.SearchCampaignId).ToList();
                        var nonGoogleCampaigns = searchCampaigns.Where(c => !googleCampaignIds.Contains(c.SearchCampaignId));
                        var cds = GetCallDailySummaries(nonGoogleCampaigns, start, end, true);
                        if (cds.Any())
                            searchStat.Calls = cds.Sum(s => s.Calls);
                    }
                    else if (searchStat.Title == "Mobile")
                    {
                        if (googleCampaigns != null) //means there were non-Google stats (set above; "." comes first)
                            searchCampaigns = googleCampaigns;
                        var cds = GetCallDailySummaries(searchCampaigns, start, end, true);
                        if (cds.Any())
                            searchStat.Calls = cds.Sum(s => s.Calls);
                    }
                }
            } // Note: searchCampaigns may have been altered at this point; consider putting the above in a GetCallStats method

            //TODO: includeAnalytics
            return stats.AsQueryable();
        }

        // Get a SearchStat summary for each week for each channel (Google/Bing/etc)... and, if includeAccountBreakdown, each SearchAccount
        public IQueryable<SearchStat> GetChannelStats(SearchProfile sp, int numWeeks, bool includeToday, bool includeAccountBreakdown, bool includeSearchChannels)
        {
            DateTime endDate = includeToday ? DateTime.Today : DateTime.Today.AddDays(-1);

            bool includeMainChannels = true; // (e.g. Google/Bing/etc)
            //if (includeAccountBreakdown)
            //{ // Don't include main channels if we're including a breakdown by account and there is only one main channel
            //    var mainChannels = searchProfile.SearchAccounts.Select(sa => sa.Channel).Distinct();
            //    if (mainChannels.Count() <= 1)
            //        includeMainChannels = false;
            //}
            IQueryable<SearchStat> stats = new List<SearchStat>().AsQueryable();
            if (includeMainChannels)
            {
                var googleStats = GetWeekStats(null, sp.SearchProfileId, "Google", null, null, null, (DayOfWeek)sp.StartDayOfWeek, numWeeks, null, endDate, sp.UseAnalytics, sp.ShowCalls, sp.RevPerViewThru);
                var bingStats = GetWeekStats(null, sp.SearchProfileId, "Bing", null, null, null, (DayOfWeek)sp.StartDayOfWeek, numWeeks, null, endDate, sp.UseAnalytics, sp.ShowCalls, sp.RevPerViewThru);
                var appleStats = GetWeekStats(null, sp.SearchProfileId, "Apple", null, null, null, (DayOfWeek)sp.StartDayOfWeek, numWeeks, null, endDate, sp.UseAnalytics, sp.ShowCalls, sp.RevPerViewThru);
                var criteoStats = GetWeekStats(null, sp.SearchProfileId, "Criteo", null, null, null, (DayOfWeek)sp.StartDayOfWeek, numWeeks, null, endDate, sp.UseAnalytics, sp.ShowCalls, sp.RevPerViewThru);
                stats = googleStats.Concat(bingStats).Concat(appleStats).Concat(criteoStats).AsQueryable();
            }
            if (includeAccountBreakdown)
            {
                var channels = new string[] { "Google", "Bing", "Apple", "Criteo" };
                foreach (var channel in channels)
                {
                    var searchAccounts = sp.SearchAccounts.Where(sa => sa.Channel == channel);
                    if (searchAccounts.Count() > 1)
                    {
                        foreach (var searchAccount in searchAccounts)
                        {
                            var accountStats = GetWeekStats(null, sp.SearchProfileId, channel, searchAccount.SearchAccountId, null, null, (DayOfWeek)sp.StartDayOfWeek, numWeeks, null, endDate, sp.UseAnalytics, sp.ShowCalls, sp.RevPerViewThru);
                            stats = stats.Concat(accountStats);
                        }
                    }
                }
            }
            if (includeSearchChannels)
            {
                foreach (var searchChannel in this.SearchChannels)
                {
                    if (!String.IsNullOrWhiteSpace(searchChannel.Device) && sp.UseAnalytics)
                        continue; // specifying device and useAnalytics not supported; analytics summaries are not broken down by device

                    var channelStats = GetWeekStats(null, sp.SearchProfileId, null, null, searchChannel.Prefix, searchChannel.Device, (DayOfWeek)sp.StartDayOfWeek, numWeeks, null, endDate, sp.UseAnalytics, sp.ShowCalls, sp.RevPerViewThru);
                    if (channelStats.Count() > 0)
                        stats = stats.Concat(channelStats);
                }
            }
            return stats;
        }

        // Get a SearchStat summary for each individual campaign (or if breakdown, one for each Network/Device combo)
        public IQueryable<SearchStat> GetCampaignStats(SearchProfile sp, string channel, DateTime? start, DateTime? end, bool breakdown, bool showingCassConvs, string campaignNameInclude = null, string campaignNameExclude = null)
        {
            if (!start.HasValue)
                start = new DateTime(DateTime.Today.Year, 1, 1);

            string channelPrefix = null;
            string device = null;
            int? searchAccountId = null;
            if (channel != null)
            {
                if (channel.StartsWith("~")) // for a specific SearchChannel
                {
                    var searchChannel = GetSearchChannelByName(channel);
                    channel = null;
                    if (searchChannel != null)
                    {
                        channelPrefix = searchChannel.Prefix;
                        device = searchChannel.Device;
                        if (channelPrefix == null)
                        {
                            if (searchChannel.Name.Contains("Google") && !searchChannel.Name.Contains("Bing"))
                                channel = "Google";
                            if (!searchChannel.Name.Contains("Google") && searchChannel.Name.Contains("Bing"))
                                channel = "Bing";
                        }
                    }
                    else
                    {
                        throw new Exception("SearchChannel not found");
                    }
                }
                else if (channel.Contains(" - ")) // for a specific SearchAccount
                {
                    int i = channel.IndexOf(" - ");
                    string accountName = channel.Substring(i + 3);
                    channel = channel.Substring(0, i);
                    var searchAccount = context.SearchAccounts.SingleOrDefault(sa => sa.Channel == channel && sa.Name == accountName);
                    if (searchAccount != null)
                        searchAccountId = searchAccount.SearchAccountId;
                }
            }
            return GetCampaignStatsInner(sp.SearchProfileId, channel, searchAccountId, channelPrefix, device, start, end, breakdown, sp.UseAnalytics, sp.ShowCalls, sp.RevPerViewThru, showingCassConvs, campaignNameInclude, campaignNameExclude);
        }

        public IQueryable<SearchStat> GetCampaignStats(SearchProfile sp, int searchAccountId, DateTime? start, DateTime? end, bool breakdown, bool showingCassConvs, string campaignNameInclude = null, string campaignNameExclude = null)
        {
            return GetCampaignStatsInner(null, null, searchAccountId, null, null, start, end, breakdown, sp.UseAnalytics, sp.ShowCalls, sp.RevPerViewThru, showingCassConvs, campaignNameInclude, campaignNameExclude);
        }

        private IQueryable<SearchStat> GetCampaignStatsInner(int? searchProfileId, string channel, int? searchAccountId, string channelPrefix, string device, DateTime? start, DateTime? end, bool breakdown, bool useAnalytics, bool includeCalls, decimal revPerViewThru, bool showingCassConvs, string campaignNameInclude = null, string campaignNameExclude = null)
        {
            var searchCampaigns = GetSearchCampaigns(null, searchProfileId, channel, searchAccountId, channelPrefix, campaignNameInclude, campaignNameExclude);
            var summaries = GetSearchDailySummaries(searchCampaigns, device, start, end, true).ToList();
            IQueryable<SearchStat> stats;

            if (breakdown) // by Network and Device
            {
                // TODO: figure out how to join to gaStats if useAnalytics==true

                var summaryGroups = summaries.GroupBy(s => new { s.SearchCampaign.SearchAccount.Channel, s.SearchCampaignId, s.SearchCampaign.SearchCampaignName, s.Network, s.Device });
                if (includeCalls)
                {   // Note: This was written with the Campaign Performance tab in mind.  channel, channelPrefix and device would be null
                    // Also assuming each campaign is under just one network.  If >1 (e.g. Search & Display), the calls will go under the first group.
                    var campaignGroups = summaryGroups.GroupBy(sg => new { sg.Key.SearchCampaignId, sg.Key.Device })
                        .OrderBy(cg => cg.Key.Device);
                    foreach (var campGroup in campaignGroups)
                    {   // "." == non-Google; "M" == Google-Mobile (For Google, we put all the calls under 'mobile')
                        if (campGroup.Key.Device == "." || campGroup.Key.Device == "M")
                        {
                            var callSummaries = GetCallDailySummaries(campGroup.Key.SearchCampaignId, start, end, true);
                            if (callSummaries.Any())
                                campGroup.First().First().Calls = callSummaries.Sum(cds => cds.Calls);
                        }
                    }
                }
                stats = summaryGroups
                    .Select(g => new SearchStat
                    {
                        CampaignId = g.Key.SearchCampaignId,
                        EndDate = end.Value,
                        CustomByStartDate = start.Value,
                        Channel = g.Key.Channel,
                        Title = g.Key.SearchCampaignName,
                        Network = g.Key.Network,
                        Device = g.Key.Device,
                        Impressions = g.Sum(s => s.Impressions),
                        Clicks = g.Sum(s => s.Clicks),
                        Orders = g.Sum(s => s.Orders),
                        ViewThrus = g.Sum(s => s.ViewThrus),
                        RevPerViewThru = revPerViewThru,
                        CassConvs = g.Sum(s => s.CassConvs),
                        CassConVal = g.Sum(s => s.CassConVal),
                        Revenue = g.Sum(s => s.Revenue),
                        Cost = g.Sum(s => s.Cost),
                        Calls = g.Sum(s => s.Calls)
                    }).AsQueryable();
            }
            else // no breakdown
            {
                var sums = summaries.GroupBy(s => new { s.SearchCampaign.SearchAccount.Channel, s.SearchCampaignId, s.SearchCampaign.SearchCampaignName })
                    .Select(g => new SearchSummary
                    {
                        Channel = g.Key.Channel,
                        CampaignId = g.Key.SearchCampaignId,
                        CampaignName = g.Key.SearchCampaignName,
                        Impressions = g.Sum(s => s.Impressions),
                        Clicks = g.Sum(s => s.Clicks),
                        Orders = g.Sum(s => s.Orders),
                        ViewThrus = g.Sum(s => s.ViewThrus),
                        CassConvs = g.Sum(s => s.CassConvs),
                        CassConVal = g.Sum(s => s.CassConVal),
                        Revenue = g.Sum(s => s.Revenue),
                        Cost = g.Sum(s => s.Cost)
                    });
                if (!useAnalytics && !includeCalls)
                {
                    stats = sums//.OrderBy(s => s.Channel).ThenBy(s => s.CampaignName)
                        .Select(s => new SearchStat
                        {
                            CampaignId = s.CampaignId,
                            EndDate = end.Value,
                            CustomByStartDate = start.Value,
                            Channel = s.Channel,
                            Title = s.CampaignName,
                            Impressions = s.Impressions,
                            Clicks = s.Clicks,
                            Orders = s.Orders,
                            ViewThrus = s.ViewThrus,
                            RevPerViewThru = revPerViewThru,
                            CassConvs = s.CassConvs,
                            CassConVal = s.CassConVal,
                            Revenue = s.Revenue,
                            Cost = s.Cost
                        }).AsQueryable();
                }
                else // including analytics and/or calls...
                {
                    stats = null; // to satisfy the compiler
                                  // shouldn't be needed b/c at least one of (useAnalytics and includeCalls) is true, so stats will get assigned
                    if (useAnalytics)
                    {
                        var gaSums = GetGoogleAnalyticsSummaries(searchCampaigns, start, end, true)
                            .GroupBy(s => s.SearchCampaignId)
                            .Select(g => new AnalyticsSummary
                            {
                                CampaignId = g.Key,
                                Transactions = g.Sum(s => s.Transactions),
                                Revenue = g.Sum(s => s.Revenue)
                            }).ToList();
                        stats = (from sum in sums
                                 join ga in gaSums on sum.CampaignId equals ga.CampaignId into gj_sums
                                 from gaSum in gj_sums.DefaultIfEmpty() // left join to gaSums
                                 select new SearchStat
                                 {
                                     CampaignId = sum.CampaignId,
                                     EndDate = end.Value,
                                     CustomByStartDate = start.Value,
                                     Channel = sum.Channel,
                                     Title = sum.CampaignName,
                                     Impressions = sum.Impressions,
                                     Clicks = sum.Clicks,
                                     Orders = (gaSum == null) ? 0 : gaSum.Transactions,
                                     ViewThrus = sum.ViewThrus,
                                     RevPerViewThru = revPerViewThru,
                                     CassConvs = sum.CassConvs,
                                     CassConVal = sum.CassConVal,
                                     Revenue = (gaSum == null) ? 0 : gaSum.Revenue,
                                     Cost = sum.Cost
                                 }).AsQueryable();
                    }
                    if (includeCalls)
                    {
                        var callSums = GetCallDailySummaries(searchCampaigns, start, end, true)
                            .GroupBy(s => s.SearchCampaignId)
                            .Select(g => new CallSummary
                            {
                                CampaignId = g.Key,
                                Calls = g.Sum(s => s.Calls)
                            }).ToList();
                        stats = (from sum in sums
                                 join ca in callSums on sum.CampaignId equals ca.CampaignId into gj_sums
                                 from callSum in gj_sums.DefaultIfEmpty() // left join to callSums
                                 select new SearchStat
                                 {
                                     CampaignId = sum.CampaignId,
                                     EndDate = end.Value,
                                     CustomByStartDate = start.Value,
                                     Channel = sum.Channel,
                                     Title = sum.CampaignName,
                                     Impressions = sum.Impressions,
                                     Clicks = sum.Clicks,
                                     Orders = sum.Orders,
                                     ViewThrus = sum.ViewThrus,
                                     RevPerViewThru = revPerViewThru,
                                     CassConvs = sum.CassConvs,
                                     CassConVal = sum.CassConVal,
                                     Revenue = sum.Revenue,
                                     Cost = sum.Cost,
                                     Calls = (callSum == null) ? 0 : callSum.Calls
                                 }).AsQueryable();
                    }
                }
            }
            return stats.Where(s => !s.AllZeros(showingCassConvs)).OrderBy(s => s.Title);
        }

        // Returns true if was able to decrease by the desired amount
        public bool DecreaseCampaignOrders(int searchCampaignId, DateTime start, DateTime end, int by = 1)
        {
            var summaries = context.SearchDailySummaries.Where(x => x.SearchCampaignId == searchCampaignId && x.Date >= start && x.Date <= end
                                                               && x.Orders > 0);
            if (!summaries.Any())
                return false;

            int decreased = 0;
            foreach (var sds in summaries)
            {
                if (decreased < by)
                {
                    int left = by - decreased;
                    if (sds.Orders >= left)
                    {
                        sds.Orders -= left;
                        decreased += left;
                    }
                    else
                    {
                        decreased += sds.Orders;
                        sds.Orders = 0;
                    }
                    if (decreased == by)
                        break;
                }
            }
            SaveChanges();
            return (decreased == by);
        }

        //public IQueryable<SearchStat> GetAdgroupStats()
        //{
        //    var stats = new List<SearchStat>
        //    {
        //        new SearchStat(true, 2013, 6, 2, 1281, 34, 2, 230m, 31.49m, "Apple Computer Ram"),
        //        new SearchStat(true, 2013, 6, 2, 1002, 23, 0, 0m, 14.13m, "Apple Memory"),
        //        new SearchStat(true, 2013, 6, 2, 1819, 20, 1, 80m, 15.96m, "Apple Memory Module"),
        //        new SearchStat(true, 2013, 6, 2, 1295, 11, 0, 0m, 17.31m, "Apple RAM"),
        //    };
        //    return stats.AsQueryable();
        //}

        // --- Private methods ---

        private List<SearchChannel> _searchChannels;
        private List<SearchChannel> SearchChannels
        {
            get
            {
                if (_searchChannels == null)
                    _searchChannels = context.SearchChannels.ToList();
                return _searchChannels;
            }
        }
        private SearchChannel GetSearchChannel(string prefix, string device)
        {
            var searchChannel = this.SearchChannels.SingleOrDefault(sc => sc.Prefix == prefix && sc.Device == device);
            return searchChannel;
        }
        private SearchChannel GetSearchChannelByName(string name)
        {
            var searchChannel = this.SearchChannels.SingleOrDefault(sc => sc.Name == name);
            return searchChannel;
        }

    }

    public class YearWeekAdjuster
    {
        public DayOfWeek StartDayOfWeek { get; set; }
        public CalendarWeekRule CalendarWeekRule { get; set; }

        public int GetYearAdjustedByWeek(DateTime date)
        {
            int year = date.Year;
            if (date.Month == 1) // adjustment only needed in January
            {
                int week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, this.CalendarWeekRule, this.StartDayOfWeek);
                if (week >= 52)
                    year = year - 1;
            }
            return year;
        }
    }

    class AnalyticsSummary
    {
        public int CampaignId { get; set; }
        public DateTime Date { get; set; }
        public CalenderWeek Week { get; set; }
        public int Transactions { get; set; }
        public decimal Revenue { get; set; }
    }

    class CallSummary
    {
        public int CampaignId { get; set; }
        public DateTime Date { get; set; }
        public CalenderWeek Week { get; set; }
        public string Device { get; set; }
        public int Calls { get; set; }
    }

    class SearchSummary
    {
        public string Channel { get; set; }
        public int CampaignId { get; set; }
        public string CampaignName { get; set; }
        public DateTime Date { get; set; }
        public CalenderWeek Week { get; set; }
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Orders { get; set; }
        public int ViewThrus { get; set; }
        public int CassConvs { get; set; }
        public double CassConVal { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public int Calls { get; set; }
    }
}