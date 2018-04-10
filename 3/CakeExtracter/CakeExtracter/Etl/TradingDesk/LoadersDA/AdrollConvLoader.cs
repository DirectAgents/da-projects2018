using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.Extracters;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class AdrollConvLoader : Loader<AdrollConvRow>
    {
        private readonly int accountId;
        private Dictionary<string, int?> strategyIdLookupByCampName = new Dictionary<string, int?>();
        private Dictionary<string, int?> adIdLookupByName = new Dictionary<string, int?>();
        private Dictionary<string, int> countryIdLookupByName = new Dictionary<string, int>();
        private Dictionary<string, int> cityIdLookupByCountryCity = new Dictionary<string, int>();

        public AdrollConvLoader(int acctId)
        {
            this.accountId = acctId;
        }

        protected override int Load(List<AdrollConvRow> items)
        {
            Logger.Info("Loading {0} Adroll Conversions..", items.Count);
            UpdateStrategyLookup(items);
            UpdateAdLookup(items);
            AddUpdateDependentCities(items);
            var convs = items.Select(c => CreateConv(c)).ToList();
            var count = TDConvLoader.UpsertConvs(convs);
            return count;
        }

        public Conv CreateConv(AdrollConvRow convRow)
        {
            var countryCity = convRow.Country + "_" + convRow.City;
            int? cityId = cityIdLookupByCountryCity.ContainsKey(countryCity) ? cityIdLookupByCountryCity[countryCity] : (int?)null;
            var conv = new Conv
            {
                AccountId = accountId,
                Time = convRow.ConvTime,
                ConvType = ConvTypeAbbrev(convRow.ConvType),
                StrategyId = strategyIdLookupByCampName[convRow.Campaign], // could be null
                TDadId = adIdLookupByName[convRow.Ad], // could be null
                ConvVal = decimal.Parse(convRow.ConvVal, NumberStyles.Currency),
                CityId = cityId,
                //ExtData = convRow.ext_data_user_id
                ExtData = convRow.ext_data_order_id
            };
            return conv;
        }
        public static string ConvTypeAbbrev(string conversionType)
        {
            conversionType = conversionType.Trim();
            if (string.IsNullOrEmpty(conversionType))
                return conversionType;
            else
                return conversionType.Substring(0, 1).ToLower();
        }

        private void AddUpdateDependentCities(List<AdrollConvRow> items)
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

                        //StartsWith(cityName) creates compatibility with DBM city names.
                        var city = db.ConvCities.Where(c => c.CountryId == countryId && c.Name.StartsWith(cityName)).FirstOrDefault();
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

        private void UpdateStrategyLookup(List<AdrollConvRow> items)
        {
            var campNames = items.Select(i => i.Campaign).Distinct();

            using (var db = new ClientPortalProgContext())
            {
                foreach (var campName in campNames)
                {
                    if (strategyIdLookupByCampName.ContainsKey(campName))
                        continue; // already encountered

                    var strats = db.Strategies.Where(s => s.AccountId == accountId && s.Name == campName);
                    if (strats.Count() == 1)
                    {
                        var strat = strats.First();
                        strategyIdLookupByCampName[campName] = strat.Id;
                    }
                    else
                        strategyIdLookupByCampName[campName] = null; // leave StrategyId blank in Conv
                }
            }
        }

        private void UpdateAdLookup(List<AdrollConvRow> items)
        {
            var adNames = items.Select(i => i.Ad).Distinct();

            using (var db = new ClientPortalProgContext())
            {
                foreach (var adName in adNames)
                {
                    if (adIdLookupByName.ContainsKey(adName))
                        continue; // already encountered

                    var tdAds = db.TDads.Where(a => a.AccountId == accountId && a.Name == adName);
                    if (tdAds.Count() == 1)
                    {
                        var tdAd = tdAds.First();
                        adIdLookupByName[adName] = tdAd.Id;
                    }
                    else
                        adIdLookupByName[adName] = null; // leave TDadId blank in Conv
                }
            }
        }
    }
}
