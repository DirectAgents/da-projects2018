using System;
using System.Data.Entity;
using System.Linq;
using ClientPortal.Data.Contracts;
using ClientPortal.Data.Entities.TD;
using ClientPortal.Data.Entities.TD.AdRoll;
using ClientPortal.Data.Entities.TD.DBM;

namespace ClientPortal.Data.Services
{
    public partial class TDRepository : ITDRepository, IDisposable
    {
        private TDContext context;

        public TDRepository(TDContext context)
        {
            this.context = context;
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        // --- AdRollProfile ---

        public IQueryable<AdRollProfile> AdRollProfiles()
        {
            var profiles = context.AdRollProfiles;
            return profiles;
        }
        public int MaxAdRollProfileId()
        {
            int maxId = -1;
            if (context.AdRollProfiles.Any())
                maxId = context.AdRollProfiles.Max(arp => arp.Id);
            return maxId;
        }

        public AdRollProfile GetAdRollProfile(int id)
        {
            var arProfile = context.AdRollProfiles.Find(id);
            return arProfile;
        }
        public void SaveAdRollProfile(AdRollProfile arProfile)
        {
            if (context.AdRollProfiles.Any(arp => arp.Id == arProfile.Id))
            {
                var entry = context.Entry(arProfile);
                entry.State = EntityState.Modified;
            }
            else
            {
                context.AdRollProfiles.Add(arProfile);
            }
            context.SaveChanges();
        }

        // --- InsertionOrder ---

        public IQueryable<InsertionOrder> InsertionOrders()
        {
            var insertionOrders = context.InsertionOrders;
            return insertionOrders;
        }

        public InsertionOrder GetInsertionOrder(int insertionOrderID)
        {
            var insertionOrder = context.InsertionOrders.Find(insertionOrderID);
            return insertionOrder;
        }
        public void SaveInsertionOrder(InsertionOrder insertionOrder)
        {
            if (context.InsertionOrders.Any(io => io.InsertionOrderID == insertionOrder.InsertionOrderID))
            {
                var entry = context.Entry(insertionOrder);
                entry.State = EntityState.Modified;
            }
            else
            {
                context.InsertionOrders.Add(insertionOrder);
            }
            context.SaveChanges();
        }

        public bool CreateAccountForInsertionOrder(int insertionOrderID)
        {
            var insertionOrder = context.InsertionOrders.Find(insertionOrderID);
            if (insertionOrder == null)
                return false;

            var tdAccount = new TradingDeskAccount
            {
                TradingDeskAccountId = MaxTradingDeskAccountId() + 1
            };
            insertionOrder.TradingDeskAccount = tdAccount;
            context.SaveChanges();
            return true;
        }

        // --- TradingDeskAccount ---

        public IQueryable<TradingDeskAccount> TradingDeskAccounts()
        {
            var tdAccounts = context.TradingDeskAccounts;
            return tdAccounts;
        }
        public int MaxTradingDeskAccountId()
        {
            int maxId = -1;
            if (context.TradingDeskAccounts.Any())
                maxId = context.TradingDeskAccounts.Max(tda => tda.TradingDeskAccountId);
            return maxId;
        }

        public TradingDeskAccount GetTradingDeskAccount(int tradingDeskAccountId)
        {
            var tdAccount = context.TradingDeskAccounts.Find(tradingDeskAccountId);
            return tdAccount;
        }
        public bool SaveTradingDeskAccount(TradingDeskAccount tdAccount)
        {
            if (context.TradingDeskAccounts.Any(tda => tda.TradingDeskAccountId == tdAccount.TradingDeskAccountId))
            {
                var entry = context.Entry(tdAccount);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            else
                return false;
        }
        public bool CreateTradingDeskAccount(TradingDeskAccount tdAccount)
        {
            if (context.TradingDeskAccounts.Any(tda => tda.TradingDeskAccountId == tdAccount.TradingDeskAccountId))
                return false;

            context.TradingDeskAccounts.Add(tdAccount);
            context.SaveChanges();
            return true;
        }

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
