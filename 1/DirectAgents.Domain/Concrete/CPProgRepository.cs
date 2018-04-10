using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities;
using DirectAgents.Domain.Entities.CPProg;

namespace DirectAgents.Domain.Concrete
{
    public partial class CPProgRepository : ICPProgRepository, IDisposable
    {
        private ClientPortalProgContext context;

        public CPProgRepository(ClientPortalProgContext context)
        {
            this.context = context;
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        #region DBO

        public Employee Employee(int id)
        {
            return context.Employees.Find(id);
        }
        public IQueryable<Employee> Employees()
        {
            return context.Employees;
        }

        public bool AddEmployee(Employee emp)
        {
            if (context.Employees.Any(e => e.Id == emp.Id))
                return false;
            context.Employees.Add(emp);
            context.SaveChanges();
            return true;
        }
        public bool SaveEmployee(Employee emp)
        {
            if (context.Employees.Any(e => e.Id == emp.Id))
            {
                var entry = context.Entry(emp);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }

        #endregion
        #region TD

        // --- Platforms/Advertisers ---

        public Platform Platform(int id)
        {
            return context.Platforms.Find(id);
        }
        public Platform Platform(string platformCode)
        {
            return context.Platforms.Where(p => p.Code == platformCode).FirstOrDefault();
        }
        public IQueryable<Platform> Platforms()
        {
            return context.Platforms;
        }

        public IQueryable<Platform> PlatformsWithoutBudgetInfo(int campId, DateTime date)
        {
            var platformIds = PlatformBudgetInfos(campId: campId, date: date).Select(pbi => pbi.PlatformId).ToArray();
            return context.Platforms.Where(p => !platformIds.Contains(p.Id));
        }

        public bool AddPlatform(Platform platform)
        {
            if (context.Platforms.Any(p => p.Id == platform.Id))
                return false;
            context.Platforms.Add(platform);
            context.SaveChanges();
            return true;
        }
        public bool SavePlatform(Platform platform, bool includeTokens = true)
        {
            if (context.Platforms.Any(p => p.Id == platform.Id))
            {
                var entry = context.Entry(platform);
                entry.State = EntityState.Modified;
                if (!includeTokens)
                    entry.Property(x => x.Tokens).IsModified = false;

                context.SaveChanges();
                return true;
            }
            return false;
        }

        public Advertiser Advertiser(int id)
        {
            return context.Advertisers.Find(id);
        }
        public IQueryable<Advertiser> Advertisers(bool includePlatforms = false)
        {
            if (includePlatforms)
                return context.Advertisers.Include("Campaigns.PlatformBudgetInfos.Platform");
            else
                return context.Advertisers;
        }

        public bool AddAdvertiser(Advertiser adv)
        {
            if (context.Advertisers.Any(a => a.Id == adv.Id))
                return false;
            context.Advertisers.Add(adv);
            context.SaveChanges();
            return true;
        }
        public bool SaveAdvertiser(Advertiser adv, bool includeLogo = false)
        {
            if (context.Advertisers.Any(a => a.Id == adv.Id))
            {
                if (!includeLogo)
                { // Keep the logo the same as it is in the db
                    adv.Logo = context.Advertisers.AsNoTracking().FirstOrDefault(a => a.Id == adv.Id).Logo;
                }
                var entry = context.Entry(adv);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }

        // --- Campaigns/Budgets ---

        public Campaign Campaign(int id)
        {
            return context.Campaigns.Find(id);
        }
        public IQueryable<Campaign> Campaigns(int? advId = null)
        {
            var campaigns = context.Campaigns.AsQueryable();
            if (advId.HasValue)
                campaigns = campaigns.Where(c => c.AdvertiserId == advId.Value);
            return campaigns;
        }
        public IQueryable<Campaign> CampaignsActive(DateTime? monthStart = null)
        {
            // A campaign is considered "active" if at least one of its accounts has DailySummaries for the specified month
            // TODO?: check Strat/Creat/Site Sums and Convs?
            DateTime? monthEnd = monthStart.HasValue ? monthStart.Value.AddMonths(1).AddDays(-1) : (DateTime?)null;
            var daySums = DailySummaries(monthStart, monthEnd);
            var activeAcctIds = daySums.Select(s => s.AccountId).Distinct().ToArray();
            var activeCampIds = context.ExtAccounts.Where(a => activeAcctIds.Contains(a.Id) && a.CampaignId.HasValue).Select(a => a.CampaignId.Value).Distinct().ToArray();
            var campaigns = context.Campaigns.Where(c => activeCampIds.Contains(c.Id));
            return campaigns;
        }

        public bool AddCampaign(Campaign camp)
        {
            if (context.Campaigns.Any(c => c.Id == camp.Id))
                return false;
            context.Campaigns.Add(camp);
            context.SaveChanges();
            return true;
        }
        public bool DeleteCampaign(int id)
        {
            var campaign = context.Campaigns.Find(id);
            if (campaign == null)
                return false;
            context.Campaigns.Remove(campaign);
            context.SaveChanges();
            return true;
        }
        public bool SaveCampaign(Campaign camp)
        {
            if (context.Campaigns.Any(c => c.Id == camp.Id))
            {
                var entry = context.Entry(camp);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        public void FillExtended(Campaign camp)
        {
            if (camp.Advertiser == null)
                camp.Advertiser = context.Advertisers.Find(camp.AdvertiserId);
            if (camp.ExtAccounts == null)
                camp.ExtAccounts = ExtAccounts(campId: camp.Id).ToList();
            if (camp.BudgetInfos == null)
                camp.BudgetInfos = BudgetInfos(campId: camp.Id).ToList();
            if (camp.PlatformBudgetInfos == null)
                camp.PlatformBudgetInfos = PlatformBudgetInfos(campId: camp.Id).ToList();
        }

        public bool AddExtAccountToCampaign(int campId, int acctId)
        {
            var campaign = context.Campaigns.Find(campId);
            var extAcct = context.ExtAccounts.Find(acctId);
            if (campaign == null || extAcct == null)
                return false;
            campaign.ExtAccounts.Add(extAcct);
            context.SaveChanges();
            return true;
        }
        public bool RemoveExtAccountFromCampaign(int campId, int acctId)
        {
            var campaign = context.Campaigns.Find(campId);
            if (campaign == null)
                return false;
            var extAcct = campaign.ExtAccounts.Where(a => a.Id == acctId).FirstOrDefault();
            if (extAcct == null)
                return false;

            campaign.ExtAccounts.Remove(extAcct);
            context.SaveChanges();
            return true;
        }

        // monthToCreate is any day in the desired month
        //public void CreateBudgetIfNotExists(Campaign campaign, DateTime monthToCreate)
        //{
        //    // Note: If you create a new Campaign, you should set Budgets to a new Collection before calling this method
        //    var firstOfMonth = new DateTime(monthToCreate.Year, monthToCreate.Month, 1);
        //    if (!campaign.BudgetInfos.Where(b => b.Date == firstOfMonth).Any())
        //    {
        //        var budgetInfo = new BudgetInfo { Date = firstOfMonth };
        //        budgetInfo.SetFrom(campaign.DefaultBudgetInfo);
        //        campaign.BudgetInfos.Add(budgetInfo);
        //        context.SaveChanges();
        //    }
        //}

        public BudgetInfo BudgetInfo(int campId, DateTime date)
        {
            return context.BudgetInfos.Find(campId, date);
        }
        public IQueryable<BudgetInfo> BudgetInfos(int? campId = null, DateTime? date = null)
        {
            var budgetInfos = context.BudgetInfos.AsQueryable();
            if (campId.HasValue)
                budgetInfos = budgetInfos.Where(b => b.CampaignId == campId.Value);
            if (date.HasValue)
                budgetInfos = budgetInfos.Where(b => b.Date == date.Value);
            return budgetInfos;
        }
        public bool AddBudgetInfo(BudgetInfo bi, bool saveChanges = true)
        {
            if (context.BudgetInfos.Any(b => b.CampaignId == bi.CampaignId && b.Date == bi.Date))
                return false;
            context.BudgetInfos.Add(bi);
            if (saveChanges)
                context.SaveChanges();
            return true;
        }
        public bool DeleteBudgetInfo(int campId, DateTime date)
        {
            var bi = context.BudgetInfos.Find(campId, date);
            if (bi == null)
                return false;
            context.BudgetInfos.Remove(bi);
            context.SaveChanges();
            return true;
        }
        public bool SaveBudgetInfo(BudgetInfo bi)
        {
            if (context.BudgetInfos.Any(b => b.CampaignId == bi.CampaignId && b.Date == bi.Date))
            {
                var entry = context.Entry(bi);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        public void FillExtended(BudgetInfo bi)
        {
            if (bi.Campaign == null)
                bi.Campaign = context.Campaigns.Find(bi.CampaignId);
        }

        public PlatformBudgetInfo PlatformBudgetInfo(int campId, int platformId, DateTime date)
        {
            return context.PlatformBudgetInfos.Find(campId, platformId, date);
        }
        public IQueryable<PlatformBudgetInfo> PlatformBudgetInfos(int? campId = null, int? platformId = null, DateTime? date = null)
        {
            var infos = context.PlatformBudgetInfos.AsQueryable();
            if (campId.HasValue)
                infos = infos.Where(i => i.CampaignId == campId.Value);
            if (platformId.HasValue)
                infos = infos.Where(i => i.PlatformId == platformId.Value);
            if (date.HasValue)
                infos = infos.Where(i => i.Date == date.Value);
            return infos;
        }
        public bool AddPlatformBudgetInfo(PlatformBudgetInfo pbi, bool saveChanges = true)
        {
            if (context.PlatformBudgetInfos.Any(i => i.CampaignId == pbi.CampaignId && i.PlatformId == pbi.PlatformId && i.Date == pbi.Date))
                return false;
            context.PlatformBudgetInfos.Add(pbi);
            if (saveChanges)
                context.SaveChanges();
            return true;
        }
        public bool DeletePlatformBudgetInfo(int campId, int platformId, DateTime date)
        {
            var pbi = context.PlatformBudgetInfos.Find(campId, platformId, date);
            if (pbi == null)
                return false;
            context.PlatformBudgetInfos.Remove(pbi);
            context.SaveChanges();
            return true;
        }
        public bool SavePlatformBudgetInfo(PlatformBudgetInfo pbi)
        {
            if (context.PlatformBudgetInfos.Any(i => i.CampaignId == pbi.CampaignId && i.PlatformId == pbi.PlatformId && i.Date == pbi.Date))
            {
                var entry = context.Entry(pbi);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        public void FillExtended(PlatformBudgetInfo pbi)
        {
            if (pbi.Campaign == null)
                pbi.Campaign = Campaign(pbi.CampaignId);
            if (pbi.Platform == null)
                pbi.Platform = Platform(pbi.PlatformId);
        }

        public PlatColMapping PlatColMapping(int id)
        {
            return context.PlatColMappings.Find(id);
        }
        public bool AddSavePlatColMapping(PlatColMapping platColMapping)
        {
            if (context.PlatColMappings.Any(p => p.Id == platColMapping.Id))
            {
                var entry = context.Entry(platColMapping);
                entry.State = EntityState.Modified;
            }
            else
            {
                if (!context.Platforms.Any(p => p.Id == platColMapping.Id))
                    return false; // no platform with that id - don't save mapping

                context.PlatColMappings.Add(platColMapping);
            }
            context.SaveChanges();
            return true;
        }
        public void FillExtended(PlatColMapping platColMapping)
        {
            if (platColMapping.Platform == null)
                platColMapping.Platform = Platform(platColMapping.Id);
        }

        public void CreateBaseFees(DateTime date, int platformIdForExtraItems)
        {
            var existingFeeItems = context.ExtraItems.Where(x => x.Date == date && x.PlatformId == platformIdForExtraItems && x.Cost == 0)
                                    .ToList();
            var campaigns = context.Campaigns.Where(c => (!c.Advertiser.StartDate.HasValue || c.Advertiser.StartDate.Value <= date) &&
                                                         (!c.Advertiser.EndDate.HasValue || c.Advertiser.EndDate.Value >= date));
            foreach (var camp in campaigns)
            {
                if (camp.BaseFee > 0 && !existingFeeItems.Any(x => x.CampaignId == camp.Id && x.Revenue == camp.BaseFee))
                {
                    var item = new ExtraItem
                    {
                        Date = date,
                        CampaignId = camp.Id,
                        PlatformId = platformIdForExtraItems,
                        Revenue = camp.BaseFee
                        // Description =
                    };
                    context.ExtraItems.Add(item);
                }
            }
            context.SaveChanges();
        }

        // REVIEW THIS !!!
        public void CopyBudgetInfos(int campId, DateTime toDate, DateTime? fromDate)
        {
            if (!fromDate.HasValue)
                fromDate = toDate.AddMonths(-1);

            var budgetInfos = context.BudgetInfos.Where(x => x.Date == fromDate.Value && x.CampaignId == campId);
            foreach (var budgetInfo in budgetInfos)
            {
                var bi = new BudgetInfo(campId, toDate, valuesToSet: budgetInfo);
                context.BudgetInfos.Add(bi);
            }
            var platformBudgetInfos = context.PlatformBudgetInfos.Where(x => x.Date == fromDate.Value && x.CampaignId == campId);
            foreach (var platformBudgetInfo in platformBudgetInfos)
            {
                var pbi = new PlatformBudgetInfo(campId, platformBudgetInfo.PlatformId, toDate, valuesToSet: platformBudgetInfo);
                context.PlatformBudgetInfos.Add(pbi);
            }
            context.SaveChanges();
        }

        // ---

        public IQueryable<Network> Networks()
        {
            return context.Networks;
        }

        public ExtAccount ExtAccount(int id)
        {
            return context.ExtAccounts.Find(id);
        }
        public IQueryable<ExtAccount> ExtAccounts(string platformCode = null, int? campId = null)
        {
            var extAccounts = context.ExtAccounts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(platformCode))
                extAccounts = extAccounts.Where(a => a.Platform.Code == platformCode);
            if (campId.HasValue)
                extAccounts = extAccounts.Where(a => a.CampaignId == campId.Value);
            return extAccounts;
        }

        public IQueryable<ExtAccount> ExtAccountsNotInCampaign(int campId)
        {
            var extAccounts = context.ExtAccounts.AsQueryable();
            var campaign = context.Campaigns.Find(campId);
            if (campaign != null)
            {
                var campaignAcctIds = campaign.ExtAccounts.Select(a => a.Id).ToArray();
                extAccounts = extAccounts.Where(a => !campaignAcctIds.Contains(a.Id));
            }
            return extAccounts;
        }

        public IQueryable<int> ExtAccountIds_Active(DateTime? monthStart = null)
        {
            DateTime? monthEnd = monthStart.HasValue ? monthStart.Value.AddMonths(1).AddDays(-1) : (DateTime?)null;
            var daySums = DailySummaries(monthStart, monthEnd);
            var activeAcctIds = daySums.Select(s => s.AccountId).Distinct();
            return activeAcctIds;
        }

        public IQueryable<ExtAccount> ExtAccounts_Social(int? advId = null, int? campId = null)
        {
            var socialCodes = DirectAgents.Domain.Entities.CPProg.Platform.Codes_Social(); //.ToArray();
            var extAccounts = context.ExtAccounts.Where(a => socialCodes.Contains(a.Platform.Code));
            if (advId.HasValue || campId.HasValue)
            {
                extAccounts = extAccounts.Where(a => a.CampaignId.HasValue);
                if (advId.HasValue)
                    extAccounts = extAccounts.Where(a => a.Campaign.AdvertiserId == advId.Value);
                if (campId.HasValue)
                    extAccounts = extAccounts.Where(a => a.CampaignId == campId.Value);
            }
            return extAccounts;
        }
        public IEnumerable<int> ExtAccountIds_Social(int? advId = null, int? campId = null)
        {
            var extAccounts = ExtAccounts_Social(advId: advId, campId: campId);
            return extAccounts.Select(a => a.Id).AsEnumerable();
        }

        public bool AddExtAccount(ExtAccount extAcct)
        {
            if (context.ExtAccounts.Any(ea => ea.Id == extAcct.Id))
                return false;
            context.ExtAccounts.Add(extAcct);
            context.SaveChanges();
            return true;
        }
        public bool SaveExtAccount(ExtAccount extAcct)
        {
            if (context.ExtAccounts.Any(ea => ea.Id == extAcct.Id))
            {
                var entry = context.Entry(extAcct);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        public void FillExtended(ExtAccount extAcct)
        {
            if (extAcct.Platform == null)
                extAcct.Platform = Platform(extAcct.PlatformId);
        }

        public IQueryable<Strategy> Strategies(int? acctId)
        {
            var strategies = context.Strategies.AsQueryable();
            if (acctId.HasValue)
                strategies = strategies.Where(s => s.AccountId == acctId.Value);
            return strategies;
        }

        public IQueryable<AdSet> AdSets(int? acctId)
        {
            var adsets = context.AdSets.AsQueryable();
            if (acctId.HasValue)
                adsets = adsets.Where(x => x.AccountId == acctId.Value);
            return adsets;
        }

        public TDad TDad(int id)
        {
            return context.TDads.Find(id);
        }
        public IQueryable<TDad> TDads(int? acctId)
        {
            var ads = context.TDads.AsQueryable();
            if (acctId.HasValue)
                ads = ads.Where(a => a.AccountId == acctId.Value);
            return ads;
        }
        public bool SaveTDad(TDad tDad)
        {
            if (context.TDads.Any(ad => ad.Id == tDad.Id))
            {
                var entry = context.Entry(tDad);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        public void FillExtended(TDad tDad)
        {
            if (tDad.ExtAccount == null)
                tDad.ExtAccount = ExtAccount(tDad.AccountId);
        }

        public IQueryable<ActionType> ActionTypes()
        {
            return context.ActionTypes;
        }

        #endregion

        // ---

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                    context.Dispose();
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
