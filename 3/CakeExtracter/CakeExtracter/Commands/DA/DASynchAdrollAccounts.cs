using System.ComponentModel.Composition;
using System.Linq;
using AdRoll;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.AdRoll;
using DirectAgents.Domain.Entities.CPProg;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchAdrollAccounts : ConsoleCommand
    {
        public static int RunStatic()
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchAdrollAccounts();
            return cmd.Run();
        }

        public override void ResetProperties()
        {
        }

        public DASynchAdrollAccounts()
        {
            IsCommand("daSynchAdrollAccounts", "synch AdRoll Accounts(Advertisables)");
        }

        public override int Execute(string[] remainingArguments)
        {
            UpdateAdvertisables();
            return 0;
        }

        public static void UpdateAdvertisables(AdRollUtility arUtility = null)
        {
            if (arUtility == null)
                arUtility = new AdRollUtility(m => Logger.Info(m), m => Logger.Warn(m));

            var freshAdvertisables = arUtility.GetAdvertisables();
            if (!freshAdvertisables.Any())
            {
                Logger.Warn("No Advertisables returned. Aborting synch.");
                return;
            }
            var freshEids = freshAdvertisables.Select(a => a.eid).ToArray();
            using (var db = new ClientPortalProgContext())
            {
                var dbAdvertisables = db.Advertisables.ToList();
                var dbAdvEids = dbAdvertisables.Select(a => a.Eid).ToArray();

                var outdatedAdvertisables = dbAdvertisables.Where(a => !freshEids.Contains(a.Eid));
                foreach (var adv in outdatedAdvertisables)
                    adv.Eid = null; // so DASynchAdrollStats doesn't pick it up

                var platformId_AdRoll = db.Platforms.Where(p => p.Code == Platform.Code_AdRoll).First().Id;
                var dbAccounts = db.ExtAccounts.Where(a => a.PlatformId == platformId_AdRoll).ToList();
                var dbAcctEids = dbAccounts.Select(a => a.ExternalId).ToArray();

                foreach (var adv in freshAdvertisables)
                {
                    // Check/update adr.Advertisable table
                    if (!dbAdvEids.Contains(adv.eid))
                    { // add
                        Logger.Info("Adding new Advertisable '{0}' ({1})", adv.name, adv.eid);
                        var newAdv = new Advertisable
                        {
                            Eid = adv.eid,
                            Name = adv.name,
                            Active = adv.is_active,
                            Status = adv.status,
                            CreatedDate = adv.created_date,
                            UpdatedDate = adv.updated_date
                        };
                        db.Advertisables.Add(newAdv);
                    }
                    else
                    { // update
                        var dbAdv = dbAdvertisables.First(a => a.Eid == adv.eid);
                        dbAdv.Name = adv.name;
                        dbAdv.Active = adv.is_active;
                        dbAdv.Status = adv.status;
                        dbAdv.CreatedDate = adv.created_date;
                        dbAdv.UpdatedDate = adv.updated_date;
                    }

                    // Check/update td.Account table ("ExtAccount")
                    string name = adv.name + (adv.is_active ? "" : " (INACTIVE)");

                    if (!dbAcctEids.Contains(adv.eid))
                    { // add
                        Logger.Info("Adding new ExtAccount '{0}' ({1})", adv.name, adv.eid);
                        var newAcct = new ExtAccount
                        {
                            PlatformId = platformId_AdRoll,
                            ExternalId = adv.eid,
                            Name = name
                        };
                        db.ExtAccounts.Add(newAcct);
                    }
                    else
                    { // update
                        var dbAcct = dbAccounts.First(a => a.ExternalId == adv.eid);
                        dbAcct.Name = name;
                    }
                }
                db.SaveChanges();
            }

        }
    }
}
