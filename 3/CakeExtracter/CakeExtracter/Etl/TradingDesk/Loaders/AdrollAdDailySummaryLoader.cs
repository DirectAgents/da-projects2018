using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.Extracters;
using ClientPortal.Data.Entities.TD;
using ClientPortal.Data.Entities.TD.AdRoll;

namespace CakeExtracter.Etl.TradingDesk.Loaders
{
    public class AdrollAdDailySummaryLoader : Loader<AdrollRow>
    {
        private readonly int adrollProfileId;
        private Dictionary<string, int> adIdLookupByName = new Dictionary<string, int>();

        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public Dictionary<int, string> AdsAffected = new Dictionary<int, string>();

        public AdrollAdDailySummaryLoader(int adrollProfileId)
        {
            this.adrollProfileId = adrollProfileId;
        }

        protected override int Load(List<AdrollRow> items)
        {
            Logger.Info("Loading {0} AdrollAdDailySummaries..", items.Count);
            AddDependentAds(items);
            var count = UpsertAdDailySummaries(items);
            return count;
        }

        private int UpsertAdDailySummaries(List<AdrollRow> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var skippedCount = 0;
            var itemCount = 0;
            using (var db = new TDContext())
            {
                DateTime date;
                foreach (var item in items)
                {
                    int? adId = null;
                    if (adIdLookupByName.ContainsKey(item.AdName))
                    {
                        date = DateTime.Parse(item.Date);
                        adId = adIdLookupByName[item.AdName];
                        var source = new AdDailySummary
                        {
                            Date = date,
                            AdRollAdId = adIdLookupByName[item.AdName],
                            Impressions = int.Parse(item.Impressions, NumberStyles.Number),
                            Clicks = int.Parse(item.Clicks, NumberStyles.Number),
                            Conversions = int.Parse(item.TotalConversions, NumberStyles.Number),
                            Spend = decimal.Parse(item.Spend, NumberStyles.Currency)
                        };
                        var target = db.Set<AdDailySummary>().Find(date, adId);
                        if (target == null)
                        {
                            db.AdDailySummaries.Add(source);
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
                                skippedCount++;
                            }
                            // Note: In the case that the csv has two identical rows and it's a new entry, we now avoid problems on the second row,
                            //       because Find will return the same "target". We don't want to change state from Added to Modified in that case.
                        }
                        itemCount++;

                        if (!MinDate.HasValue || date < MinDate.Value)
                            MinDate = date;
                        if (!MaxDate.HasValue || date > MaxDate.Value)
                            MaxDate = date;
                        if (!AdsAffected.ContainsKey(source.AdRollAdId))
                            AdsAffected[source.AdRollAdId] = item.AdName;
                    }
                    else
                        Logger.Warn("adIdLookupByName didn't contain key: {0}", item.AdName);
                }
                Logger.Info("Saving {0} AdDailySummaries ({1} updates, {2} additions, {3} duplicates)", itemCount, updatedCount, addedCount, skippedCount);
                db.SaveChanges();
            }
            return itemCount;
        }

        //Note: AdRoll's csv reports don't include the ad's Eid so it is difficult to match on ads in the db
        //      So if the AdRollAds are created here, we would need a way to fill in the Eids later (i.e. when using the API)

        private void AddDependentAds(List<AdrollRow> items)
        {
            int width, height;
            DateTime createDate, createDatePlusOne;
            using (var db = new TDContext())
            {
                // Find the unique AdNames by grouping
                var itemGroups = items.GroupBy(i => new { i.AdName, i.Size, i.Type, i.CreateDate });
                foreach (var group in itemGroups)
                {
                    var key = group.Key;
                    width = 0;
                    height = 0;
                    if (key.Size != null && key.Size.Contains('x'))
                    {
                        var dimensions = key.Size.Split('x');
                        Int32.TryParse(dimensions[0], out width);
                        Int32.TryParse(dimensions[1], out height);
                    }
                    createDate = new DateTime(2000, 1, 1);
                    DateTime.TryParse(key.CreateDate, out createDate);
                    createDatePlusOne = createDate.AddDays(1);

                    // See if the Ad exists in the db
                    var ads = db.AdRollAds.Where(a => a.Name == key.AdName && a.Width == width && a.Height == height && a.Type == key.Type
                                                        && a.CreatedDate >= createDate && a.CreatedDate < createDatePlusOne);
                    if (ads.Count() == 0)
                    {   // Create new AdRollAd
                        var ad = new AdRollAd
                        {
                            AdRollProfileId = adrollProfileId,
                            Name = key.AdName,
                            Type = key.Type,
                            Width = width,
                            Height = height,
                            CreatedDate = createDate
                        };
                        db.AdRollAds.Add(ad);
                        db.SaveChanges();
                        Logger.Info("Saving new AdRollAd: {0} ({1})", ad.Name, ad.Id);
                        adIdLookupByName[key.AdName] = ad.Id;
                    }
                    else
                    {   // Put existing Ad id in the lookup; (the first one if there's more than one)
                        adIdLookupByName[key.AdName] = ads.First().Id;
                    }
                }
            }
        }

    }
}
