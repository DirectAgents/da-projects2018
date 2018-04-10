using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using CakeExtracter.Etl.CakeMarketing.Loaders;
using ClientPortal.Data.Contexts;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchCampaignsCommand : ConsoleCommand
    {
        public static int RunStatic(int advertiserId, int? offerId)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new SynchCampaignsCommand
            {
                AdvertiserId = advertiserId,
                OfferId = offerId
            };
            return cmd.Run();
        }

        public int AdvertiserId { get; set; }
        public int? OfferId { get; set; }

        public override void ResetProperties()
        {
            AdvertiserId = 0;
            OfferId = null;
        }

        public SynchCampaignsCommand()
        {
            IsCommand("synchCampaigns", "synch Campaigns");
            HasOption<int>("a|advertiserId=", "Advertiser Id (0 = all (default))", c => AdvertiserId = c);
            HasOption<int>("o|offerId=", "Offer Id (default = all)", c => OfferId = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var offerIds = GetOfferIds();

            foreach (var offerIdBatch in offerIds.InBatches(4)) // doing 4 at a time
            {
                Parallel.ForEach(offerIdBatch, offerId =>
                {
                    var extracter = new CampaignsExtracter(offerId: offerId);
                    var loader = new CampaignsLoader();
                    var extracterThread = extracter.Start();
                    var loaderThread = loader.Start(extracter);
                    extracterThread.Join();
                    loaderThread.Join();

                    using (var db = new ClientPortalContext())
                    {
                        var off = db.Offers.FirstOrDefault(o => o.OfferId == offerId);
                        if (off != null)
                        {
                            off.LastSynch_Campaigns = DateTime.Now;
                            db.SaveChanges();
                        }
                    }
                });
            }
            return 0;
        }

        private IEnumerable<int> GetOfferIds()
        {
            using (var db = new ClientPortalContext())
            {
                var offers = db.Offers.AsQueryable();
                if (this.AdvertiserId != 0)
                    offers = offers.Where(o => o.AdvertiserId == AdvertiserId);
                if (this.OfferId.HasValue)
                    offers = offers.Where(o => o.OfferId == OfferId.Value);

                return offers.Select(o => o.OfferId).ToList();
            }

        }
    }
}
