using System;
using System.Collections.Generic;
using System.Linq;
using ClientPortal.Data.Contexts;
using ClientPortal.Data.Contracts;
using ClientPortal.Data.DTOs;

namespace DAGenerators.Spreadsheets
{
    public class GeneratorCP
    {
        public static SearchReportPPC GenerateSearchReport(IClientPortalRepository cpRepo, string templateFolder, int searchProfileId, int numWeeks, int numMonths, DateTime endDate, bool groupBySearchAccount, string campaignNameInclude = null, string campaignNameExclude = null)
        {
            var searchProfile = cpRepo.GetSearchProfile(searchProfileId);
            if (searchProfile == null)
                return null;

            var profileAbbrev = searchProfile.SearchProfileName.Replace(" ", "");
            var className = "SearchReport_" + profileAbbrev;

            SearchReportPPC spreadsheet;
            var baseType = typeof(SearchReportPPC);
            var type = baseType.Assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(baseType) && t.Name == className);

            if (type == null)
            {
                if (searchProfile.ShowRevenue) // eCom/Retail
                {
                    spreadsheet = new SearchReportPPC();
                }
                else // LeadGen
                {
                    if (searchProfile.ShowCalls)
                        spreadsheet = new SearchReportLeadGenWithCalls();
                    else
                        spreadsheet = new SearchReportLeadGen();
                }
            }
            else // custom
            {
                spreadsheet = (SearchReportPPC)Activator.CreateInstance(type);
            }

            spreadsheet.Setup(templateFolder);
            if (!searchProfile.ShowViewThrus)
            {
                spreadsheet.MakeColumnHidden(spreadsheet.Metrics1.ViewThrus);
                spreadsheet.MakeColumnHidden(spreadsheet.Metrics1.ViewThruRev);
            }
            if (!searchProfile.ShowCassConvs)
            {
                spreadsheet.MakeColumnHidden(spreadsheet.Metrics1.CassConvs);
                spreadsheet.MakeColumnHidden(spreadsheet.Metrics1.CassConVal);
            }
            spreadsheet.SetReportDate(endDate);
            spreadsheet.SetClientName(searchProfile.SearchProfileName);

            //bool partialMonth = (endDate.AddDays(1).Day > 1); // it's a full month if the day after endDate is the 1st

            var weeklyStats = cpRepo.GetWeekStats(searchProfile, numWeeks, null, endDate, campaignNameInclude, campaignNameExclude);
            var monthlyStats = cpRepo.GetMonthStats(searchProfile, numMonths, null, endDate, false, campaignNameInclude, campaignNameExclude);
            spreadsheet.LoadWeeklyStats(weeklyStats);
            spreadsheet.LoadMonthlyStats(monthlyStats);

            if (weeklyStats.Count() > 0)
                spreadsheet.CreateCharts(true);
            else if (monthlyStats.Count() > 0)
                spreadsheet.CreateCharts(false);


            // YearOverYear full stats - for the YoY sheet/tab
            //int numMonthsYoY = numMonths; // (numMonths > 12) ? 12 : numMonths;
            int numMonthsYoY = (numMonths > 12) ? 12 : numMonths;
            var yoyMonthlyStats = cpRepo.GetMonthStats(searchProfile, numMonthsYoY, null, endDate, true, campaignNameInclude, campaignNameExclude);
            spreadsheet.LoadYearOverYear_Full(yoyMonthlyStats);


            // Year-Over-Year - for the most recent completed month
            var monthToUse = endDate.AddDays(1).AddMonths(-1); // handles the case where endDate is the last of the month
            DateTime monthStart = new DateTime(monthToUse.Year, monthToUse.Month, 1);
            DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthStats = cpRepo.GetSearchStats(searchProfile, monthStart, monthEnd, false, campaignNameInclude, campaignNameExclude);
            monthStats.Title = monthStart.ToString("MMM-yy");

            monthStart = monthStart.AddYears(-1);
            monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var monthStatsLastYear = cpRepo.GetSearchStats(searchProfile, monthStart, monthEnd, false, campaignNameInclude, campaignNameExclude);
            monthStatsLastYear.Title = monthStart.ToString("MMM-yy");

            var yoyStats_Summary = new[] { monthStatsLastYear, monthStats };
            spreadsheet.LoadYearOverYear_Summary(yoyStats_Summary);


            // Week-Over-Week - for the most recent full week given the StartDayOfWeek setting
            DateTime weekEnd = endDate;
            while (weekEnd.AddDays(1).DayOfWeek != (DayOfWeek)searchProfile.StartDayOfWeek) // go to the beginning of the week
                weekEnd = weekEnd.AddDays(-1);
            DateTime weekStart = weekEnd.AddDays(-6);

            var weekStats = cpRepo.GetSearchStats(searchProfile, weekStart, weekEnd, false, campaignNameInclude, campaignNameExclude);
            weekStats.Title = weekStart.ToString("MM/dd") + " - " + weekEnd.ToString("MM/dd");

            weekStart = weekStart.AddDays(-7);
            weekEnd = weekStart.AddDays(6);
            var weekStatsLastWeek = cpRepo.GetSearchStats(searchProfile, weekStart, weekEnd, false, campaignNameInclude, campaignNameExclude);
            weekStatsLastWeek.Title = weekStart.ToString("MM/dd") + " - " + weekEnd.ToString("MM/dd");

