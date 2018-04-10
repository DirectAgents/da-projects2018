using System;
using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.Entities.AB;

namespace DirectAgents.Domain.Abstract
{
    public interface IABRepository : IDisposable
    {
        void SaveChanges();

        ABClient Client(int id);
        IQueryable<ABClient> Clients();
        bool AddClient(ABClient client);
        void AddClients(IEnumerable<ABClient> clients);
        bool SaveClient(ABClient client);

        ABVendor Vendor(int id);
        IQueryable<ABVendor> Vendors();

        ClientBudget ClientBudget(int clientId, DateTime date);

        ClientPayment ClientPayment(int id);
        bool SaveClientPayment(ClientPayment payment);
        void FillExtended(ClientPayment payment);

        ClientPaymentBit ClientPaymentBit(int id);
        IQueryable<ClientPaymentBit> ClientPaymentBits(int? paymentId = null);
        void DeleteClientPaymentBit(ClientPaymentBit bit);

        Job Job(int id);
        IQueryable<Job> Jobs(int? clientId = null);
        IQueryable<Job> ActiveJobs(int clientId, DateTime? startDate, DateTime? endDate);
        void DeleteJob(Job job);

        IQueryable<ABExtraItem> ExtraItems(DateTime? startDate, DateTime? endDate, int? jobId = null);

        ClientAccount ClientAccount(int id);
        IQueryable<ClientAccount> ClientAccounts(int? clientId);
        bool SaveClientAccount(ClientAccount clientAccount);

        Campaign Campaign(int id);
        IQueryable<Campaign> Campaigns(int? clientId = null);
        IEnumerable<CampaignWrap> CampaignWraps(int? clientId = null, int? acctId = null);
        IQueryable<SpendBucket> SpendBuckets(int? clientId = null, int? acctId = null);

        IQueryable<ProtoPeriod> ProtoPeriods();
        ProtoCampaign ProtoCampaign(int id);
        IQueryable<ProtoCampaign> ProtoCampaigns(int? clientId = null, int? clientAccountId = null);
        bool SaveProtoCampaign(ProtoCampaign campaign);
        void FillExtended(ProtoCampaign campaign);
        IQueryable<ProtoSpendBit> ProtoSpendBits(int? clientId = null, int? clientAccountId = null, int? campaignId = null, int? periodId = null);
    }
}
