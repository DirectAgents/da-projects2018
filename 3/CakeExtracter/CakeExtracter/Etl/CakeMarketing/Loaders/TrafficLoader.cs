using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;

namespace CakeExtracter.Etl.CakeMarketing.Loaders
{
    public class TrafficLoader : Loader<DateRangeTraffic>
    {
        protected override int Load(List<DateRangeTraffic> items)
        {
            // delete all rates to start fresh and not need upsert
            using (var db = new CakeExtracter.Data.CakeTraffic.CakeTrafficContext())
            {
                db.Rates.ToList().ForEach(c => db.Rates.Remove(c));
                Logger.Info("Deleting all rates..");
                db.SaveChanges();
            }

            // delete all rates to start fresh and not need upsert
            using (var db = new CakeExtracter.Data.CakeTraffic.CakeTrafficContext())
            {
                db.Traffics.ToList().ForEach(c => db.Traffics.Remove(c));
                Logger.Info("Deleting all traffic..");
                db.SaveChanges();
            }

            Logger.Info("Upserting dependencies..");

            Parallel.Invoke(
                () => Logger.Info("Upserted {0} advertisers", UpsertAdvertisers(items)),
                () => Logger.Info("Upserted {0} affiliates", UpsertAffiliates(items)),
                () => Logger.Info("Upserted {0} offers", UpsertOffers(items))
            );

            var dateRangeTraffic = items.Single();
            var traffic = dateRangeTraffic.Traffic;

            Logger.Info("Adding all traffic..");
            foreach (var set in traffic.InBatches(250))
            {
                using (var db = new CakeExtracter.Data.CakeTraffic.CakeTrafficContext())
                {
                    foreach (var item in set)
                    {

                        var advertisers = db.Advertisers.ToDictionary(c => c.AdvertiserId);
                        var offers = db.Offers.ToDictionary(c => c.OfferId);
                        var affiliates = db.Affiliates.ToDictionary(c => c.AffiliateId);

                        var newTraffic = new CakeExtracter.Data.CakeTraffic.CakeTraffic
                        {
                            Advertiser = advertisers[item.AdvertiserId],
                            Offer = offers[item.OfferId],
                            Affiliate = affiliates[item.AffiliateId],
                            Conversions = item.Conversions,
                            Revenue = item.Revenue,
                            Cost = item.Cost
                        };

                        foreach (var advRate in item.AdvertiserRates)
                        {
                            newTraffic.AdvertiserRates.Add(new CakeExtracter.Data.CakeTraffic.CakeTrafficRate { Price = advRate.Price, Conversions = advRate.Conversions });
                        }

                        foreach (var affRate in item.AffiliateRates)
                        {
                            newTraffic.AffiliateRates.Add(new CakeExtracter.Data.CakeTraffic.CakeTrafficRate { Price = affRate.Price, Conversions = affRate.Conversions });
                        }

                        db.Traffics.Add(newTraffic);
                    }

                    Logger.Info("saving {0} traffics..", set.Count);

                    db.SaveChanges();
                }
            }

            return 1; // single item loaded
        }

        private int UpsertAdvertisers(List<DateRangeTraffic> items)
        {
            int count = 0;
            using (var db = new CakeExtracter.Data.CakeTraffic.CakeTrafficContext())
            {
                var dateRangeTraffic = items.Single();
                var traffic = dateRangeTraffic.Traffic;
                var advertisers = from c in traffic
                                  select new CakeExtracter.Data.CakeTraffic.CakeTrafficAdvertiser
                                  {
                                      AdvertiserId = c.AdvertiserId,
                                      AdvertiserName = c.AdvertiserName
                                  };
                foreach (var item in advertisers.DistinctBy(c => c.AdvertiserId))
                {
                    //Logger.Info("upserting advertiser id={0} name={1}", item.AdvertiserId, item.AdvertiserName);
                    db.Advertisers.AddOrUpdate(item);
                    count++;
                }
                Logger.Info("Saving Advertisers..");
                db.SaveChanges();
            }
            return count;
        }

        private int UpsertOffers(List<DateRangeTraffic> items)
        {
            int count = 0;
            using (var db = new CakeExtracter.Data.CakeTraffic.CakeTrafficContext())
            {
                var dateRangeTraffic = items.Single();
                var traffic = dateRangeTraffic.Traffic;
                var offers = from c in traffic
                             select new CakeExtracter.Data.CakeTraffic.CakeTrafficOffer
                             {
                                 OfferId = c.OfferId,
                                 OfferName = c.OfferName
                             };
                foreach (var item in offers.DistinctBy(c => c.OfferId))
                {
                    //Logger.Info("upserting offer id={0} name={1}", item.OfferId, item.OfferName);
                    db.Offers.AddOrUpdate(item);
                    count++;
                }
                Logger.Info("Saving Offers..");
                db.SaveChanges();
            }
            return count;
        }

        private int UpsertAffiliates(List<DateRangeTraffic> items)
        {
            int count = 0;
            using (var db = new CakeExtracter.Data.CakeTraffic.CakeTrafficContext())
            {
                var dateRangeTraffic = items.Single();
                var traffic = dateRangeTraffic.Traffic;
                var offers = from c in traffic
                             select new CakeExtracter.Data.CakeTraffic.CakeTrafficAffiliate
                             {
                                 AffiliateId = c.AffiliateId,
                                 AffiliateName = c.AffiliateName
                             };
                foreach (var item in offers.DistinctBy(c => c.AffiliateId))
                {
                    //Logger.Info("upserting affiliate id={0} name={1}", item.AffiliateId, item.AffiliateName);
                    db.Affiliates.AddOrUpdate(item);
                    count++;
                }
                Logger.Info("Saving Affiliates..");
                db.SaveChanges();
            }
            return count;
        }
    }
}