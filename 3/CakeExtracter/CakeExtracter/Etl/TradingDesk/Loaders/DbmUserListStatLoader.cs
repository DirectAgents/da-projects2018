using System.Collections.Generic;
using System.Data.Entity;
using CakeExtracter.Etl.TradingDesk.Extracters;
using ClientPortal.Data.Entities.TD;
using ClientPortal.Data.Entities.TD.DBM;

namespace CakeExtracter.Etl.TradingDesk.Loaders
{
    public class DbmUserListStatLoader : Loader<UserListStatRow>
    {
        private readonly int userListRunID;

        public DbmUserListStatLoader(int userListRunID)
        {
            this.userListRunID = userListRunID;
        }

        protected override int Load(List<UserListStatRow> items)
        {
            Logger.Info("Loading {0} UserListStats..", items.Count);
            int count = UpsertUserListStats(items);
            return count;
        }

        private int UpsertUserListStats(List<UserListStatRow> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;
            var itemCount = 0;
            using (var db = new TDContext())
            {
                foreach (var item in items)
                {
                    var source = new UserListStat
                    {
                        UserListRunID = this.userListRunID,
                        UserListID = item.UserListID,
                        UserListName = item.UserListName,
                        //Cost =
                        EligibleCookies = item.EligibleCookies,
                        MatchRatio = item.MatchRatio,
                        PotentialImpressions = item.PotentialImpressions,
                        //Uniques =
                    };

                    if (item.Cost != "Unknown")
                    {
                        float cost;
                        if (float.TryParse(item.Cost, out cost))
                            source.Cost = cost;
                        else
                            Logger.Warn("Could not parse Cost: \"{0}\"", item.Cost);
                    }

                    if (item.Uniques == "<1000")
                        source.Uniques = 999;
                    else
                    {
                        int uniques;
                        if (int.TryParse(item.Uniques, out uniques))
                            source.Uniques = uniques;
                        else
                            Logger.Warn("Could not parse Uniques: \"{0}\"", item.Uniques);
                    }

                    var target = db.Set<UserListStat>().Find(source.UserListRunID, source.UserListID);
                    if (target == null)
                    {
                        db.UserListStats.Add(source);
                        addedCount++;
                    }
                    else
                    {
                        var entry = db.Entry(target);
                        if (entry.State == EntityState.Unchanged)
                        {
                            entry.State = EntityState.Detached;
                            AutoMapper.Mapper.Map(source, target);
                            entry.State = EntityState.Modified;
                            updatedCount++;
                        }
                        else
                        {
                            skippedCount++; // if duplicate rows extracted
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} UserListStats ({1} updates, {2} additions, {3} duplicates)", itemCount, updatedCount, addedCount, skippedCount);
                db.SaveChanges();
            }
            return itemCount;
        }
    }
}
