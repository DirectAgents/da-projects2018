using System;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Common;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Commands.Test
{
    [Export(typeof(ConsoleCommand))]
    public class TestFacebookCommand : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public TestFacebookCommand()
        {
            IsCommand("testFacebook", "test Facebook");
        }

        public override int Execute(string[] remainingArguments)
        {
            //FillStats();
            return 0;
        }

        public void FillStats()
        {
            //int[] acctIds = new int[] { 1294, 1295, 1296 }; // Priv
            int[] acctIds = new int[] { 1324 }; // britbox 1324, 1325, 1326, 1355

            var yesterday = DateTime.Today.AddDays(-1);
            var dateRange = new DateRange(new DateTime(2017, 10, 1), yesterday);

            foreach (var acctId in acctIds)
            {
                //FillStratStatsFor(acctId, dateRange);
                FillAdSetStatsFor(acctId, dateRange);
            }
        }
        public void FillStratStatsFor(int acctId, DateRange dateRange)
        {
            using (var db = new ClientPortalProgContext())
            {
                var stratIds = db.Strategies.Where(x => x.AccountId == acctId).Select(x => x.Id).ToArray();

                //todo? look at stratSums where x.Strategy.AccountId == acctId instead of this...
                var ssQuery = db.StrategySummaries.Where(x => stratIds.Contains(x.StrategyId));

                foreach (var date in dateRange.Dates)
                {
                    var ssForDate = ssQuery.Where(x => x.Date == date).Select(x => x.StrategyId).Distinct().ToArray();
                    var missingStratIds = stratIds.Where(x => !ssForDate.Contains(x)).ToArray();
                    foreach (var stratId in missingStratIds)
                    {
                        var ss = new StrategySummary
                        {
                            StrategyId = stratId,
                            Date = date
                        };
                        db.StrategySummaries.Add(ss);
                    }
                    db.SaveChanges();
                    Logger.Info("({0}) {1} {2} added", acctId, date.ToShortDateString(), missingStratIds.Length);
                }
            }
        }

        public void FillAdSetStatsFor(int acctId, DateRange dateRange)
        {
            using (var db = new ClientPortalProgContext())
            {
                var adsetIds = db.AdSets.Where(x => x.AccountId == acctId).Select(x => x.Id).ToArray();

                var asQuery = db.AdSetSummaries.Where(x => adsetIds.Contains(x.AdSetId));

                foreach (var date in dateRange.Dates)
                {
                    var asForDate = asQuery.Where(x => x.Date == date).Select(x => x.AdSetId).Distinct().ToArray();
                    var missingAdSetIds = adsetIds.Where(x => !asForDate.Contains(x)).ToArray();
                    foreach (var adsetId in missingAdSetIds)
                    {
                        var asSum = new AdSetSummary
                        {
                            AdSetId = adsetId,
                            Date = date
                        };
                        db.AdSetSummaries.Add(asSum);
                    }
                    db.SaveChanges();
                    Logger.Info("({0}) {1} {2} added", acctId, date.ToShortDateString(), missingAdSetIds.Length);
                }
            }
        }

    }
}
