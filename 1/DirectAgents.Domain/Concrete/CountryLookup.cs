using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Model.Staging;

namespace DirectAgents.Domain.Concrete
{
    internal class CountryLookup
    {
        CakeStagingEntities cake;
        IDictionary<int, IEnumerable<string>> lookup;

        public CountryLookup(CakeStagingEntities cake)
        {
            this.cake = cake;
        }

        public IEnumerable<string> this[int offerID]
        {
            get
            {
                if (this.lookup == null)
                {
                    var offers = from o in cake.CakeOffers.ToList()
                                 let Id = o.Offer_Id
                                 let Countries = from part in SplitOfferName(o.OfferName)
                                                 where IsCountryCode(part)
                                                 select part
                                 select new { Id, Countries };

                    this.lookup = offers.ToDictionary(x => x.Id, x => x.Countries);
                }
                return this.lookup[offerID];
            }
        }

        static string[] SplitOfferName(string offerName)
        {
            // If one of these strings is found, everything including and after it will be stripped off when scanning for country codes
            int pos = -1;
            if (pos == -1) pos = offerName.IndexOf("MS Guide");
            if (pos == -1) pos = offerName.IndexOf("SC Johnson");
            if (pos == -1) pos = offerName.IndexOf("NO EMAIL");
            if (pos == -1) pos = offerName.IndexOf("NO NETWORKS");
            if (pos == -1) pos = offerName.IndexOf("BY APPROVAL");
            if (pos == -1) pos = offerName.IndexOf("IN CAKE");
            if (pos == -1) pos = offerName.IndexOf("DO NOT USE");
            if (pos >= 0) offerName = offerName.Substring(0, pos);

            var split = offerName.Split(new[] { ' ', '-', '/' }, StringSplitOptions.RemoveEmptyEntries);
            return split;
        }

        static string[] IgnoreCodes = { "DA", "LP", "OS", "PC", "RX", "HP" };
        static bool IsCountryCode(string str)
        {
            bool result = str.Length == 2 && !IgnoreCodes.Contains(str) && str.ToCharArray().All(c => IsUpperAlpha(c));
            return result;
        }

        static bool IsUpperAlpha(char c)
        {
            return c >= 'A' && c <= 'Z';
        }
    }
}
