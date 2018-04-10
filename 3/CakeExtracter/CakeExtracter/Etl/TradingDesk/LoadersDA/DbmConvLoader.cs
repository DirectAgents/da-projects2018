using System.Collections.Generic;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.Loaders;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using DirectAgents.Domain.Entities.DBM;
using System;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class DbmConvLoader : Loader<DataTransferRow>
    {
        private DbmConvConverter convConverter;
        private Dictionary<int, int> accountIdLookupByExtId = new Dictionary<int, int>();
        private Dictionary<string, int> countryIdLookupByName = new Dictionary<string, int>();
        private Dictionary<string, int> cityIdLookupByName = new Dictionary<string, int>();
        private Dictionary<int,string> bucketLookupByName = new Dictionary<int,string>();

        public DbmConvLoader(DbmConvConverter convConverter)
        {
            this.convConverter = convConverter;
        }

        protected override int Load(List<DataTransferRow> items)
        {
            UpdateAccountLookup(items);
            UpdateBuckets(items);
            UpdateDependentCities(items);
            var convs = items.Select(i => CreateConv(i)).Where(i => i.AccountId > 0).ToList();
            var count = TDConvLoader.UpsertConvs(convs);
            return count;
        }

        private Conv CreateConv(DataTransferRow dtRow)
        {
            var conv = new Conv
            {
                AccountId = accountIdLookupByExtId[dtRow.insertion_order_id.Value],
                Time = RoundedTime( convConverter.EventTime(dtRow)),
                ConvType = (dtRow.event_sub_type == "postview") ? "v" : "c",
                ConvVal = 0,
                IP = dtRow.ip
            };
            if (dtRow.city_name != null)
                conv.CityId = cityIdLookupByName[dtRow.city_name];
            return conv;
        }

        private void UpdateAccountLookup(List<DataTransferRow> items)
        {
            var acctExtIds = items.Select(i => i.insertion_order_id.Value).Distinct();

            using (var db = new ClientPortalProgContext())
            {
                foreach (var acctExtId in acctExtIds)
                {
                    if (accountIdLookupByExtId.ContainsKey(acctExtId))
                        continue; // already encountered

                    var tdAccts = db.ExtAccounts.Where(a => a.ExternalId == acctExtId.ToString());
                    if (tdAccts.Count() == 1)
                    {
                        var tdAcct = tdAccts.First();
                        accountIdLookupByExtId[acctExtId] = tdAcct.Id;
                    }
                    else
                    {
                        var newAccount = new ExtAccount
                        {
                            ExternalId = acctExtId.ToString(),
                            PlatformId = Platform.GetId(db,Platform.Code_DBM),
                            Name = "Unknown"
                        };
                        db.ExtAccounts.Add(newAccount);
                        db.SaveChanges();
                        Logger.Info("Added new ExtAccount: InsertionOrder {0}", acctExtId);
                        accountIdLookupByExtId[acctExtId] = newAccount.Id;
                    }

                }
            }
        }

        //TODO: don't add cities that are already in database (e.g., "Chicago,Illinois" isn't distinct from just "Chicago")
        private void UpdateDependentCities(List<DataTransferRow> items)
        {
            var cities = items.Select(i => new Tuple<string,string,string,bool>(i.city_name,i.short_name,i.country_name,i.unique_city_name)).Distinct().Where(i => i.Item1 != null);
            var countryGroups = cities.GroupBy(c => c.Item3);

            using (var db = new ClientPortalProgContext())
            {
                foreach (var countryGroup in countryGroups)
                {
                    var countryName = countryGroup.Key;
                    var country = db.ConvCountries.FirstOrDefault(c => c.Name == countryName);

                    if (!countryIdLookupByName.ContainsKey(countryName))
                    {
                        if (country == null)
                        {
                            country = new ConvCountry
                            {
                                Name = countryName
                            };
                            db.ConvCountries.Add(country);
                            db.SaveChanges();
                            Logger.Info("Added new country {0} with id {1} to database.", countryName, country.Id);

                        }
                    }

                    countryIdLookupByName[countryName] = country.Id;
                    int countryId = country.Id;

                    foreach (var city in countryGroup)
                    {
                        if (cityIdLookupByName.ContainsKey(city.Item1))
                            continue;

                        var cpCity = db.ConvCities.FirstOrDefault(c => c.Name == city.Item1 && c.CountryId == countryId);

                        if (cpCity == null && city.Item4 == true)
                            cpCity = db.ConvCities.FirstOrDefault(c => c.Name == city.Item2 && c.CountryId == countryId);

                        if (cpCity == null)
                        {
                            cpCity = new ConvCity
                            {
                                Name = city.Item1,
                                CountryId = countryId
                            };

                            db.ConvCities.Add(cpCity);
                            db.SaveChanges();
                            Logger.Info("Added new city {0} in country {1} with id {2} to database.", city.Item1, countryName, cpCity.Id);
                        }

                        else if (city.Item4 == true && cpCity.Name != city.Item1)
                        {
                            var formerName = cpCity.Name;
                            cpCity.Name = city.Item1;
                            db.SaveChanges();
                            Logger.Info("Updated city name from {0} to {1} in database.", formerName, city.Item1);
                        }
                        cityIdLookupByName[city.Item1] = cpCity.Id;
                    }
                }
            }
        }

        private void UpdateBuckets(List<DataTransferRow> items)
        {
            var tuples = items.Select(i => new Tuple<int,string>(i.insertion_order_id.Value,i.advertiser_id)).Distinct();

            using (var db = new ClientPortalProgContext())
            {
                foreach (var insertOrderId in tuples)
                {

                    if (bucketLookupByName.ContainsKey(insertOrderId.Item1))
                        continue; //already encountered

                    var dbmInsertOrders = db.InsertionOrders.Where(i => i.ID == insertOrderId.Item1);
                    
                    if (dbmInsertOrders.Count() == 1)
                    {
                        var dbmInsertOrder = dbmInsertOrders.First();
                        bucketLookupByName[insertOrderId.Item1] = dbmInsertOrder.Bucket;
                    }
                    else
                    {
                        var newInsertOrder = new InsertionOrder
                        {
                            ID = insertOrderId.Item1,
                            Bucket = insertOrderId.Item2
                        };
                        Logger.Info("New Insertion Order {0} with bucket {1}.", insertOrderId.Item1.ToString(), insertOrderId.Item2);
                        db.InsertionOrders.Add(newInsertOrder);
                        db.SaveChanges();
                        bucketLookupByName[insertOrderId.Item1] = insertOrderId.Item2;
                    }
                }
            }
        }

        public static DateTime RoundedTime(DateTime dt)
        {
            var ticks = dt.Ticks;
            var rt = (ticks / 100000) * 100000;
            var extraTicks = rt - ticks;
            long roundedTicks;
            if (-15000 < extraTicks)
                roundedTicks = 0;
            else if (-45000 < extraTicks && extraTicks <= -15000)
                roundedTicks = 30000;
            else if (-84500 < extraTicks && extraTicks <= -45000)
                roundedTicks = 70000;
            else
                roundedTicks = 100000;
            var newDt = dt.AddTicks((long)extraTicks);
            newDt = newDt.AddTicks(roundedTicks);
            return newDt;
        }
    }
}
