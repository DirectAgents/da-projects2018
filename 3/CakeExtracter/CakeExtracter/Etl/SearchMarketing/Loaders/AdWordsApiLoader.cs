using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Etl.SearchMarketing.Loaders
{
    public class AdWordsApiLoader : Loader<Dictionary<string, string>>
    {
        public const string GoogleChannel = "Google";
        private readonly int searchAccountId;
        private readonly bool includeClickType; // if true, we use SearchDailySummary2's
        private readonly bool clickAssistConvStats;

        private Dictionary<string, Dictionary<DateTime, decimal>> currencyMultipliers = new Dictionary<string, Dictionary<DateTime, decimal>>();

        public AdWordsApiLoader(int searchAccountId, bool includeClickType, bool clickAssistConvStats = false)
        {
            this.searchAccountId = searchAccountId;
            this.includeClickType = includeClickType;
            this.clickAssistConvStats = clickAssistConvStats;
        }

        protected override int Load(List<Dictionary<string, string>> items)
        {
            Logger.Info("Loading {0} SearchDailySummaries..", items.Count);
            SetCurrencyMultipliers(items, this.currencyMultipliers);
            AddUpdateDependentSearchAccounts(items, this.searchAccountId);
            AddUpdateDependentSearchCampaigns(items, this.searchAccountId);
            var count = UpsertSearchDailySummaries(items);
            return count;
        }

        public static string Network_StringToLetter(string network)
        {
            if (network == null)
                return null;
            else if (network == "YouTube Videos")
                return "V"; // Y will be for "YouTube Search"
            else
                return network.Substring(0, 1);
        }
        public static string Device_StringToLetter(string device)
        {
            if (device == null)
                return null;
            else
                return device.Substring(0, 1);
        }
        public static string ClickType_StringToLetter(string clickType)
        {
            var clickTypeAbbrev = clickType.Substring(0, 1);
            var ctLower = clickType.ToLower();
            if (ctLower == "product listing ad - coupon") // started on 10/18/14 for Folica|Search (conflict with "Product listing ad")
                clickTypeAbbrev = "Q";
            else if (ctLower == "phone calls") // noticed for The Credit Pros -> "DA Spanish Mobile" on 11/11/17
                clickTypeAbbrev = "C";
            return clickTypeAbbrev;
        }

        private int UpsertSearchDailySummaries(List<Dictionary<string, string>> items)
        {
            var addedCount = 0;
            var updatedCount = 0;
            var itemCount = 0;
            //var networks = items.Select(i => i["network"]).Distinct();
            //var devices = items.Select(i => i["device"]).Distinct();
            //var clickTypes = items.Select(i => i["clickType"]).Distinct();
            //var pla = items.Where(i => i["clickType"] == "Product listing ad").OrderBy(i => i["campaignID"]);
            //var plac = items.Where(i => i["clickType"] == "Product Listing Ad - Coupon").OrderBy(i => i["campaignID"]);
            using (var db = new ClientPortalContext())
            {
                var passedInAccount = db.SearchAccounts.Find(this.searchAccountId);

                foreach (var item in items)
                {
                    var customerId = item["customerID"];
                    var campaignId = int.Parse(item["campaignID"]);

                    var searchAccount = passedInAccount;
                    if (searchAccount.ExternalId != customerId)
                        searchAccount = searchAccount.SearchProfile.SearchAccounts.Single(sa => sa.ExternalId == customerId && sa.Channel == GoogleChannel);

                    var sds = new SearchDailySummary
                    {   // the basic fields
                        SearchCampaignId = searchAccount.SearchCampaigns.Single(c => c.ExternalId == campaignId).SearchCampaignId,
                        Date = DateTime.Parse(item["day"].Replace('-', '/')),
                        CurrencyId = (!item.Keys.Contains("currency") || item["currency"] == "USD") ? 1 : -1, // NOTE: non USD (if exists) -1 for now
                        Network = Network_StringToLetter(item["network"])
                    };

                    if (clickAssistConvStats)
                    {
                        sds.Device = "A"; // mark for "all" devices
                        sds.CassConvs = int.Parse(item["clickAssistedConv"]);
                        sds.CassConVal = double.Parse(item["clickAssistedConvValue"]);
                    }
                    else
                    {
                        sds.Revenue = decimal.Parse(item["totalConvValue"]);
                        sds.Cost = decimal.Parse(item["cost"]) / 1000000; // convert from microns to dollars
                        var conversions = double.Parse(item["conversions"]);
                        sds.Orders = Convert.ToInt32(conversions); // default rounding - nearest even # if .5
                        sds.Clicks = int.Parse(item["clicks"]);
                        sds.Impressions = int.Parse(item["impressions"]);
                        sds.Device = Device_StringToLetter(item["device"]);
                        sds.ViewThrus = int.Parse(item["viewThroughConv"]);

                        if (searchAccount.RevPerOrder.HasValue)
                            sds.Revenue = sds.Orders * searchAccount.RevPerOrder.Value;
                    }

                    // Adjust revenue and cost if there's a Currency Multiplier...
                    if (!item.Keys.Contains("currency") || item["currency"] == "USD")
                        sds.CurrencyId = 1; // USD or not specified
                    else
                    {
                        var code = item["currency"];
                        var firstOfMonth = new DateTime(sds.Date.Year, sds.Date.Month, 1);
                        if (!currencyMultipliers.ContainsKey(code) || !currencyMultipliers[code].ContainsKey(firstOfMonth))
                            sds.CurrencyId = -1;
                        else
                        {
                            sds.CurrencyId = 1;
                            var toUSDmult = currencyMultipliers[code][firstOfMonth];
                            sds.Revenue = sds.Revenue * toUSDmult;
                            sds.Cost = sds.Cost * toUSDmult;
                            //TODO? Do for CassConVal as well?
                        }
                    }

                    bool added;
                    if (includeClickType)
                        added = UpsertSearchDailySummary2(db, sds, item["clickType"]);
                    else
                        added = UpsertSearchDailySummary(db, sds);

                    if (added)
                        addedCount++;
                    else
                        updatedCount++;

                    itemCount++;
                }
                Logger.Info("Saving {0} SearchDailySummaries ({1} updates, {2} additions)", itemCount, updatedCount, addedCount);
                db.SaveChanges();
            }
            return itemCount;
        }

        // return true if added; false if updated
        private bool UpsertSearchDailySummary(ClientPortalContext db, SearchDailySummary sds)
        {
            var target = db.Set<SearchDailySummary>().Find(sds.SearchCampaignId, sds.Date, sds.Network, sds.Device);
            if (target == null)
            {
                db.SearchDailySummaries.Add(sds);
                return true;
            }
            else
            {
                db.Entry(target).State = EntityState.Detached;

                ////For Megabus conversions fix. Also comment out sds.Cost/Clicks/Impressions assignments above.
                //sds.Impressions = target.Impressions;
                //sds.Clicks = target.Clicks;
                //sds.Cost = target.Cost;

                target = AutoMapper.Mapper.Map(sds, target);
                db.Entry(target).State = EntityState.Modified;
                return false;
            }
        }
        private bool UpsertSearchDailySummary2(ClientPortalContext db, SearchDailySummary sds, string clickType)
        {
            var clickTypeAbbrev = ClickType_StringToLetter(clickType);
            var sds2 = new SearchDailySummary2
            {
                SearchCampaignId = sds.SearchCampaignId,
                Date = sds.Date,
                Network = sds.Network,
                Device = sds.Device,
                ClickType = clickTypeAbbrev,
                Revenue = sds.Revenue,
                Cost = sds.Cost,
                Orders = sds.Orders,
                Clicks = sds.Clicks,
                // HACK: ignoring impressions for rows that do not have H or C as click type
                Impressions = (clickTypeAbbrev == "H" || clickTypeAbbrev == "C") ? sds.Impressions : 0,
                CurrencyId = sds.CurrencyId
            };

            var target = db.Set<SearchDailySummary2>().Find(sds2.SearchCampaignId, sds2.Date, sds2.Network, sds2.Device, sds2.ClickType);
            if (target == null)
            {
                db.SearchDailySummary2.Add(sds2);
                return true;
            }
            else
            {
                db.Entry(target).State = EntityState.Detached;
                target = AutoMapper.Mapper.Map(sds2, target);
                db.Entry(target).State = EntityState.Modified;
                return false;
            }
        }

        public static void SetCurrencyMultipliers(List<Dictionary<string, string>> items, Dictionary<string, Dictionary<DateTime, decimal>> currMults)
        {
            if (!items.Any() || !items[0].Keys.Contains("currency"))
                return;

            var currencyTuples1 = items.Where(i => i["currency"] != "USD")
                                       .Select(i => Tuple.Create(i["currency"], DateTime.Parse(i["day"].Replace('-', '/')))).Distinct();
            var currencyTuples = currencyTuples1.Select(t => Tuple.Create(t.Item1, new DateTime(t.Item2.Year, t.Item2.Month, 1))).Distinct().ToList();
            var currencyGroups = currencyTuples.GroupBy(ct => ct.Item1); // group by currency code

            foreach (var cg in currencyGroups) // (foreach currency code)
            {
                if (currMults.ContainsKey(cg.Key))
                { // See if dictionary values are already set
                    var monthsWithoutMultiplier = cg.Where(t => !currMults[cg.Key].ContainsKey(t.Item2));
                    if (!monthsWithoutMultiplier.Any())
                        continue; // they're all there already
                }
                else
                {
                    currMults.Add(cg.Key, new Dictionary<DateTime, decimal>());
                }
                var singleCurrencyMultipliers = currMults[cg.Key];

                using (var db = new ClientPortalContext())
                {
                    var currConversions = db.CurrencyConversions.Where(c => c.Currency.Code == cg.Key).OrderBy(c => c.Date).ToList();
                    if (!currConversions.Any())
                        continue;

                    DateTime earliestConvDate = currConversions.First().Date;
                    foreach (var currTuple in cg) // (foreach month)
                    {
                        if (singleCurrencyMultipliers.ContainsKey(currTuple.Item2))
                            continue;

                        if (currTuple.Item2 < earliestConvDate)
                        { // Use the oldest CurrencyConversion
                            singleCurrencyMultipliers.Add(currTuple.Item2, currConversions.First().ToUSDmultiplier);
                        }
                        else
                        { // Use the most recent CurrencyConversion that's <= the currTuple's date
                            var currConv = currConversions.Where(c => c.Date <= currTuple.Item2).OrderBy(c => c.Date).Last();
                            singleCurrencyMultipliers.Add(currTuple.Item2, currConv.ToUSDmultiplier);
                        }
                    }
                }
            }
        }

        public static void AddUpdateDependentSearchAccounts(List<Dictionary<string, string>> items, int searchAccountId)
        {
            using (var db = new ClientPortalContext())
            {
                var searchAccount = db.SearchAccounts.Find(searchAccountId);

                var accountTuples = items.Select(i => Tuple.Create(i["account"], i["customerID"])).Distinct();
                bool multipleAccounts = accountTuples.Count() > 1;

                foreach (var tuple in accountTuples)
                {
                    var accountName = tuple.Item1;
                    var customerId = tuple.Item2;
                    SearchAccount existing = null;

                    if (searchAccount.ExternalId == customerId || !multipleAccounts)
                    {   // if the customerId matches or if there's only one account in the items-to-load, use the passed-in SearchAccount
                        existing = searchAccount;
                    }
                    else
                    {   // See if there are any sibling SearchAccounts that match by customerId... or finally, by name
                        existing = searchAccount.SearchProfile.SearchAccounts.SingleOrDefault(sa => sa.ExternalId == customerId && sa.Channel == GoogleChannel);
                        if (existing == null)
                            existing = searchAccount.SearchProfile.SearchAccounts.SingleOrDefault(sa => sa.Name == accountName && sa.Channel == GoogleChannel);
                    }

                    if (existing == null)
                    {
                        searchAccount.SearchProfile.SearchAccounts.Add(new SearchAccount
                        {
                            Name = accountName,
                            Channel = GoogleChannel,
                            //AccountCode = , // todo: have extracter get client code
                            ExternalId = customerId
                        });
                        Logger.Info("Saving new SearchAccount: {0} ({1})", accountName, customerId);
                    }
                    else
                    {
                        if (existing.Name != accountName)
                        {
                            existing.Name = accountName;
                            Logger.Info("Saving updated SearchAccount name: {0} ({1})", accountName, existing.SearchAccountId);
                        }
                        if (existing.ExternalId != customerId)
                        {
                            existing.ExternalId = customerId;
                            Logger.Info("Saving updated SearchAccount ExternalId: {0} ({1})", customerId, existing.SearchAccountId);
                        }
                    }
                    db.SaveChanges();
                }
            }
        }

        public static void AddUpdateDependentSearchCampaigns(List<Dictionary<string, string>> items, int searchAccountId)
        {
            using (var db = new ClientPortalContext())
            {
                var passedInAccount = db.SearchAccounts.Find(searchAccountId);
                var campaignTuples = items.Select(c => Tuple.Create(c["customerID"], c["campaign"], c["campaignID"])).Distinct();

                foreach (var tuple in campaignTuples)
                {
                    var customerId = tuple.Item1;
                    var campaignName = tuple.Item2;
                    var campaignId = int.Parse(tuple.Item3);

                    var searchAccount = passedInAccount;
                    if (searchAccount.ExternalId != customerId)
                        searchAccount = searchAccount.SearchProfile.SearchAccounts.Single(sa => sa.ExternalId == customerId && sa.Channel == GoogleChannel);

                    var existing = searchAccount.SearchCampaigns.SingleOrDefault(c => c.ExternalId == campaignId);

                    if (existing == null)
                    {
                        searchAccount.SearchCampaigns.Add(new SearchCampaign
                        {
                            SearchCampaignName = campaignName,
                            ExternalId = campaignId
                        });
                        Logger.Info("Saving new SearchCampaign: {0} ({1})", campaignName, campaignId);
                        db.SaveChanges();
                    }
                    else if(existing.SearchCampaignName != campaignName)
                    {
                        existing.SearchCampaignName = campaignName;
                        Logger.Info("Saving updated SearchCampaign name: {0} ({1})", campaignName, campaignId);
                        db.SaveChanges();
                    }
                }
            }
        }
    }
}
