using Amazon.Entities;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CakeExtracter.Etl.TradingDesk.LoadersDA
{
    public class AmazonAdSetSummaryLoader : Loader<AdSetSummary>
    {
        private TDAdSetSummaryLoader tdAdSetSummaryLoader;

        public AmazonAdSetSummaryLoader(int accountId = -1)
        {
            this.tdAdSetSummaryLoader = new TDAdSetSummaryLoader(accountId);
        }

        protected override int Load(List<AdSetSummary> items)
        {
            Logger.Info("Loading {0} Amazon AdSet and Summary data:", items.Count);

            tdAdSetSummaryLoader.AddUpdateDependentStrategies(items);
            tdAdSetSummaryLoader.AddUpdateDependentAdSets(items);
            tdAdSetSummaryLoader.AssignAdSetIdToItems(items);
            return tdAdSetSummaryLoader.UpsertDailySummaries(items);            
        }
    }
}
