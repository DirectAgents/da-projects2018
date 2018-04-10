using System.Collections.Generic;
using System.Linq;
using DBM;
using DBM.Entities;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class DBMCreativeExtracter : Extracter<Creative>
    {
        protected DBMUtility _dbmUtility;
        protected IEnumerable<string> CreativeEids;

        public DBMCreativeExtracter(string[] creativeEids, DBMUtility dbmUtility = null)
        {
            this._dbmUtility = dbmUtility ?? new DBMUtility(m => Logger.Info(m), m => Logger.Warn(m));
            this.CreativeEids = creativeEids;
        }

        protected override void Extract()
        {
            Logger.Info("Attempting to extract {0} DBM Creatives", CreativeEids.Count());
            foreach (string creativeEid in CreativeEids)
            {
                //var creative = _dbmUtility.GetCreative(creativeEid);
                //if (creative != null)
                //    Add(creative);
            }
            End();
        }
    }
}
