using System.ComponentModel.Composition;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.Extracters;
using CakeExtracter.Etl.CakeMarketing.Loaders;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class SynchOffersCommand : ConsoleCommand
    {
        public static int RunStatic(int advertiserId)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new SynchOffersCommand
            {
                AdvertiserId = advertiserId
            };
            return cmd.Run();
        }

        public int AdvertiserId { get; set; }

        public override void ResetProperties()
        {
            AdvertiserId = 0;
        }

        public SynchOffersCommand()
        {
            IsCommand("synchOffers", "synch Offers");
            HasOption<int>("a|advertiserId=", "Advertiser Id (0 = all (default))", c => AdvertiserId = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var extracter = new OffersExtracter(advertiserId: AdvertiserId);
            var loader = new OffersLoader();
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();
            return 0;
        }
    }
}
