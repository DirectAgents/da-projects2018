using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Domain.Concrete
{
    public class ABRepository : IABRepository, IDisposable
    {
        private ABContext context;

        public ABRepository(ABContext context)
        {
            this.context = context;
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        // --- Clients ---

        public ABClient Client(int id)
        {
            return context.ABClients.Find(id);
        }
        public IQueryable<ABClient> Clients()
        {
            return context.ABClients;
        }

        public bool AddClient(ABClient client)
        {
            if (context.ABClients.Any(c => c.Id == client.Id))
                return false;
            context.ABClients.Add(client);
            context.SaveChanges();
            return true;
        }
        public void AddClients(IEnumerable<ABClient> clients)
        {
            context.ABClients.AddRange(clients);
            context.SaveChanges();
        }

        public bool SaveClient(ABClient client)
        {
            if (context.ABClients.Any(c => c.Id == client.Id))
            {
                var entry = context.Entry(client);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }

        // --- Vendors ---

        public ABVendor Vendor(int id)
        {
            return context.ABVendors.Find(id);
        }
        public IQueryable<ABVendor> Vendors()
        {
            return context.ABVendors;
        }

        // ---

        public ClientBudget ClientBudget(int clientId, DateTime date)
        {
            return context.ClientBudgets.Find(clientId, date);
        }

        public ClientPayment ClientPayment(int id)
        {
            return context.ClientPayments.Find(id);
        }
        public bool SaveClientPayment(ClientPayment payment)
        {
            if (context.ClientPayments.Any(x => x.Id == payment.Id))
            {
                var entry = context.Entry(payment);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        public void FillExtended(ClientPayment payment)
        {
            if (payment.Client == null)
                payment.Client = context.ABClients.Find(payment.ClientId);
            if (payment.Bits == null)
                payment.Bits = ClientPaymentBits(paymentId: payment.Id).ToList();
        }

        public ClientPaymentBit ClientPaymentBit(int id)
        {
            return context.ClientPaymentBits.Find(id);
        }
        public IQueryable<ClientPaymentBit> ClientPaymentBits(int? paymentId = null)
        {
            var bits = context.ClientPaymentBits.AsQueryable();
            if (paymentId.HasValue)
                bits = bits.Where(x => x.ClientPaymentId == paymentId.Value);
            return bits;
        }
        public void DeleteClientPaymentBit(ClientPaymentBit bit)
        {
            context.ClientPaymentBits.Remove(bit);
            context.SaveChanges();
        }

        public Job Job(int id)
        {
            return context.Jobs.Find(id);
        }
        public IQueryable<Job> Jobs(int? clientId = null)
        {
            var jobs = context.Jobs.AsQueryable();
            if (clientId.HasValue)
                jobs = jobs.Where(j => j.ClientId == clientId.Value);
            return jobs;
        }
        public IQueryable<Job> ActiveJobs(int clientId, DateTime? startDate, DateTime? endDate)
        {
            var jobs = context.Jobs.Where(j => j.ClientId == clientId);
            if (!startDate.HasValue && !endDate.HasValue)
            {   // no daterange specified
                jobs = jobs.Where(j => j.ExtraItems.Any());
            }
            else
            {   // find the jobs that were active within the daterange
                var extraItems = jobs.SelectMany(j => j.ExtraItems);
                if (startDate.HasValue)
                    extraItems = extraItems.Where(x => x.Date >= startDate.Value);
                if (endDate.HasValue)
                    extraItems = extraItems.Where(x => x.Date <= endDate.Value);
                var jobIds = extraItems.Select(x => x.JobId).Distinct().ToArray();
                jobs = jobs.Where(j => jobIds.Contains(j.Id));
            }
            return jobs;
        }
        public void DeleteJob(Job job)
        {
            context.Jobs.Remove(job);
            context.SaveChanges();
        }

        public IQueryable<ABExtraItem> ExtraItems(DateTime? startDate, DateTime? endDate, int? jobId = null)
        {
            var extraItems = context.ABExtraItems.AsQueryable();
            if (startDate.HasValue)
                extraItems = extraItems.Where(x => x.Date >= startDate.Value);
            if (endDate.HasValue)
                extraItems = extraItems.Where(x => x.Date <= endDate.Value);
            if (jobId.HasValue)
                extraItems = extraItems.Where(x => x.JobId == jobId.Value);
            return extraItems;
        }

        public IQueryable<ClientAccount> ClientAccounts(int? clientId)
        {
            var clientAccounts = context.ClientAccounts.AsQueryable();
            if (clientId.HasValue)
                clientAccounts = clientAccounts.Where(x => x.ClientId == clientId.Value);
            return clientAccounts;
        }
        public ClientAccount ClientAccount(int id)
        {
            return context.ClientAccounts.Find(id);
        }
        public bool SaveClientAccount(ClientAccount clientAccount)
        {
            if (context.ClientAccounts.Any(ca => ca.Id == clientAccount.Id))
            {
                var entry = context.Entry(clientAccount);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public Campaign Campaign(int id)
        {
            return context.Campaigns.Find(id);
        }
        public IQueryable<Campaign> Campaigns(int? clientId = null)
        {
            var campaigns = context.Campaigns.AsQueryable();
            if (clientId.HasValue)
                campaigns = campaigns.Where(c => c.SpendBuckets.Select(b => b.ClientAccount.ClientId).Contains(clientId.Value)); // ?? best way ??
            return campaigns;
        }
        public IEnumerable<CampaignWrap> CampaignWraps(int? clientId = null, int? acctId = null)
        {
            var campaignWraps = CampaignBucketGroups(clientId, acctId).AsEnumerable().Select(x => new CampaignWrap(x));
            return campaignWraps;
        }
        private IQueryable<IGrouping<Campaign, SpendBucket>> CampaignBucketGroups(int? clientId = null, int? acctId = null)
        {
            var spendBuckets = SpendBuckets(clientId, acctId);
            return spendBuckets.GroupBy(sb => sb.Campaign);
        }

        public IQueryable<SpendBucket> SpendBuckets(int? clientId = null, int? acctId = null)
        {
            var spendBuckets = context.SpendBuckets.AsQueryable();
            if (clientId.HasValue)
                spendBuckets = spendBuckets.Where(x => x.ClientAccount.ClientId == clientId.Value);
            if (acctId.HasValue)
                spendBuckets = spendBuckets.Where(x => x.AcctId == acctId.Value);
            return spendBuckets;
        }

        // ---

        public IQueryable<ProtoPeriod> ProtoPeriods()
        {
            return context.ProtoPeriods;
        }

        public ProtoCampaign ProtoCampaign(int id)
        {
            return context.ProtoCampaigns.Find(id);
        }
        public IQueryable<ProtoCampaign> ProtoCampaigns(int? clientId = null, int? clientAccountId = null)
        {
            var campaigns = context.ProtoCampaigns.AsQueryable();
            if (clientId.HasValue)
                campaigns = campaigns.Where(c => c.ClientAccount.ClientId == clientId.Value);
            if (clientAccountId.HasValue)
                campaigns = campaigns.Where(c => c.ClientAccountId == clientAccountId.Value);
            return campaigns;
        }
        public bool SaveProtoCampaign(ProtoCampaign campaign)
        {
            if (context.ProtoCampaigns.Any(c => c.Id == campaign.Id))
            {
                var entry = context.Entry(campaign);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        public void FillExtended(ProtoCampaign campaign)
        {
            if (campaign.ClientAccount == null)
                campaign.ClientAccount = context.ClientAccounts.Find(campaign.ClientAccountId);
            //SpendBits?
        }

        public IQueryable<ProtoSpendBit> ProtoSpendBits(int? clientId = null, int? clientAccountId = null, int? campaignId = null, int? periodId = null)
        {
            var spendBits = context.ProtoSpendBits.AsQueryable();
            if (clientId.HasValue)
                spendBits = spendBits.Where(x => x.ProtoCampaign.ClientAccount.ClientId == clientId.Value);
            if (clientAccountId.HasValue)
                spendBits = spendBits.Where(x => x.ProtoCampaign.ClientAccountId == clientAccountId.Value);
            if (campaignId.HasValue)
                spendBits = spendBits.Where(x => x.ProtoCampaignId == campaignId.Value);
            if (periodId.HasValue)
                spendBits = spendBits.Where(x => x.ProtoPeriodId == periodId.Value);
            return spendBits;
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
