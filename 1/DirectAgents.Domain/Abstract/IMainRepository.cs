using System;
using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.DTO;
using DirectAgents.Domain.Entities;
using DirectAgents.Domain.Entities.Cake;
using DirectAgents.Domain.Entities.Screen;

namespace DirectAgents.Domain.Abstract
{
    public interface IMainRepository : IDisposable
    {
        void SaveChanges();

        IQueryable<Salesperson> Salespeople();
        IQueryable<SalespersonStat> SalespersonStats(DateTime? minDate = null);
        IQueryable<SalespersonStat> SalespersonStats(int? salespersonId, DateTime? date);
        SalespersonStat GetSalespersonStat(int salespersonId, DateTime date);
        void SaveSalespersonStat(SalespersonStat stat);
        void DeleteSalespersonStats(DateTime date);

        IQueryable<Variable> GetVariables();
        Variable GetVariable(string name);
        void SaveVariable(Variable variable);

        // Cake
        IDepartmentRepository Create_Cake_DeptRepository();

        Contact GetContact(int contactId);
        IQueryable<Contact> GetAccountManagers();
        IQueryable<Advertiser> GetAdvertisers(int? acctMgrId = null, bool? withBudgetedOffers = null, int? ABClientId = null);
        Advertiser GetAdvertiser(int advertiserId);
        IQueryable<Offer> GetOffers(bool includeExtended = false, int? acctMgrId = null, int? advertiserId = null, bool? withBudget = null, bool includeInactive = true, bool? hidden = null);
        IQueryable<Offer> GetOffersUnion(bool includeExtendedInfo, bool excludeInactive, bool? withBudget, int[] offerIds);
        Offer GetOffer(int offerId, bool includeExtended, bool fillBudgetStats);
        IEnumerable<int> OfferIds(int? advId = null);

        void FillOfferBudgetStats(Offer offer);

        IEnumerable<CampaignSummary> TopOffers(int num, TopCampaignsBy by);

        IQueryable<OfferDailySummary> GetOfferDailySummaries(int? offerId, DateTime? startDate = null, DateTime? endDate = null);
        IQueryable<OfferDailySummary> GetOfferDailySummariesForBudget(int offerId);
        IQueryable<OfferDailySummary> GetOfferDailySummariesForBudget(Offer offer);

        StatsSummary GetStatsSummary(int? offerId, DateTime? startDate, DateTime? endDate);

        IQueryable<CampSum> GetCampSums(int? advertiserId = null, int? offerId = null, DateTime? monthStart = null);
    }
}
