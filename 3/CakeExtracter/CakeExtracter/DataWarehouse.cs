using System;
using System.Collections.Generic;
using System.Linq;
using CakeExtracter.Common;
using ClientPortal.Data.Contexts;
using ClientPortal.Web.Models.Cake;

namespace CakeExtracter
{
    class DataWarehouse
    {
        public void UpdateClicks(List<click> clicks)
        {
            Console.WriteLine("Processing {0} clicks..", clicks.Count);

            ProcessAdvertisers(clicks.Select(c => c.advertiser));
            ProcessOffers(clicks.Select(c => c.offer));
            ProcessAffiliates(clicks.Select(c => c.affiliate));
            ProcessRegions(clicks.Select(c => c.region));
            ProcessCountries(clicks.Select(c => c.country));
            ProcessClicks(clicks);
        }

        public void UpdateConversions(List<conversion> conversions)
        {
            var remaining = ProcessConversions(conversions);
            int remainingCount = remaining.Count;
            var cake = new CakeMarketing(new CakeMarketingCache {Enabled = false});

            if (remaining.Count > 0)
            {
                var dateGroups = remaining.GroupBy(c => (c.click_date ?? DateTime.MinValue).Date).ToArray();
                int dateGroupsCount = dateGroups.Count();

                Console.WriteLine("extracting clicks for {0} dates in order to process remaining {1} conversions..", dateGroupsCount, remainingCount);

                foreach (var dateGroup in dateGroups.OrderBy(c => c.Key))
                {
                    Console.WriteLine("extracting clicks for {0} in order to process {1} conversions", dateGroup.Key, dateGroup.Count());

                    var clicksToProcess = cake.ClicksByConversion(dateGroup).SelectMany(c => c.clicks);

                    UpdateClicks(clicksToProcess.ToList());

                    var stillToProcess = ProcessConversions(dateGroup);
                    if (stillToProcess.Count > 0)
                        throw new Exception(string.Format("Failed to process {0} convresions", remainingCount));
                }
            }
        }

        #region Process Methods
        private void ProcessAdvertisers(IEnumerable<advertiser> advertisers)
        {
            var dimAdvertisers = AdvertisersToDimAdvertisers(advertisers);
            LoadDimension(dimAdvertisers, c => c.AdvertiserKey);
        }

        private void ProcessOffers(IEnumerable<offer> offers)
        {
            var dimOffers = OffersToDimOffers(offers);
            LoadDimension(dimOffers, c => c.OfferKey);
        }

        private void ProcessAffiliates(IEnumerable<affiliate> affiliates)
        {
            var dimAffiliates = AffiliatesToDimAffiliates(affiliates);
            LoadDimension(dimAffiliates, c => c.AffiliateKey);
        }

        private void ProcessRegions(IEnumerable<region> regions)
        {
            var dimRegions = RegionsToDimRegions(regions);
            LoadDimRegions(dimRegions);
        }

        private void ProcessCountries(IEnumerable<country> countries)
        {
            var dimCountries = CountriesToDimCountries(countries);
            LoadDimCountries(dimCountries);
        }

        private void ProcessClicks(List<click> clicks)
        {
            var dimCountries = DimCountries();
            var dimRegions = DimRegions();

            int count = 0;
            int clicksCount = clicks.Count;
            const int step = 1000;
            foreach (var clickSet in clicks.InBatches(step))
            {
                Console.WriteLine("Processing {0}/{1}..", count, clicksCount);

                var factClicks = new List<FactClick>();

                using (var db = new ClientPortalDWContext())
                {
                    foreach (var click in clickSet)
                    {
                        if (db.FactClicks.FirstOrDefault(c => c.ClickKey == click.click_id) == null)
                        {
                            factClicks.Add(new FactClick
                            {
                                ClickKey = click.click_id,
                                CountryKey = dimCountries[click.country.country_code].CountryKey,
                                RegionKey = dimRegions[click.region.region_code].RegionKey,
                                DateKey = click.click_date.Date,
                                AdvertiserKey = click.advertiser.advertiser_id,
                                AffiliateKey = click.affiliate.affiliate_id,
                                OfferKey = click.offer.offer_id
                            });
                        }
                    }

                    Console.WriteLine("Saving {0} click facts..", factClicks.Count);
                    factClicks.ForEach(c => db.FactClicks.Add(c));
                    db.SaveChanges();

                    count += step;
                }
            }
        }

