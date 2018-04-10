using System.Collections.Generic;
using System.Linq;
using CakeExtracter.Etl.TradingDesk.LoadersDA;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.CPProg;
using FacebookAPI;
using FacebookAPI.Entities;

namespace CakeExtracter.Etl.SocialMarketing.LoadersDA
{
    public class FacebookAdSetSummaryLoader : Loader<FBSummary>
    {
        private readonly bool LoadActions;
        private TDAdSetSummaryLoader adsetSummaryLoader;
        private Dictionary<string, int> actionTypeIdLookupByCode = new Dictionary<string, int>();

        public FacebookAdSetSummaryLoader(int accountId, bool loadActions = false)
        {
            this.BatchSize = FacebookUtility.RowsReturnedAtATime; //FB API only returns 25 rows at a time
            this.adsetSummaryLoader = new TDAdSetSummaryLoader(accountId);
            this.LoadActions = loadActions;
        }

        protected override int Load(List<FBSummary> items)
        {
            var dbItems = items.Select(i => CreateAdSetSummary(i)).ToList();
            adsetSummaryLoader.AddUpdateDependentStrategies(dbItems);
            adsetSummaryLoader.AddUpdateDependentAdSets(dbItems);
            adsetSummaryLoader.AssignAdSetIdToItems(dbItems);
            var count = adsetSummaryLoader.UpsertDailySummaries(dbItems);

            if (LoadActions)
            {
                AddUpdateDependentActionTypes(items);
                UpsertAdSetActions(items, dbItems);
            }
            return count;
        }

        public static AdSetSummary CreateAdSetSummary(FBSummary item)
        {
            var sum = new AdSetSummary
            {
                Date = item.Date,
                AdSetName = item.AdSetName,
                AdSetEid = item.AdSetId,
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
            AddUpdateDependentActionTypes(items, this.actionTypeIdLookupByCode);
        }
        public static void AddUpdateDependentActionTypes(List<FBSummary> items, Dictionary<string, int> actionTypeIdLookupByCode)
        {
            var actionTypeCodes = items.Where(i => i.Actions != null).SelectMany(i => i.Actions.Keys).Distinct();

            using (var db = new ClientPortalProgContext())
            {
                foreach (var actionTypeCode in actionTypeCodes)
                {
                    if (actionTypeIdLookupByCode.ContainsKey(actionTypeCode))
                        continue;
                    var actionTypesInDb = db.ActionTypes.Where(x => x.Code == actionTypeCode);
                    if (!actionTypesInDb.Any())
                    {
                        var actionType = new ActionType
                        {
                            Code = actionTypeCode
                        };
                        db.ActionTypes.Add(actionType);
                        db.SaveChanges();
                        Logger.Info("Saved new ActionType: {0}", actionTypeCode);
                        actionTypeIdLookupByCode[actionTypeCode] = actionType.Id;
                    }
                    else
                    {
                        var actionType = actionTypesInDb.First();
                        actionTypeIdLookupByCode[actionTypeCode] = actionType.Id;
                    }
                }
            }
        }

        //Note: get the actions from the items(FBSummaries); get the adsetId from the adsetSummaries
        private void UpsertAdSetActions(List<FBSummary> items, List<AdSetSummary> adsetSummaries)
        {
            int addedCount = 0;
            int updatedCount = 0;
            int deletedCount = 0;
            using (var db = new ClientPortalProgContext())
            { // The items and adsetSummaries have a 1-to-1 correspondence because of how the latter were instantiated above
                var itemEnumerator = items.GetEnumerator();
                var asEnumerator = adsetSummaries.GetEnumerator();
                while (itemEnumerator.MoveNext())
                {
                    asEnumerator.MoveNext();
                    if (itemEnumerator.Current.Actions == null)
                        continue;
                    var date = itemEnumerator.Current.Date;
                    var adsetId = asEnumerator.Current.AdSetId;
                    var fbActions = itemEnumerator.Current.Actions.Values;

                    var actionTypeIds = fbActions.Select(x => actionTypeIdLookupByCode[x.ActionType]).ToArray();
                    var existingActions = db.AdSetActions.Where(x => x.Date == date && x.AdSetId == adsetId);

                    //Delete actions that no longer have stats for the date/adset
                    foreach (var adsetAction in existingActions.Where(x => !actionTypeIds.Contains(x.ActionTypeId)))
                    {
                        db.AdSetActions.Remove(adsetAction);
                        deletedCount++;
                    }

                    //Add/update the rest
                    foreach (var fbAction in fbActions)
                    {
                        int actionTypeId = actionTypeIdLookupByCode[fbAction.ActionType];
                        var actionsOfType = existingActions.Where(x => x.ActionTypeId == actionTypeId); // should be one at most
                        if (!actionsOfType.Any())
                        { // Create new
                            var adsetAction = new AdSetAction
                            {
                                Date = date,
                                AdSetId = adsetId,
                                ActionTypeId = actionTypeId,
                                PostClick = fbAction.Num_click ?? 0,
                                PostView = fbAction.Num_view ?? 0,
                                PostClickVal = fbAction.Val_click ?? 0,
                                PostViewVal = fbAction.Val_view ?? 0
                            };
                            db.AdSetActions.Add(adsetAction);
                            addedCount++;
                        }
                        else foreach (var adsetAction in actionsOfType) // should be just one, but just in case
                            { // Update
                                adsetAction.PostClick = fbAction.Num_click ?? 0;
                                adsetAction.PostView = fbAction.Num_view ?? 0;
                                adsetAction.PostClickVal = fbAction.Val_click ?? 0;
                                adsetAction.PostViewVal = fbAction.Val_view ?? 0;
                                updatedCount++;
                            }
                    }
                    db.SaveChanges();
                } // loop through items
                Logger.Info("Saved AdSetActions ({0} updates, {1} additions, {2} deletions)", updatedCount, addedCount, deletedCount);
            } // using db
        }

    }
}
