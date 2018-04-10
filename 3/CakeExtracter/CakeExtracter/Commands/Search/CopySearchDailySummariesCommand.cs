using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Common;
using CakeExtracter.Etl.SearchMarketing.Extracters;
using CakeExtracter.Etl.SearchMarketing.Loaders;
using ClientPortal.Data.Contexts;
using DirectAgents.Domain.Contexts;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class CopySearchDailySummariesCommand : ConsoleCommand
    {
        public int FromSearchAccountId { get; set; }
        public int ToSearchAccountId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public override void ResetProperties()
        {
            FromSearchAccountId = -1;
            ToSearchAccountId = -1;
            StartDate = null;
            EndDate = null;
        }

        public CopySearchDailySummariesCommand()
        {
            IsCommand("copySearchDailySummaries", "copy SearchDailySummaries");
            HasRequiredOption<int>("a|fromSearchAccountId=", "FROM SearchAccount Id", c => FromSearchAccountId = c);
            HasRequiredOption<int>("b|toSearchAccountId=", "TO SearchAccount Id", c => ToSearchAccountId = c);
            HasOption<DateTime>("s|startDate=", "Start Date (default is the 1st of the month <= yesterday)", c => StartDate = c);
            HasOption<DateTime>("e|endDate=", "End Date (default is yesterday)", c => EndDate = c);
        }

        // Copy SDS's from one database (ClientPortalContext) to another (ClientPortalSearchContext)
        // Only copies SDS's whose SearchCampaign has a non-null ExternalId

        //TODO: Handle null ExternalIds.  Match on SearchCampaignName?

        public override int Execute(string[] remainingArguments)
        {
            var yesterday = DateTime.Today.AddDays(-1);
            var firstOfMonth = new DateTime(yesterday.Year, yesterday.Month, 1);
            var dateRange = new DateRange(StartDate ?? firstOfMonth, EndDate ?? yesterday);
            Logger.Info("Copy SearchDailySummaries from SearchAccount ({0}) to ({1}). DateRange {2}.", FromSearchAccountId, ToSearchAccountId, dateRange);

            CopySearchCampaigns(FromSearchAccountId, ToSearchAccountId);

            var extracter = new DbSearchDailySummaryByCampaignExtracter(FromSearchAccountId, dateRange, includeNullSearchCampaignExternalIds: false);
            var loader = new SearchDailySummaryFromOldDbLoader(ToSearchAccountId);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            return 0;
        }

        private static void CopySearchCampaigns(int fromAccountId, int toAccountId)
        {
            List<SearchCampaign> searchCampaigns;
            using (var fromDb = new ClientPortalContext())
            {
                searchCampaigns = fromDb.SearchCampaigns.Where(sc => sc.SearchAccountId == fromAccountId && sc.ExternalId != null &&
                                                                     sc.SearchDailySummaries.Any()).ToList();
            }

            using (var toDb = new ClientPortalSearchContext())
            {
                var existingSearchCampaigns = toDb.SearchCampaigns.Where(sc => sc.SearchAccountId == toAccountId && sc.ExternalId != null);
                var existingExternalIds = existingSearchCampaigns.Select(x => x.ExternalId.Value).ToArray();

                foreach (var sc in searchCampaigns)
                {
                    if (!existingExternalIds.Contains(sc.ExternalId.Value))
                    {
                        var newSearchCampaign = new DirectAgents.Domain.Entities.CPSearch.SearchCampaign
                        {
                            SearchAccountId = toAccountId,
                            SearchCampaignName = sc.SearchCampaignName,
                            Channel = sc.Channel,
                            ExternalId = sc.ExternalId,
                            LCcmpid = sc.LCcmpid
                        };
                        toDb.SearchCampaigns.Add(newSearchCampaign);
                    }
                }
                toDb.SaveChanges();
            }
        }
    }
}
