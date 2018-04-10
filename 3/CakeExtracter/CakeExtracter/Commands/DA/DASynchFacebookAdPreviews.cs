using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using CakeExtracter.Bootstrappers;
using CakeExtracter.Common;
using CakeExtracter.Etl.SocialMarketing.Extracters;
using CakeExtracter.Etl.SocialMarketing.LoadersDA;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using FacebookAPI;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class DASynchFacebookAdPreviews : ConsoleCommand
    {
        public static int RunStatic(int? accountId = null)
        {
            AutoMapperBootstrapper.CheckRunSetup();
            var cmd = new DASynchFacebookAdPreviews
            {
                AccountId = accountId
            };
            return cmd.Run();
        }

        public int? AccountId { get; set; }
        //public DateTime? StartDate { get; set; }
        //public DateTime? EndDate { get; set; }

        public override void ResetProperties()
        {
            //StartDate = null;
            //EndDate = null;
        }

        public DASynchFacebookAdPreviews()
        {
            IsCommand("daSynchFacebookAdPreviews", "synch Facebook ads");
            HasOption<int>("a|accountId=", "Account Id (default = all)", c => AccountId = c);
        }

        public override int Execute(string[] remainingArguments)
        {
            var fbUtility = new FacebookUtility(m => Logger.Info(m), m => Logger.Warn(m));

            var accounts = GetAccounts();
            foreach (var acct in accounts)
            {
                DoETL_AdPreview(acct, fbUtility);
            }

            return 0;
        }

        public void DoETL_AdPreview(ExtAccount account, FacebookUtility fbUtility = null)
        {
            var fbIds = GetAdFBIds(account.Id);
            if (fbIds.Count() > 0)
            {
                var extracter = new FacebookAdPreviewExtracter(account.ExternalId, GetAdFBIds(account.Id), fbUtility);
                var loader = new FacebookAdPreviewLoader(account.Id);
                var extracterThread = extracter.Start();
                var loaderThread = loader.Start(extracter);
                extracterThread.Join();
                loaderThread.Join();
            }
        }

        public IEnumerable<ExtAccount> GetAccounts()
        {
            using (var db = new ClientPortalProgContext())
            {
                var accounts = db.ExtAccounts.Where(a => a.Platform.Code == Platform.Code_FB);
                if (AccountId.HasValue)
                    accounts = accounts.Where(a => a.Id == AccountId.Value);

                return accounts.ToList().Where(a => !string.IsNullOrWhiteSpace(a.ExternalId));
            }
        }

        public IEnumerable<string> GetAdFBIds(int accountId)
        {
            using (var db = new ClientPortalProgContext())
            {
                var fbIds =
                    from a in db.TDads
                    where (a.AccountId == accountId) && (a.ExternalId != null)
                    select a.ExternalId;
                return fbIds.ToList();
            }
        }

    }
}
