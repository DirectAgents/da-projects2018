using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdRoll.Entities;
using DirectAgents.Domain.Contexts;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class AdRollAdLoader : Loader<Ad>
    {
        private readonly int accountId;

        private const string patternAdUrl = @"img src=&#34;(.*?)&#34;";
        private Regex regexAdUrl = new Regex(patternAdUrl);

        public AdRollAdLoader(int acctId)
        {
            this.accountId = acctId;
        }

        protected override int Load(List<Ad> items)
        {
            Logger.Info("Updating {0} Ads..", items.Count);
            using (var db = new ClientPortalProgContext())
            {
                var dbAds = db.TDads.Where(a => a.AccountId == this.accountId);

                foreach (var itemAd in items)
                {
                    string adUrl = itemAd.src;
                    if (String.IsNullOrWhiteSpace(adUrl) && !String.IsNullOrWhiteSpace(itemAd.html))
                    { //Try extracting ad url...
                        var match = regexAdUrl.Match(itemAd.html);
                        if (match.Success)
                            adUrl = match.Groups[1].Value;
                    }

                    // should be just one, but...
                    var adMatch = dbAds.Where(a => a.ExternalId == itemAd.eid);
                    if (adMatch.Any())
                    {
                        foreach (var dbAd in adMatch)
                        {
                            if (adUrl != null)
                                dbAd.Url = adUrl;
                            dbAd.Name = itemAd.name;
                            dbAd.Width = itemAd.width;
                            dbAd.Height = itemAd.height;
                            dbAd.Headline = itemAd.headline;
                            dbAd.Body = itemAd.body;
                            dbAd.Message = itemAd.message;
                            dbAd.DestinationUrl = itemAd.destination_url;
                            // status, created_date, updated_date...
                        }
                    }
                    else
                    {
                        Logger.Warn("Ad {0} not found. Skipping update.", itemAd.eid);
                    }
                }
                db.SaveChanges();
            }
            return items.Count;
        }
    }
}
