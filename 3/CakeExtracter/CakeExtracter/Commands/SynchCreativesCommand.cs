using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using CakeExtracter.Etl.CakeMarketing.Loaders;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchCreativesCommand : ConsoleCommand
    {
        public static int RunStatic(int advertiserId, int? offerId, bool overwriteNames)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new SynchCreativesCommand
            {
                AdvertiserId = advertiserId,
                OfferId = offerId,
                OverwriteNames = overwriteNames
            };
            return cmd.Run();
        }

        public int AdvertiserId { get; set; }
        public int? OfferId { get; set; }
        public bool OverwriteNames { get; set; }

        public override void ResetProperties()
        {
            AdvertiserId = 0;
            OfferId = null;
            OverwriteNames = true;
        }

        public SynchCreativesCommand()
        {
            IsCommand("synchCreatives", "synch Creatives");
            HasOption<int>("a|advertiserId=", "Advertiser Id (0 = all (default))", c => AdvertiserId = c);
            HasOption<int>("o|offerId=", "Offer Id (default = all)", c => OfferId = c);
            HasOption("n|overwriteNames=", "overwrite names of existing Creatives (default is true)", c => OverwriteNames = bool.Parse(c));
        }

        public override int Execute(string[] remainingArguments)
        {
            var offers = GetOffers();
            foreach (var offer in offers)
            {
                var extracter = new CreativesExtracter(offer.OfferId);
                var loader = new CreativesLoader(offer.OfferId, this.OverwriteNames);
                var extracterThread = extracter.Start();
                var loaderThread = loader.Start(extracter);
                extracterThread.Join();
                loaderThread.Join();

                using (var db = new ClientPortalContext())
                {
                    var off = db.Offers.FirstOrDefault(o => o.OfferId == offer.OfferId);
                    if (off != null)
                    {
                        off.LastSynch_Creatives = DateTime.Now;
                        db.SaveChanges();
                    }
                }
            }
            return 0;
        }

        private IEnumerable<Offer> GetOffers()
        {
            using (var db = new ClientPortalContext())
            {
                var offers = db.Offers.AsQueryable();
                if (this.AdvertiserId != 0)
                    offers = offers.Where(o => o.AdvertiserId == AdvertiserId);
                if (this.OfferId.HasValue)
                    offers = offers.Where(o => o.OfferId == OfferId.Value);

                return offers.ToList();
            }

        }
    }
}
