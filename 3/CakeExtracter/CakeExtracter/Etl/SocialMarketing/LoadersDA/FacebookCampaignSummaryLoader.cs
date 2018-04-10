using System.Collections.Generic;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.LoadersDA;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using FacebookAPI;
using FacebookAPI.Entities;

namespace CakeExtracter.Etl.SocialMarketing.LoadersDA
{
    public class FacebookCampaignSummaryLoader : Loader<FBSummary>
    {
        private readonly bool LoadActions;
        private TDStrategySummaryLoader strategySummaryLoader;
        private Dictionary<string, int> actionTypeIdLookupByCode = new Dictionary<string, int>();

        public FacebookCampaignSummaryLoader(int accountId, bool loadActions = false)
        {
            this.BatchSize = FacebookUtility.RowsReturnedAtATime; //FB API only returns 25 rows at a time
            this.strategySummaryLoader = new TDStrategySummaryLoader(accountId);
            this.LoadActions = loadActions;
        }

        protected override int Load(List<FBSummary> items)
        {
            var dbItems = items.Select(i => CreateStrategySummary(i)).ToList();
            strategySummaryLoader.AddUpdateDependentStrategies(dbItems);
            strategySummaryLoader.AssignStrategyIdToItems(dbItems);
            var count = strategySummaryLoader.UpsertDailySummaries(dbItems);

            if (LoadActions)
            {
                AddUpdateDependentActionTypes(items);
                UpsertStrategyActions(items, dbItems);
            }
            return count;
        }

        public static StrategySummary CreateStrategySummary(FBSummary item)
        {
            var sum = new StrategySummary
            {
                Date = item.Date,
                StrategyName = item.CampaignName,
                StrategyEid = item.CampaignId,
                Impressions = item.Impressions,
                AllClicks = item.AllClicks,
                Clicks = item.LinkClicks,
                PostClickConv = item.Conversions_click,
                PostViewConv = item.Conversions_view,
                PostClickRev = item.ConVal_click,
                PostViewRev = item.ConVal_view,
                Cost = item.Spend
            };
            return sum;
        }

        private void AddUpdateDependentActionTypes(List<FBSummary> items)
        {
            FacebookAdSetSummaryLoader.AddUpdateDependentActionTypes(items, this.actionTypeIdLookupByCode);
        }

        //Note: get the actions from the items(FBSummaries); get the strategyId from the strategySummaries
        private void UpsertStrategyActions(List<FBSummary> items, List<StrategySummary> strategySummaries)
        {
            int addedCount = 0;
            int updatedCount = 0;
            int deletedCount = 0;
            using (var db = new ClientPortalProgContext())
            {
                var itemEnumerator = items.GetEnumerator();
                var ssEnumerator = strategySummaries.GetEnumerator();
                while (itemEnumerator.MoveNext())
                {
                    ssEnumerator.MoveNext();
                    if (itemEnumerator.Current.Actions == null)
                        continue;
                    var date = itemEnumerator.Current.Date;
                    var strategyId = ssEnumerator.Current.StrategyId;
                    var fbActions = itemEnumerator.Current.Actions.Values;

                    var actionTypeIds = fbActions.Select(x => actionTypeIdLookupByCode[x.ActionType]).ToArray();
                    var existingActions = db.StrategyActions.Where(x => x.Date == date && x.StrategyId == strategyId);

                    //Delete actions that no longer have stats for the date/strategy
                    foreach (var stratAction in existingActions.Where(x => !actionTypeIds.Contains(x.ActionTypeId)))
                    {
                        db.StrategyActions.Remove(stratAction);
                        deletedCount++;
                    }

                    //Add/update the rest
                    foreach (var fbAction in fbActions)
                    {
                        int actionTypeId = actionTypeIdLookupByCode[fbAction.ActionType];
                        var actionsOfType = existingActions.Where(x => x.ActionTypeId == actionTypeId); // should be one at most
                        if (!actionsOfType.Any())
                        { // Create new
                            var stratAction = new StrategyAction
                            {
                                Date = date,
                                StrategyId = strategyId,
                                ActionTypeId = actionTypeId,
                                PostClick = fbAction.Num_click ?? 0,
                                PostView = fbAction.Num_view ?? 0
                            };
                            db.StrategyActions.Add(stratAction);
                            addedCount++;
                        }
                        else foreach (var stratAction in actionsOfType) // should be just one, but just in case
                        { // Update
                            stratAction.PostClick = fbAction.Num_click ?? 0;
                            stratAction.PostView = fbAction.Num_view ?? 0;
                            updatedCount++;
                        }
                    }
                    db.SaveChanges();
                } // loop through items
                Logger.Info("Saved StrategyActions ({0} updates, {1} additions, {2} deletions)", updatedCount, addedCount, deletedCount);
            } // using db
        }

    }
}