            var wowStats_Summary = new[] { weekStatsLastWeek, weekStats };
            spreadsheet.LoadWeekOverWeek_Summary(wowStats_Summary);

            // Monthly campaign performance stats...
            // start with the most recent and go back the number of months specified
            var periodStart = new DateTime(endDate.Year, endDate.Month, 1); // (the first could be a partial month)
            var periodEnd = endDate;
            for (int i = 0; i < numMonths; i++)
            {
                var campaignStatsDict = GetOneCampaignStatsDict(cpRepo, searchProfile, periodStart, periodEnd, groupBySearchAccount, campaignNameInclude, campaignNameExclude);

                if (campaignStatsDict.Keys.Any()) // TODO: if empty, somehow generate a row with zeros for this month
                {
                    bool collapse = (i > 0);
                    spreadsheet.LoadMonthlyCampaignPerfStats(campaignStatsDict, collapse, periodStart, periodEnd);
                }
                periodEnd = periodStart.AddDays(-1);
                periodStart = periodStart.AddMonths(-1);
            }
            spreadsheet.CampaignPerfStatsCleanup(true);

            // Weekly campaign performance stats...
            periodStart = endDate; // Start with the week that includes endDate (could be a partial week)
            while (periodStart.DayOfWeek != (DayOfWeek)searchProfile.StartDayOfWeek)
                periodStart = periodStart.AddDays(-1);
            periodEnd = endDate;

            spreadsheet.SetReportingPeriod(periodStart, periodEnd); // currently just used for Teacher Express template
            // TODO: implement showing reporting period as latest week or latest month

            // Load the weekly campaign stats, starting with the most recent and going back the number of weeks specified
            for (int i = 0; i < numWeeks; i++)
            {
                var campaignStatsDict = GetOneCampaignStatsDict(cpRepo, searchProfile, periodStart, periodEnd, groupBySearchAccount, campaignNameInclude, campaignNameExclude);

                if (campaignStatsDict.Keys.Any()) // TODO: if empty, somehow generate a row with zeros for this week
                {
                    bool collapse = (i > 0);
                    spreadsheet.LoadWeeklyCampaignPerfStats(campaignStatsDict, collapse, periodStart, periodEnd);
                }
                periodEnd = periodStart.AddDays(-1);
                periodStart = periodStart.AddDays(-7);
            }
            spreadsheet.CampaignPerfStatsCleanup(false);


            // Display vs Search tab
            //TODO: be able to use campaignNameInclude if passed in (combine terms)
            var weeklyStats_Display = cpRepo.GetWeekStats(searchProfile, numWeeks, null, endDate, campaignNameInclude: "display");
            var weeklyStats_Search = cpRepo.GetWeekStats(searchProfile, numWeeks, null, endDate, campaignNameExclude: "display");
            spreadsheet.LoadWeeklyDisplayStats(weeklyStats_Display);
            spreadsheet.LoadWeeklySearchStats(weeklyStats_Search);

            return spreadsheet;
        }

        // Get stats for one week/month/etc - grouped by channel or searchAccount
        public static Dictionary<string, IEnumerable<SearchStat>> GetOneCampaignStatsDict(IClientPortalRepository cpRepo, SearchProfile searchProfile, DateTime periodStart, DateTime periodEnd, bool groupBySearchAccount, string campaignNameInclude = null, string campaignNameExclude = null)
        {
            var campaignStatsDict = new Dictionary<string, IEnumerable<SearchStat>>();

            //int numChannels = searchProfile.SearchAccounts.Select(sa => sa.Channel).Distinct().Count();
            if (groupBySearchAccount)
            {
                foreach (var searchAccount in searchProfile.SearchAccounts)
                {
                    var campaignStats = cpRepo.GetCampaignStats(searchProfile, searchAccount.SearchAccountId, periodStart, periodEnd, false, searchProfile.ShowCassConvs, campaignNameInclude, campaignNameExclude);
                    if (campaignStats.Any())
                        campaignStatsDict[searchAccount.Name] = SortCampaignStats(searchProfile, campaignStats);
                }
            }
            else // group campaigns by channel (Google, Bing, etc)
            {
                var campaignStats = cpRepo.GetCampaignStats(searchProfile, null, periodStart, periodEnd, false, searchProfile.ShowCassConvs, campaignNameInclude, campaignNameExclude);
                var channels = campaignStats.Select(s => s.Channel).Distinct();
                foreach (string channel in channels)
                {
                    campaignStatsDict[channel] = SortCampaignStats(searchProfile, campaignStats.Where(s => s.Channel == channel));
                    //int maxRows = 582; // 584 was too much (with 5 weeks, 0 month... btw, 6 weeks, 0 months worked)
                    //if (campaignStatsDict[channel].Count() > maxRows)
                    //    campaignStatsDict[channel] = campaignStatsDict[channel].Take(maxRows);
                }
            }
            return campaignStatsDict;
        }

        private static IEnumerable<SearchStat> SortCampaignStats(SearchProfile searchProfile, IQueryable<SearchStat> campaignStats)
        {
            if (searchProfile.ShowRevenue)
                return campaignStats.OrderByDescending(s => s.Revenue).ThenByDescending(s => s.Cost).ThenByDescending(s => s.Impressions).ToList();
            else
                return campaignStats.ToList(); // already ordered by title (campaign name)
        }

        // DisposeResources when done with SearchReport?
    }
}
