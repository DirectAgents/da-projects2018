using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.Extracters;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.DBM;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class DBMCreativeDailySummaryLoader : Loader<DbmRowBase>
    {
        public DateTime? EarliestDate { get; set; }

        protected override int Load(List<DbmRowBase> items)
        {
            Logger.Info("Loading {0} CreativeDailySummaries..", items.Count);
            AddUpdateDependentInsertionOrders(items);
            AddUpdateDependentCreatives(items);
            var count = UpsertCreativeDailySummaries(items);
            return count;
        }

        private int UpsertCreativeDailySummaries(List<DbmRowBase> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var itemCount = 0;
            using (var db = new ClientPortalProgContext())
            {
                foreach (var item in items)
                {
                    //DateTime date = DateTime.Parse(item.Date);
                    var date = item.Date;
                    if (EarliestDate == null || date < EarliestDate.Value)
                        EarliestDate = date;

                    int creativeID = int.Parse(((DbmRowWithCreative)item).CreativeID);
                    var source = new CreativeDailySummary
                    {
                        Date = date,
                        InsertionOrderID = item.InsertionOrderID,
                        CreativeID = creativeID,
                        Impressions = int.Parse(item.Impressions),
                        Clicks = int.Parse(item.Clicks),
                        PostClickConv = (int)decimal.Parse(item.PostClickConversions),
                        PostViewConv = (int)decimal.Parse(item.PostViewConversions),
                        Revenue = decimal.Parse(item.Revenue)
                    };
                    var target = db.Set<CreativeDailySummary>().Find(date, item.InsertionOrderID, creativeID);
                    if (target == null)
                    {
                        db.DBMCreativeDailySummaries.Add(source);
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

        public static void AddUpdateDependentInsertionOrders(List<DbmRowBase> items)
        {
            var ioTuples = items.Select(i => Tuple.Create(i.InsertionOrderID, i.InsertionOrder)).Distinct();
            AddUpdateInsertionOrders(ioTuples);
        }
        public static void AddUpdateInsertionOrders(IEnumerable<Tuple<int, string>> ioTuples)
        {
            using (var db = new ClientPortalProgContext())
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
                            ID = insertionOrderID,
                            Name = insertionOrderName
                        };
                        db.InsertionOrders.Add(io);
                        Logger.Info("Saving new InsertionOrder: {0} ({1})", io.Name, io.ID);
                        db.SaveChanges();
                    }
                    else if (existing.Name != insertionOrderName)
                    {
                        existing.Name = insertionOrderName;
                        Logger.Info("Saving updated InsertionOrder: {0} ({1})", insertionOrderName, existing.ID);
                        db.SaveChanges();
                    }
                }
            }
        }

        private static void AddUpdateDependentCreatives(List<DbmRowBase> items)
        {
            var tuples = items.Select(i => Tuple.Create(((DbmRowWithCreative)i).CreativeID, ((DbmRowWithCreative)i).Creative)).Distinct();

            using (var db = new ClientPortalProgContext())
            {
                foreach (var tuple in tuples)
                {
                    int creativeID;
                    string creativeName = tuple.Item2;

                    if (int.TryParse(tuple.Item1, out creativeID))
                    {
                        Creative existing = db.Creatives.Find(creativeID);
                        if (existing == null)
                        {
                            var creative = new Creative
                            {
                                ID = creativeID,
                                Name = creativeName
                            };
                            db.Creatives.Add(creative);
                            Logger.Info("Saving new Creative: {0} ({1})", creative.Name, creative.ID);
                            db.SaveChanges();
                        }
                        else if (existing.Name != creativeName)
                        {
                            existing.Name = creativeName;
                            Logger.Info("Saving updated Creative: {0} ({1})", creativeName, existing.ID);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
