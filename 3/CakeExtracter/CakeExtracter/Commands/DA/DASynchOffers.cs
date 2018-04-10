using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.DALoaders;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using DirectAgents.Domain.Contexts;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchOffers : ConsoleCommand
    {
        public static int RunStatic(int advertiserId, bool loadInactive)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchOffers
            {
                AdvertiserId = advertiserId,
                LoadInactive = loadInactive
            };
            return cmd.Run();
        }

        public int AdvertiserId { get; set; }
        public int OfferId { get; set; }
        public bool LoadInactive { get; set; }

        public override void ResetProperties()
        {
            AdvertiserId = 0;
            OfferId = 0;
            LoadInactive = false;
        }

        public DASynchOffers()
        {
            IsCommand("daSynchOffers", "synch Offers");
            HasOption<int>("a|advertiserId=", "Advertiser Id (0 = all (default))", c => AdvertiserId = c);
            HasOption<int>("o|offerId=", "Offer Id (0 = all (default))", c => OfferId = c);
            HasOption("l|loadInactive=", "load inactive offers (default is false)", c => LoadInactive = bool.Parse(c));
        }

        public override int Execute(string[] remainingArguments)
        {
            var extracter = new OffersExtracter(advertiserId: AdvertiserId, offerIds: GetOfferIds());
            var loader = new DAOffersLoader(LoadInactive);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
            return 0;
        }

        private IEnumerable<int> GetOfferIds()
        {
            var offerIds = new int[] { OfferId };
            //int[] offerIds;
            //using (var db = new DAContext())
            //{
            //    offerIds = db.Offers.Select(o => o.OfferId).ToArray();
            //}
            return offerIds;
        }
    }
}
