using System;
using System.Collections.Generic;
using System.Linq;
using EomTool.Domain.DTOs;
using EomTool.Domain.Entities;

namespace EomTool.Domain.Abstract
{
    public interface IMainRepository : IDisposable
    {
        void SaveChanges();
        void EnableLogging();

        Advertiser GetAdvertiser(int id);
        IQueryable<Advertiser> Advertisers(bool withActivity = false);
        AccountManager GetAccountManager(int id);
        IQueryable<AccountManager> AccountManagers(bool withActivityOnly = false);

        Person GetPerson(int id);
        IQueryable<Person> People();
        bool SavePerson(Person inPerson);
        Person NewPerson(string first_name = "zFirst", string last_name = "zLast");

        IQueryable<Person> AvailableAnalystPeople(int pid, int affid);
        IQueryable<AnalystRole> AnalystRoles(int? personId = null, int? pid = null, int? affid = null);
        void AddAnalystRole(AnalystRole analystRole);
        bool DeleteAnalystRole(int pid, int affid, int personId);

        Analyst GetAnalyst(int id);
        IQueryable<Analyst> Analysts();
        bool SaveAnalyst(Analyst analyst);
        Analyst NewAnalyst(string name = "zNew");

        AnalystManager GetAnalystManager(int id);
        IQueryable<AnalystManager> AnalystManagers();
        bool SaveAnalystManager(AnalystManager anMgr);
        AnalystManager NewAnalystManager(string name = "zNew");

        Strategist GetStrategist(int id);
        IQueryable<Strategist> Strategists();
        bool SaveStrategist(Strategist strategist);
        Strategist NewStrategist(string name = "zNew");

        CampAff GetCampAff(int pid, int affid);
        IQueryable<CampAff> CampAffs();
        void FillExtended(CampAff campAff);
        bool SaveCampAff(CampAff campAff);

        IQueryable<Campaign> Campaigns(int? amId = null, int? advertiserId = null, int? affId = null, bool activeOnly = false);
        IEnumerable<CampaignAmount> CampaignAmounts(int pid, int? campaignStatus);
        IEnumerable<CampaignAmount> CampaignAmounts(int? amId, int? advertiserId, bool byAffiliate, int? campaignStatus);

        IEnumerable<CampaignAmount> CampaignAmounts2(int? campaignStatus);
        IEnumerable<CampAffItem> CampAffItems(bool includeNotes, int? campaignStatus = null, int? unitType = null, int? source = null);

        IEnumerable<EOMStat> EOMStatsByAdvertiser(int? unitType);

        Invoice GenerateInvoice(IEnumerable<CampAffId> campAffIds);
        void SaveInvoice(Invoice invoice, bool markSentToAccounting = false);
        IQueryable<Invoice> Invoices(bool fillExtended);
        Invoice GetInvoice(int id, bool fillLineItems = false);
        bool SetInvoiceStatus(int id, int statusId);

        // ---
        IQueryable<MarginApproval> MarginApprovals(bool fillExtended);
        void SaveMarginApproval(MarginApproval marginApproval);
        // ---

        void ChangeUnitType(IEnumerable<int> itemIds, int unitTypeId);

        IncomeType GetIncomeType(int id);
        IQueryable<IncomeType> IncomeTypes();
        bool SaveIncomeType(IncomeType inIncomeType);
        IncomeType NewIncomeType(string name = "zNew");
        bool NewIncomeType(IncomeType incomeType);

        List<UnitType> UnitTypeList { get; }
        string UnitTypeName(int unitTypeId);
        string ItemCode(int unitTypeId);
        bool UnitTypeExists(int unitTypeId);
        UnitType GetUnitType(int unitTypeId);
        UnitType GetUnitType(string unitTypeName);
        IQueryable<UnitType> UnitTypes();
        bool SaveUnitType(UnitType inUnitType);
        UnitType NewUnitType(string name = "zNew");

        List<Currency> CurrencyList { get; }
        bool CurrencyExists(int currency);
        string CurrencyName(int currencyId);
        int CurrencyId(string currencyName);
        Currency GetCurrency(int currencyId);
        Currency GetCurrency(string currencyName);

        List<CampaignStatus> CampaignStatusList { get; }
        List<ItemAccountingStatus> AccountingStatusList { get; }

        // --- Extra Item Import stuff ---
        bool CampaignExists(int pid);
        Campaign GetCampaign(int pid);
        bool SaveCampaign(Campaign inCampaign);

        IQueryable<Affiliate> Affiliates(bool withActivity = false);
        bool AffiliateExists(int affId);
        Affiliate GetAffiliate(int affId);
        Affiliate GetAffiliateById(int id);
        string AffiliateName(int affId, bool withId = false);

        Source GetSource(int sourceId);
        Source GetSource(string sourceName);

        Item GetItem(int id, bool fillExtended = false);
        IQueryable<Item> GetItems(IEnumerable<int> ids);
        IQueryable<Item> GetItems(int? pid = null, int? affId = null, int? advertiserId = null);
        void AddItem(Item item);
        bool ItemExists(Item item);

        // --- Auditing stuff ---
        void RestoreX();
        IQueryable<AuditSummary> AuditSummaries();
        IQueryable<Audit> Audits(DateTime? date = null, string operation = null, string primaryKey = null, string sysUser = null);

        // --- Media Buyer Approval stuff ---
        IQueryable<PublisherPayout> PublisherPayouts { get; }
        IQueryable<PublisherSummary> PublisherSummaries { get; }
        IQueryable<PublisherPayout> PublisherPayoutsByMode(string mode, bool includeZero);
        IQueryable<PublisherSummary> PublisherSummariesByMode(string mode, bool includeZero);
        IQueryable<CampaignsPublisherReportDetail> CampaignPublisherReportDetails { get; }

        void Media_ApproveItems(int[] itemIds);
        void Media_HoldItems(int[] itemIds);
        void ApproveItemsByAffId(int affId, string note, string author);
        void ReleaseItemsByAffId(int affId, string note, string author);
        void HoldItemsByAffId(int affId, string note, string author);

        IQueryable<Affiliate> AffiliatesForMediaBuyers(string[] mediaBuyers);
    }
}
