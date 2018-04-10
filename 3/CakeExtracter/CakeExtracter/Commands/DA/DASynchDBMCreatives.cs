using System;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.LoadersDA;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchDBMCreatives : ConsoleCommand
    {
        public override void ResetProperties()
        {
        }

        public DASynchDBMCreatives()
        {
            IsCommand("DASynchDBMCreatives", "synch DBM Creatives");
        }

        public override int Execute(string[] remainingArguments)
        {
            using (var db = new ClientPortalProgContext())
            {
                var platformId_DBM = db.Platforms.Where(p => p.Code == Platform.Code_DBM).First().Id;
                var extAccounts = db.ExtAccounts.Where(a => a.PlatformId == platformId_DBM && !String.IsNullOrEmpty(a.ExternalId));
                foreach (var extAcct in extAccounts)
                {
                    Logger.Info("Synch Ads for Account: {0} ({1})", extAcct.Name, extAcct.ExternalId);
                    var tdAds = db.TDads.Where(a => a.AccountId == extAcct.Id);
                    var adEids = tdAds.Select(a => a.ExternalId).ToArray();

                    var creativeExtracter = new DBMCreativeExtracter(adEids);
                    //var adLoader = new AdRollAdLoader(extAcct.Id);
                    var creativeExtracterThread = creativeExtracter.Start();
                    //var adLoaderThread = adLoader.Start(creativeExtracter);
                    creativeExtracterThread.Join();
                    //adLoaderThread.Join();
                }
            }
            return 0;
        }
    }
}
