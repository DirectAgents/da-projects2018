using System.ComponentModel.Composition;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.CakeMarketing.DALoaders;
using CakeExtracter.Etl.CakeMarketing.Extracters;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchAdvertisers : ConsoleCommand
    {
        public static int RunStatic(int advertiserId, bool includeContacts, bool synchOffersAlso)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchAdvertisers
            {
                AdvertiserId = advertiserId,
                IncludeContacts = includeContacts,
                SynchOffersAlso = synchOffersAlso
            };
            return cmd.Run();
        }

        public int AdvertiserId { get; set; }
        public bool IncludeContacts { get; set; }
        public bool SynchOffersAlso { get; set; }

        public override void ResetProperties()
        {
            AdvertiserId = 0;
            IncludeContacts = false;
            SynchOffersAlso = false;
        }

        public DASynchAdvertisers()
        {
            IsCommand("daSynchAdvertisers", "synch Advertisers");
            HasOption<int>("a|advertiserId=", "Advertiser Id (0 = all (default))", c => AdvertiserId = c);
            HasOption("c|contacts=", "synch Contacts also (default is false)", c => IncludeContacts = bool.Parse(c));
            HasOption("o|offers=", "synch Offers also (default is false)", c => SynchOffersAlso = bool.Parse(c));
        }

        public override int Execute(string[] remainingArguments)
        {
            var extracter = new AdvertisersExtracter(AdvertiserId);
            var loader = new DAAdvertisersLoader(IncludeContacts);
            var extracterThread = extracter.Start();
            var loaderThread = loader.Start(extracter);
            extracterThread.Join();
            loaderThread.Join();

            if (SynchOffersAlso)
                DASynchOffers.RunStatic(AdvertiserId, false);

            return 0;
        }
    }
}
