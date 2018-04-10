using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.Extracters;
using ClientPortal.Data.Entities.TD;
using ClientPortal.Data.Entities.TD.DBM;

namespace CakeExtracter.Etl.TradingDesk.Loaders
{
    public class DbmCreativeDailySummaryLoader : Loader<DbmRowBase>
    {

        protected override int Load(List<DbmRowBase> items)
        {
            Logger.Info("Loading {0} CreativeDailySummaries..", items.Count);
            DbmDailySummaryLoader.AddUpdateDependentInsertionOrders(items);
            AddUpdateDependentCreatives(items);
            var count = UpsertCreativeDailySummaries(items);
            return count;
        }

        private int UpsertCreativeDailySummaries(List<DbmRowBase> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var itemCount = 0;
            using (var db = new TDContext())
            {
                foreach (var item in items)
                {
                    //DateTime date = DateTime.Parse(item.Date);
                    var date = item.Date;
                    int creativeID = int.Parse(((DbmRowWithCreative)item).CreativeID);
                    var source = new CreativeDailySummary
                    {
                        Date = date,
                        CreativeID = creativeID,
                        Impressions = int.Parse(item.Impressions),
                        Clicks = int.Parse(item.Clicks),
                        Conversions = (int)decimal.Parse(item.TotalConversions),
                        Revenue = decimal.Parse(item.Revenue)
                    };
                    var target = db.Set<CreativeDailySummary>().Find(date, creativeID);
                    if (target == null)
                    {
                        db.CreativeDailySummaries.Add(source);
                        addedCount++;
                    }
                    else
                    {
                        var entry = db.Entry(target);
                        entry.State = EntityState.Detached;
                        AutoMapper.Mapper.Map(source, target);
                        entry.State = EntityState.Modified;
                        updatedCount++;
                    }
                    itemCount++;
                }
                Logger.Info("Saving {0} CreativeDailySummaries ({1} updates, {2} additions)", itemCount, updatedCount, addedCount);
                db.SaveChanges();
            }
            return itemCount;
        }

        private void AddUpdateDependentCreatives(List<DbmRowBase> items)
        {
            using (var db = new TDContext())
            {
                var tuples = items.Select(i => Tuple.Create(((DbmRowWithCreative)i).CreativeID, ((DbmRowWithCreative)i).Creative, i.InsertionOrderID)).Distinct();
                foreach (var tuple in tuples)
                {
                    int creativeID;
                    string creativeName = tuple.Item2;
                    int insertionOrderID = tuple.Item3;

                    //FOR DEMO
                    if (creativeName.StartsWith("Betterment ") || creativeName.StartsWith("Betterment_"))
                        creativeName = creativeName.Substring(11);

                    if (int.TryParse(tuple.Item1, out creativeID))
                    {
                        Creative existing = db.Creatives.Find(creativeID);
                        if (existing == null)
                        {
                            var creative = new Creative
                            {
                                CreativeID = creativeID,
                                CreativeName = creativeName,
                                InsertionOrderID = insertionOrderID
                            };
                            db.Creatives.Add(creative);
                            Logger.Info("Saving new Creative: {0} ({1})", creative.CreativeName, creative.CreativeID);
                            db.SaveChanges();
                        }
                        else if (existing.CreativeName != creativeName)
                        {
                            existing.CreativeName = creativeName;
                            Logger.Info("Saving updated Creative: {0} ({1})", creativeName, existing.CreativeID);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

    }
}
