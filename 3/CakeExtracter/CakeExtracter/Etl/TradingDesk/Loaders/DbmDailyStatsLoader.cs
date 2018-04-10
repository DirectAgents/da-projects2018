using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.Extracters;
using ClientPortal.Data.Entities.TD;
using ClientPortal.Data.Entities.TD.DBM;

namespace CakeExtracter.Etl.TradingDesk.Loaders
{
    public class DbmDailyStatsLoader : Loader<DailyLocationStatRow>
    {

        protected override int Load(List<DailyLocationStatRow> items)
        {
            Logger.Info("Loading {0} DailyLocationStats..", items.Count);
            AddUpdateDependentInsertionOrders(items);
            AddUpdateDependentCities(items);
            AddUpdateDependentRegions(items);
            AddUpdateDependentDMAs(items);
            var count = UpsertDailyLocationStats(items);
            return count;
        }

        private int UpsertDailyLocationStats(List<DailyLocationStatRow> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;
            var itemCount = 0;
            using (var db = new TDContext())
            {
                foreach (var item in items)
                {
                    var source = new DailyLocationStat
                    {
                        // properties needed to uniquely identify the record:
                        Date = item.Date,
                        InsertionOrderID = item.InsertionOrderID,
                        CityID = item.GetCityID()
                        //...
                    };
                }
            }

            return 0; //temp
        }

        public static void AddUpdateDependentInsertionOrders(List<DailyLocationStatRow> items)
        {
            var ioTuples = items.Select(i => Tuple.Create(i.InsertionOrderID, i.InsertionOrder)).Distinct();
            DbmDailySummaryLoader.AddUpdateInsertionOrders(ioTuples);
        }

        public static void AddUpdateDependentCities(List<DailyLocationStatRow> items)
        {
            var cityTuples = items.Select(i => Tuple.Create(i.CityID, i.City)).Distinct();
            using (var db = new TDContext())
            {
                foreach (var tuple in cityTuples)
                {
                    int cityID;
                    string cityName = tuple.Item2;
                    if (!int.TryParse(tuple.Item1, out cityID))
                        cityID = -1; //TODO: check name is "Unknown"...

                    City existing = db.Cities.Find(cityID);
                    if (existing == null)
                    {
                        var city = new City { CityID = cityID, Name = cityName };
                        db.Cities.Add(city);
                        Logger.Info("Saving new City: {0} ({1})", city.Name, city.CityID);
                        db.SaveChanges();
                    }
                    else if (existing.Name != cityName)
                    {
                        existing.Name = cityName;
                        Logger.Info("Saving updated City: {0} ({1})", cityName, existing.CityID);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void AddUpdateDependentRegions(List<DailyLocationStatRow> items)
        {
            var regionTuples = items.Select(i => Tuple.Create(i.RegionID, i.Region)).Distinct();
            using (var db = new TDContext())
            {
                foreach (var tuple in regionTuples)
                {
                    int regionID;
                    string regionName = tuple.Item2;
                    if (!int.TryParse(tuple.Item1, out regionID))
                        regionID = -1; //TODO: check name is "Unknown"...

                    Region existing = db.Regions.Find(regionID);
                    if (existing == null)
                    {
                        var region = new Region { RegionID = regionID, Name = regionName };
                        db.Regions.Add(region);
                        Logger.Info("Saving new Region: {0} ({1})", region.Name, region.RegionID);
                        db.SaveChanges();
                    }
                    else if (existing.Name != regionName)
                    {
                        existing.Name = regionName;
                        Logger.Info("Saving updated Region: {0} ({1})", regionName, existing.RegionID);
                        db.SaveChanges();
                    }
                }
            }
        }

        public static void AddUpdateDependentDMAs(List<DailyLocationStatRow> items)
        {
            var dmaTuples = items.Select(i => Tuple.Create(i.DMACode, i.DMAName)).Distinct();
            using (var db = new TDContext())
            {
                foreach (var tuple in dmaTuples)
                {
                    int dmaCode;
                    string dmaName = tuple.Item2;
                    if (!int.TryParse(tuple.Item1, out dmaCode))
                        dmaCode = -1; //TODO: check name is "Unknown"...

                    DMA existing = db.DMAs.Find(dmaCode);
                    if (existing == null)
                    {
                        var dma = new DMA { DMACode = dmaCode, Name = dmaName };
                        db.DMAs.Add(dma);
                        Logger.Info("Saving new DMA: {0} ({1})", dma.Name, dma.DMACode);
                        db.SaveChanges();
                    }
                    else if (existing.Name != dmaName)
                    {
                        existing.Name = dmaName;
                        Logger.Info("Saving updated DMA: {0} ({1})", dmaName, existing.DMACode);
                        db.SaveChanges();
                    }
                }
            }
        }

    }
}
