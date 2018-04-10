using System.Collections.Generic;
using System.Linq;
using CakeExtracter.Common;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Etl.SearchMarketing.Extracters
{
    public class DbSearchDailySummaryByCampaignExtracter : Extracter<SearchCampaign>
    {
        private readonly int searchAccountId;
        protected readonly DateRange dateRange;
        protected readonly bool includeNullSearchCampaignExternalIds; // (include their SDS's)

        public DbSearchDailySummaryByCampaignExtracter(int searchAccountId, DateRange dateRange, bool includeNullSearchCampaignExternalIds = true)
        {
            this.searchAccountId = searchAccountId;
            this.dateRange = dateRange;
            this.includeNullSearchCampaignExternalIds = includeNullSearchCampaignExternalIds;
        }

        protected override void Extract()
        {
            Logger.Info("Extracting SearchDailySummaries from Db for SearchAccount {0} from {1:d} to {2:d}",
                this.searchAccountId, this.dateRange.FromDate, this.dateRange.ToDate);
            var items = EnumerateRows();
            Add(items);
            End();
        }

        private IEnumerable<SearchCampaign> EnumerateRows()
        {
            var searchCampaigns = new List<SearchCampaign>();
            using (var db = new ClientPortalContext())
            {
                var sds = db.SearchDailySummaries.Where(x => x.SearchCampaign.SearchAccountId == searchAccountId &&
                                                             x.Date >= dateRange.FromDate && x.Date <= dateRange.ToDate);
                if (!includeNullSearchCampaignExternalIds)
                    sds = sds.Where(x => x.SearchCampaign.ExternalId.HasValue);

                var sdsGroups = sds.GroupBy(x => x.SearchCampaign);
                foreach (var sdsGroup in sdsGroups)
                {
                    var searchCamp = new SearchCampaign
                    {
                        ExternalId = sdsGroup.Key.ExternalId
                    };
                    searchCamp.SearchDailySummaries = sdsGroup.ToList();
                    searchCampaigns.Add(searchCamp);
                }
            }
            return searchCampaigns;
        }
    }
}
