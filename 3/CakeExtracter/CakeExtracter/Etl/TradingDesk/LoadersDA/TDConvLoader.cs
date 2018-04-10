using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using CakeExtracter.Etl.TradingDesk.Extracters;
using System.Globalization;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class TDConvLoader : Loader<ConvRow>
    {
        //public TDConvLoader() { }
        private readonly int accountId;
        private readonly string platCode;
        private Dictionary<string, int> countryIdLookupByName = new Dictionary<string, int>();
        private Dictionary<string, int> cityIdLookupByCountryCity = new Dictionary<string, int>();

        public TDConvLoader(int acctId, string platCode)
        {
            this.accountId = acctId;
            this.platCode = platCode;
        }

        //currently only loading DBM conversions--include Adroll later
        protected override int Load(List<ConvRow> items)
        {
            Logger.Info("Loading {0} DA-TD Convs..", items.Count);
            var convs = items.Select(c => CreateConv(c)).ToList();
            var count = UpsertConvs(convs);
            return count;
        }

        public Conv CreateConv(ConvRow convRow)
        {
            var conv = new Conv
            {
                AccountId = accountId,
                Time = convRow.ConvTime,
                //StrategyId = strategyIdLookupByCampName[convRow.Campaign], // could be null
                //TDadId = adIdLookupByName[convRow.Ad], // could be null
                ConvVal = decimal.Parse(convRow.ConvVal, NumberStyles.Currency),
                //CityId = cityId,
                ExtData = convRow.ext_data_order_id
            };

            if (platCode == Platform.Code_DBM)
            {
                conv.ConvType = (convRow.PostClickConvs == 0) ? "v" : "c";
            }


            return conv;
        }

        private void AddUpdateDependentCities(List<ConvRow> items)
        {
            using (var db = new ClientPortalProgContext())
            {
                var tuples = items.Select(c => new Tuple<string, string>(c.Country, c.City)).Distinct();
                var countryGroups = tuples.GroupBy(t => t.Item1);
                foreach (var countryGroup in countryGroups) // first, loop through each country
                {
                    var countryName = countryGroup.Key;
                    if (!countryIdLookupByName.ContainsKey(countryName))
                    { // look for this country in the db...
                        var country = db.ConvCountries.FirstOrDefault(c => c.Name == countryName);
                        // (assuming just one ConvCountries with the specified Name)
                        if (country == null)
                        {
                            country = new ConvCountry
                            {
                                Name = countryName
                            };
                            db.ConvCountries.Add(country);
                            db.SaveChanges();
                            Logger.Info("Saved new country: {0} ({1})", country.Name, country.Id);
                        }
                        countryIdLookupByName[countryName] = country.Id;
                    }
                    int countryId = countryIdLookupByName[countryName];
                    foreach (var tuple in countryGroup) // within each country, loop through each city
                    {
                        var cityName = tuple.Item2;
                        var countryCity = countryName + "_" + cityName;
                        if (cityIdLookupByCountryCity.ContainsKey(countryCity))
                            continue; // already encountered

                        var city = db.ConvCities.Where(c => c.CountryId == countryId && c.Name == cityName).FirstOrDefault();
                        // (assuming just one)
                        if (city == null)
                        {
                            city = new ConvCity
                            {
                                CountryId = countryId,
                                Name = cityName
                            };
                            db.ConvCities.Add(city);
                            db.SaveChanges();
                            Logger.Info("Saved new city: {0} ({1}), {2}", city.Name, city.Id, countryName);
                        }
                        cityIdLookupByCountryCity[countryCity] = city.Id;
                    }
                }
            }
        }

        public static int UpsertConvs(List<Conv> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var duplicateCount = 0;
            var unmatchedCount = 0;
            var itemCount = 0;
            using (var db = new ClientPortalProgContext())
            {
                foreach (var item in items)
                {
                    var targets = db.Convs.Where(c => c.AccountId == item.AccountId && c.Time == item.Time);
                    
                    if (targets.Count() > 1) // found more than one
                    {
                        unmatchedCount++;
                    }
                    else
                    {
                        var target = targets.FirstOrDefault();
                        if (target == null) // add...
                        {
                            db.Convs.Add(item);
                            addedCount++;
                        }
                        else // update...
                        {
                            var entry = db.Entry(target);
                            if (entry.State == EntityState.Unchanged)
                            {
                                item.Id = target.Id; // so the target's Id won't get zeroed out

                                entry.State = EntityState.Detached;
                                AutoMapper.Mapper.Map(item, target);
                                entry.State = EntityState.Modified;
                                updatedCount++;
                            }
                            else
                            {
                                Logger.Warn("Encountered duplicate for {0:d} - Strategy {1} | TDad {2}", item.Time, item.StrategyId, item.TDadId);
                                duplicateCount++;
                            }
                        }
                    }
                    itemCount++;
                }
                Logger.Info("Processing {0} Conversions ({1} updates, {2} additions, {3} duplicates, {4} unmatched)",
                            itemCount, updatedCount, addedCount, duplicateCount, unmatchedCount);
                if (duplicateCount > 0)
                    Logger.Warn("Encountered {0} duplicates which were skipped", duplicateCount);
                int numChanges = db.SaveChanges();
            }
            return itemCount;
        }
    }
}
