using Amazon.Entities;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class AmazonAdSummaryLoader : Loader<TDadSummary>
    {
        private TDadSummaryLoader tdAdSummaryLoader;

        public AmazonAdSummaryLoader(int accountId = -1)
        {
            this.tdAdSummaryLoader = new TDadSummaryLoader(accountId);
        }

        protected override int Load(List<TDadSummary> tDadItems)
        {
            Logger.Info("Loading {0} Amazon ProductAd / Creative data: ", tDadItems.Count);

            tdAdSummaryLoader.AddUpdateDependentTDads(tDadItems);
            tdAdSummaryLoader.AssignTDadIdToItems(tDadItems);
            var count = tdAdSummaryLoader.UpsertDailySummaries(tDadItems);
            return count;
        }
    }
}