        private List<conversion> ProcessConversions(IEnumerable<conversion> conversions)
        {
            var factConversionsToLoad = new List<FactConversion>();

            var conversionsNeedingClick = new List<conversion>();

            using (var db = new ClientPortalDWContext())
            {
                foreach (var conversion in conversions)
                {
                    // if the conversion fact does not exist
                    if (db.FactConversions.FirstOrDefault(c => c.ConversionKey == conversion.conversion_id) == null)
                    {
                        int? clickKey = conversion.click_id;

                        if (clickKey != null)
                        {
                            // ..and if the click fact does exist
                            if (db.FactClicks.FirstOrDefault(c => c.ClickKey == conversion.click_id) != null)
                            {
                                factConversionsToLoad.Add(new FactConversion
                                {
                                    ConversionKey = conversion.conversion_id,
                                    ClickKey = clickKey.Value,
                                    DateKey = conversion.conversion_date.Date,
                                });
                            }
                            else
                            {
                                conversionsNeedingClick.Add(conversion);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Conversion id {0} has null click id.", conversion.conversion_id);
                        }
                    }
                }

                Console.WriteLine("Saving {0} conversion facts..", factConversionsToLoad.Count);
                factConversionsToLoad.ForEach(c => db.FactConversions.Add(c));
                db.SaveChanges();
            }

            return conversionsNeedingClick;
        }
        #endregion

        #region Transform Methods
        private IEnumerable<DimAdvertiser> AdvertisersToDimAdvertisers(IEnumerable<advertiser> advertisers)
        {
            return advertisers.Select(c => new DimAdvertiser
                {
                    AdvertiserKey = c.advertiser_id,
                    AdvertiserName = c.advertiser_name
                });
        }

        private IEnumerable<DimOffer> OffersToDimOffers(IEnumerable<offer> offers)
        {
            return offers.Select(c => new DimOffer
            {
                OfferKey = c.offer_id,
                OfferName = c.offer_name
            });
        }

        private IEnumerable<DimAffiliate> AffiliatesToDimAffiliates(IEnumerable<affiliate> affiliates)
        {
            return affiliates.Select(c => new DimAffiliate()
            {
                AffiliateKey = c.affiliate_id,
                AffiliateName = c.affiliate_name
            });
        }

        private IEnumerable<DimRegion> RegionsToDimRegions(IEnumerable<region> regions)
        {
            return regions.Select(c => new DimRegion
            {
                RegionCode = c.region_code
            });
        }

        private IEnumerable<DimCountry> CountriesToDimCountries(IEnumerable<country> countries)
        {
            return countries.Select(c => new DimCountry
            {
                CountryCode = c.country_code
            });
        }
        #endregion

        #region ToLoad Methods
        private void LoadDimension<T>(IEnumerable<T> toLoad, Func<T, int> key) where T : class
        {
            using (var db = new ClientPortalDWContext())
            {
                var existing = db.Set<T>().ToList();
                var add = toLoad.Except(existing, new IntKeyComparer<T>(key));
                foreach (var item in add)
                {
                    db.Set<T>().Add(item);
                }
                db.SaveChanges();
            }
        }

        private void LoadDimRegions(IEnumerable<DimRegion> dimRegions)
        {
            using (var db = new ClientPortalDWContext())
            {
                var existing = db.DimRegions.ToList();
                var add = dimRegions.Except(existing, new DimRegionEqualityComparer());
                foreach (var item in add)
                {
                    db.DimRegions.Add(item);
                }
                db.SaveChanges();
            }
        }

        private void LoadDimCountries(IEnumerable<DimCountry> dimCountries)
        {
            using (var db = new ClientPortalDWContext())
            {
                var existing = db.DimCountries.ToList();
                var add = dimCountries.Except(existing, new DimCountryEqualityComparer());
                foreach (var item in add)
                {
                    db.DimCountries.Add(item);
                }
                db.SaveChanges();
            }
        }
        #endregion

        #region Lookups
        Dictionary<string, DimCountry> DimCountries()
        {
            using (var db = new ClientPortalDWContext())
            {
                return db.DimCountries.ToDictionary(c => c.CountryCode);
            }
        }

        Dictionary<string, DimRegion> DimRegions()
        {
            using (var db = new ClientPortalDWContext())
            {
                return db.DimRegions.ToDictionary(c => c.RegionCode);
            }
        }
        #endregion

        #region EqualityComparers
        class IntKeyComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, int> _key;

            public IntKeyComparer(Func<T, int> key)
            {
                _key = key;
            }
            public bool Equals(T x, T y)
            {
                return _key(x) == _key(y);
            }

            public int GetHashCode(T obj)
            {
                return _key(obj).GetHashCode();
            }
        }
        class DimCountryEqualityComparer : IEqualityComparer<DimCountry>
        {
            public bool Equals(DimCountry x, DimCountry y)
            {
                return x.CountryCode == y.CountryCode;
            }

            public int GetHashCode(DimCountry obj)
            {
                return obj.CountryCode.GetHashCode();
            }
        }
        class DimRegionEqualityComparer : IEqualityComparer<DimRegion>
        {
            public bool Equals(DimRegion x, DimRegion y)
            {
                return x.RegionCode == y.RegionCode;
            }

            public int GetHashCode(DimRegion obj)
            {
                return obj.RegionCode.GetHashCode();
            }
        }
        #endregion
    }
}