using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using CakeExtracter.Common;
using CakeExtracter.Etl.TradingDesk.Extracters;
using CakeExtracter.Etl.TradingDesk.Loaders;
using ClientPortal.Data.Entities.TD;
using ClientPortal.Data.Entities.TD.DBM;

namespace CakeExtracter.Commands
{
    [Export(typeof(ConsoleCommand))]
    public class TDSynchUserListStats : ConsoleCommand
    {
        public int? UserListRunID { get; set; }

        public override void ResetProperties()
        {
            UserListRunID = null;
        }

        public TDSynchUserListStats()
        {
            IsCommand("tdSynchUserListStats", "synch user list stats");
            HasOption<int>("r|userListRunID=", "UserListRun ID (default = all)", c => UserListRunID = c);
            // min match ratio (default 100)
            // min uniques (default 1000) ?
        }

        public override int Execute(string[] remainingArguments)
        {
            foreach (var userListRun in GetUserListRuns())
            {
                // extracter, loader, start/join
                var extracter = new DbmUserListStatExtracter(userListRun);
                var loader = new DbmUserListStatLoader(userListRun.ID);
                var extracterThread = extracter.Start();
                var loaderThread = loader.Start(extracter);
                extracterThread.Join();
                loaderThread.Join();
            }
            return 0;
        }

        public IEnumerable<UserListRun> GetUserListRuns()
        {
            using (var db = new TDContext())
            {
                var userListRuns = db.UserListRuns.AsQueryable();
                if (this.UserListRunID.HasValue)
                {
                    userListRuns = userListRuns.Where(ulr => ulr.ID == UserListRunID.Value);
                }
                return userListRuns.Where(ulr => ulr.Bucket != null).ToList();
                // exclude UserListRuns that don't have a bucket
            }
        }
    }
}
