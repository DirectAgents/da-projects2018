using System;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Common;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Commands.Test
{
    [Export(typeof(ConsoleCommand))]
    public class TestAdWordsCommand : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public TestAdWordsCommand()
        {
            IsCommand("testAdWords", "test AdWords");
        }

        public override int Execute(string[] remainingArguments)
        {
            //FillStats();
            return 0;
        }

        public void FillStats()
        {
            //int searchAccountId = 161; // Priv
            int searchAccountId = 182; // BritBox

            //var start = new DateTime(2017, 8, 6);
            var start = new DateTime(2017, 8, 7);
            var yesterday = DateTime.Today.AddDays(-1);
            var dateRange = new DateRange(start, yesterday);

            using (var db = new ClientPortalContext())
            {
                var campIds = db.SearchCampaigns.Where(x => x.SearchAccountId == searchAccountId).Select(x => x.SearchCampaignId).ToArray();

                var sdsQuery = db.SearchDailySummaries.Where(x => campIds.Contains(x.SearchCampaignId));

                foreach (var date in dateRange.Dates)
                {
                    var sdsForDate = sdsQuery.Where(x => x.Date == date).Select(x => x.SearchCampaignId).Distinct().ToArray();
                    var missingCampIds = campIds.Where(x => !sdsForDate.Contains(x)).ToArray();
                    foreach (var campId in missingCampIds)
                    {
                        var sds = new SearchDailySummary
                        {
                            SearchCampaignId = campId,
                            Date = date,
                            Network = "S",
                            Device = "M",
                            CurrencyId = 1
                        };
                        db.SearchDailySummaries.Add(sds);
                    }
                    db.SaveChanges();
                    Logger.Info("{0} {1} existed, {2} added", date.ToShortDateString(), sdsForDate.Length, missingCampIds.Length);
                }
            }
        }

    }
}
