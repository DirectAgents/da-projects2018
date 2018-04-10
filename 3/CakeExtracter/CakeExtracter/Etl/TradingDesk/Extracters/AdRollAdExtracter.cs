using System.Collections.Generic;
using System.Linq;
using AdRoll;
using AdRoll.Entities;

namespace CakeExtracter.Etl.TradingDesk.Extracters
{
    public class AdRollAdExtracter : Extracter<Ad>
    {
        protected AdRollUtility _arUtility;
        protected IEnumerable<string> AdEids;

        public AdRollAdExtracter(string[] adEids, AdRollUtility arUtility = null)
        {
            this._arUtility = arUtility ?? new AdRollUtility(m => Logger.Info(m), m => Logger.Warn(m));
            this.AdEids = adEids;
        }

        protected override void Extract()
        {
            Logger.Info("Attempting to extract {0} AdRoll Ads - will check for null/empty, etc", AdEids.Count());
            foreach (string adEid in AdEids)
            {
                if (!string.IsNullOrWhiteSpace(adEid))
                {
                    var ad = _arUtility.GetAd(adEid);
                    if (ad != null)
                        Add(ad);
                }
            }
            End();
        }
    }
}
