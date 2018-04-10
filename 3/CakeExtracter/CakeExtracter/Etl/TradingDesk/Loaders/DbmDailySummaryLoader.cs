using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.Extracters;
using ClientPortal.Data.Entities.TD;
using ClientPortal.Data.Entities.TD.DBM;

namespace CakeExtracter.Etl.TradingDesk.Loaders
{
    public class DbmDailySummaryLoader : Loader<DbmRowBase>
    {

        protected override int Load(List<DbmRowBase> items)
        {
            Logger.Info("Loading {0} DailySummaries..", items.Count);
            AddUpdateDependentInsertionOrders(items);
            var count = UpsertDailySummaries(items);
            return count;
        }

        private int UpsertDailySummaries(List<DbmRowBase> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var itemCount = 0;
            using (var db = new TDContext())
            {
                foreach(var item in items)
                {
                    //DateTime date = DateTime.Parse(item.Date);
                    var date = item.Date;
                    var source = new DBMDailySummary
                    {
                        Date = date,
                        InsertionOrderID = item.InsertionOrderID,
                        Impressions = int.Parse(item.Impressions),
                        Clicks = int.Parse(item.Clicks),
                        Conversions = (int)decimal.Parse(item.TotalConversions),
                        Revenue = decimal.Parse(item.Revenue)
                    };
                    var target = db.Set<DBMDailySummary>().Find(date, item.InsertionOrderID);
                    if (target == null)
                    {
                        db.DBMDailySummaries.Add(source);
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
                Logger.Info("Saving {0} DailySummaries ({1} updates, {2} additions)", itemCount, updatedCount, addedCount);
                db.SaveChanges();
            }
            return itemCount;
        }

        public static void AddUpdateDependentInsertionOrders(List<DbmRowBase> items)
        {
            var ioTuples = items.Select(i => Tuple.Create(i.InsertionOrderID, i.InsertionOrder)).Distinct();
            AddUpdateInsertionOrders(ioTuples);
        }
        public static void AddUpdateInsertionOrders(IEnumerable<Tuple<int, string>> ioTuples)
        {
            using (var db = new TDContext())
            {
                foreach (var ioTuple in ioTuples)
                {
                    int insertionOrderID = ioTuple.Item1;
                    string insertionOrderName = ioTuple.Item2;
                    InsertionOrder existing = db.InsertionOrders.Find(insertionOrderID);
                    if (existing == null)
                    {
                        var io = new InsertionOrder
                        {
                            InsertionOrderID = insertionOrderID,
                            InsertionOrderName = insertionOrderName
                        };
                        db.InsertionOrders.Add(io);
                        Logger.Info("Saving new InsertionOrder: {0} ({1})", io.InsertionOrderName, io.InsertionOrderID);
                        db.SaveChanges();
                    }
                    else if (existing.InsertionOrderName != insertionOrderName)
                    {
                        existing.InsertionOrderName = insertionOrderName;
                        Logger.Info("Saving updated InsertionOrder: {0} ({1})", insertionOrderName, existing.InsertionOrderID);
                        db.SaveChanges();
                    }
                }
            }
        }

    }
}
