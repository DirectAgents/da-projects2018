using System;
using System.Collections.Generic;
using System.Linq;
using ClientPortal.Data.Contexts;
using ClientPortal.Data.Contracts;

namespace ClientPortal.Data.Services
{
    public partial class ClientPortalRepository : IClientPortalRepository, IDisposable
    {
        ClientPortalContext context;

        public ClientPortalRepository(ClientPortalContext clientPortalContext)
        {
            this.context = clientPortalContext;
            context.Database.CommandTimeout = 180;
        }

        public void SaveChanges()
        {
            this.context.SaveChanges();
        }

        // ------

        #region Offers, Campaigns
        public IQueryable<Offer> Offers(int? advertiserId, DateTime? start = null, DateTime? end = null)
        {
            var offers = context.Offers.Where(o => o.AdvertiserId == advertiserId);

            if (start.HasValue || end.HasValue) // filter on offers that have daily summary data within this timeframe
            {
                var dailySummaries = context.DailySummaries.AsQueryable();
                if (start.HasValue)
                    dailySummaries = dailySummaries.Where(ds => ds.Date >= start.Value);
                if (end.HasValue)
                    dailySummaries = dailySummaries.Where(ds => ds.Date <= end.Value);

                var offerIdsWithData = dailySummaries.Select(ds => ds.OfferId).Distinct();

                offers = from o in offers
                         join od in offerIdsWithData
                         on o.OfferId equals od
                         select o;
            }

            return offers;
        }

        // includes Advertisers
        // pass in advertiserId==null for all advertisers
        public IQueryable<Offer> Offers(int? accountManagerId, int? advertiserId, bool cpmOnly, int? minCampaigns = null)
        {
            var offers = context.Offers.AsQueryable();
            if (advertiserId.HasValue)
                offers = offers.Where(o => o.AdvertiserId == advertiserId.Value);
            if (cpmOnly)
                offers = offers.Where(o => o.OfferName.Contains("CPM") || o.CPMReports.Any() );
            if (minCampaigns.HasValue && minCampaigns > 0)
                offers = offers.Where(o => o.Campaigns.Count >= minCampaigns);

            var q = from o in offers
                    join a in context.Advertisers.AsQueryable()
                    on o.AdvertiserId equals a.AdvertiserId
                    select new { Offer = o, AdvertiserId = a.AdvertiserId, AdvertiserName = a.AdvertiserName, AccountManager = a.AccountManager };

            var offersDetached = from row in q.AsEnumerable()
                                 select row.Offer.ThisWithAdvertiserInfo(row.AdvertiserId, row.AdvertiserName, row.AccountManager);

            if (accountManagerId.HasValue)
                offersDetached = offersDetached.Where(o => o.Advertiser.AccountManagerId == accountManagerId.Value);

            return offersDetached.AsQueryable();
        }

        // includes Advertiser
        public Offer GetOffer(int id)
        {
            var offer = context.Offers.Find(id);
            if (offer != null && offer.AdvertiserId.HasValue)
            {
                offer.Advertiser = context.Advertisers.Find(offer.AdvertiserId.Value);
            }
            return offer;
        }

        // use offerId==null for all campaigns
        public IQueryable<Campaign> Campaigns(int? offerId, bool cpmOnly)
        {
            var campaigns = context.Campaigns.Include("Affiliate").AsQueryable();

            if (offerId.HasValue)
                campaigns = campaigns.Where(c => c.OfferId == offerId.Value);
            if (cpmOnly)
                campaigns = campaigns.Where(c => c.Offer.OfferName.Contains("CPM"));

            return campaigns;
        }

        public Campaign GetCampaign(int id)
        {
            var campaign = context.Campaigns.Find(id);
            return campaign;
        }
        #endregion

        #region Creatives, CreativeFiles
        public IQueryable<Creative> Creatives(int? offerId)
        {
            var creatives = context.Creatives.Include("CreativeType").Include("CreativeFiles").AsQueryable();

            if (offerId.HasValue)
                creatives = creatives.Where(c => c.OfferId == offerId.Value);

            return creatives;
        }

        public Creative GetCreative(int id)
        {
            var creative = context.Creatives.Find(id);
            return creative;
        }

        public bool SaveCreative(Creative inCreative, bool saveChanges = false)
        {
            bool success = false;
            var creative = context.Creatives.Find(inCreative.CreativeId);
            if (creative != null)
            {
                creative.CreativeName = inCreative.CreativeName;
                if (saveChanges) SaveChanges();
                success = true;
            }
            return success;
        }

        public void FillExtended_Creative(Creative inCreative)
        {
            if (inCreative.Offer == null)
            {
                var creative = context.Creatives.Find(inCreative.CreativeId);
                if (creative != null)
                {
                    inCreative.OfferId = creative.OfferId;
                    inCreative.Offer = creative.Offer;
                    inCreative.CreativeTypeId = creative.CreativeTypeId;
                    inCreative.CreativeType = creative.CreativeType;
                }
            }
        }

        public IQueryable<CreativeFile> CreativeFiles(int? creativeId)
        {
            var creativeFiles = context.CreativeFiles.AsQueryable();
            if (creativeId.HasValue)
                creativeFiles = creativeFiles.Where(cf => cf.CreativeId == creativeId.Value);
            return creativeFiles;
        }
        #endregion

        #region CPMReports, CampaignDrops, CreativeStats
        public IQueryable<CPMReport> CPMReports(int? offerId)
        {
            var cpmReports = context.CPMReports.Include("CampaignDrops").AsQueryable();
            if (offerId.HasValue)
                cpmReports = cpmReports.Where(c => c.OfferId == offerId.Value);
            return cpmReports;
        }

        public CPMReport GetCPMReport(int id, bool includeAdvertiser = false)
        {
            var report = context.CPMReports.Find(id);
            if (report != null && includeAdvertiser)
            {
                report.Offer.Advertiser = GetAdvertiser(report.Offer.AdvertiserId);
            }
            return report;
        }

        public void SaveCPMReport(CPMReport inReport, bool saveChanges = false)
        {
            var report = GetCPMReport(inReport.CPMReportId);
            if (report == null)
            {   // Add
                context.CPMReports.Add(inReport);
            }
            else
            {   // Edit
                report.SetFieldsFrom(inReport);
            }
            if (saveChanges) SaveChanges();
        }

        public bool AddDropToCPMReport(int cpmReportId, int campaignDropId, bool saveChanges = false)
        {
            bool success = false;
            var report = GetCPMReport(cpmReportId);
            var drop = GetCampaignDrop(campaignDropId);
            if (report != null && drop != null)
            {
                if (!report.CampaignDrops.Select(cd => cd.CampaignDropId).Contains(drop.CampaignDropId))
                {
                    report.CampaignDrops.Add(drop);
                    if (saveChanges) SaveChanges();
                    success = true;
                }
            }
            return success;
        }

        public bool RemoveDropFromCPMReport(int cpmReportId, int campaignDropId, bool saveChanges = false)
        {
            bool success = false;
            var report = GetCPMReport(cpmReportId);
            var drop = GetCampaignDrop(campaignDropId);
            if (report != null && drop != null)
            {
                if (report.CampaignDrops.Select(cd => cd.CampaignDropId).Contains(drop.CampaignDropId))
                {
                    report.CampaignDrops.Remove(drop);
                    if (saveChanges) SaveChanges();
                    success = true;
                }
            }
            return success;
        }

        public void FillExtended_CPMReport(CPMReport inReport)
        {
            if (inReport.Offer == null || inReport.CampaignDrops == null)
            {
                var report = GetCPMReport(inReport.CPMReportId);
                if (report != null)
                {
                    inReport.OfferId = report.OfferId;
                    inReport.Offer = report.Offer;
                    inReport.CampaignDrops = report.CampaignDrops;
                    inReport.Offer.Advertiser = GetAdvertiser(inReport.Offer.AdvertiserId);
                }
            }
        }

        // return Offer, or null if the report can't be deleted
        public Offer DeleteCPMReport(int reportId, bool saveChanges = false)
        {
            Offer offer = null;
            var report = context.CPMReports.Find(reportId);
            if (report != null && report.CampaignDrops.Count == 0)
            {
                offer = report.Offer;
                context.CPMReports.Remove(report);
                if (saveChanges) SaveChanges();
            }
            return offer;
        }

        //not used??
        public bool CopyCPMReportDrops(int id, bool saveChanges = false)
        {
            var report = GetCPMReport(id);
            if (report == null) return false;

            foreach (var drop in report.CampaignDrops.Where(cd => cd.CopyOf == null).ToList())
            {
                var dropCopy = new CampaignDrop
                {
                    CampaignId = drop.CampaignId,
                    CopyOf = drop.CampaignDropId
                };
                dropCopy.SetFieldsFrom(drop);

                foreach (var cStat in drop.CreativeStats)
                {
                    var cStatCopy = new CreativeStat
                    {
                        CreativeId = cStat.CreativeId,
                        Clicks = cStat.Clicks,
                        Leads = cStat.Leads
                    };
                    dropCopy.CreativeStats.Add(cStatCopy);
                }
                report.CampaignDrops.Remove(drop);
                report.CampaignDrops.Add(dropCopy);
            }
            if (saveChanges) SaveChanges();
            return true;
        }

        public IQueryable<CampaignDrop> CampaignDropsNotInReport(int reportId)
        {
            var report = GetCPMReport(reportId);
            if (report == null)
                return null;

            var dropIds_nonCopies = report.CampaignDrops.Where(cd => cd.CopyOf == null).Select(cd => cd.CampaignDropId);
            var dropIds_original = report.CampaignDrops.Where(cd => cd.CopyOf != null).Select(cd => cd.CampaignDropOriginal).Select(cd => cd.CampaignDropId);
            var dropIds = dropIds_nonCopies.Union(dropIds_original).ToList();

            // maybe... report.Offer.AllCampaignDrops()...?
            return CampaignDrops(report.OfferId, null).Where(cd => !dropIds.Contains(cd.CampaignDropId));
        }

        public IQueryable<CampaignDrop> CampaignDrops(int? offerId, int? campaignId, bool includeCopies = false)
        {
            var campaignDropsDbQuery = context.CampaignDrops.Include("Campaign").Include("CreativeStats.Creative").Include("CPMReports");
            if (includeCopies)
                campaignDropsDbQuery = campaignDropsDbQuery.Include("CampaignDropCopies.CPMReports");
            var campaignDrops = campaignDropsDbQuery.AsQueryable();

            // Just return "originals".  "Copies" are for archive purposes (e.g. in sent reports)
            campaignDrops = campaignDrops.Where(cd => cd.CopyOf == null);

            if (offerId.HasValue)
                campaignDrops = campaignDrops.Where(cd => cd.Campaign.OfferId == offerId.Value);
            if (campaignId.HasValue)
                campaignDrops = campaignDrops.Where(cd => cd.CampaignId == campaignId.Value);
            return campaignDrops;
        }

        public CampaignDrop GetCampaignDrop(int id)
        {
            var campaignDrop = context.CampaignDrops.Find(id);
            return campaignDrop;
        }

        public CampaignDrop AddCampaignDrop(int campaignId, DateTime date, bool saveChanges = false)
        {
            var drop = new CampaignDrop
            {
                CampaignId = campaignId,
                Date = date
            };

            if (AddCampaignDrop(drop, null, saveChanges))
                return drop;
            else
                return null;
        }


        // returns whether the drop was successfully created
        public bool AddCampaignDrop(CampaignDrop drop, int? creativeId, bool saveChanges = false)
        {
            var campaign = context.Campaigns.Find(drop.CampaignId);
            if (campaign == null)
                return false;

            if (creativeId.HasValue)
            {
                var creative = context.Creatives.Find(creativeId);
                if (creative == null)
                    return false;

                var creativeStat = new CreativeStat
                {
                    Creative = creative,
                    //CampaignDrop = drop
                };
                drop.CreativeStats.Add(creativeStat);
            }
            context.CampaignDrops.Add(drop);

            try
            {
                if (saveChanges)
                    SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        // save an existing drop
        public bool SaveCampaignDrop(CampaignDrop inDrop, bool saveChanges = false)
        {
            bool success = false;
            var drop = context.CampaignDrops.Find(inDrop.CampaignDropId);
            if (drop != null)
            {
                //Note: could we check if there are any changes first? (and if not, don't do anything)

                success = true;
                DuplicateDropIfNecessary(drop);

                // Now save any changes (in the original)
                drop.SetFieldsFrom(inDrop);

                foreach (var creativeStat in inDrop.CreativeStats)
                {
                    if (!SaveCreativeStat(creativeStat))
                        success = false;
                }

                if (saveChanges) SaveChanges();
            }
            return success;
        }

        public void FillExtended_CampaignDrop(CampaignDrop drop)
        {
            if (drop.Campaign == null)
            {
                var campaignDrop = context.CampaignDrops.Find(drop.CampaignDropId);
                if (campaignDrop != null)
                {
                    drop.CampaignId = campaignDrop.CampaignId;
                    drop.Campaign = campaignDrop.Campaign;
                }
            }
        }

        // return Campaign, or null if the campaignDrop doesn't exist
        public Campaign DeleteCampaignDrop(int dropId, bool saveChanges = false)
        {
            Campaign campaign = null;
            var drop = context.CampaignDrops.Find(dropId);
            if (drop != null)
            {
                campaign = drop.Campaign;
                foreach (var creativeStat in drop.CreativeStats.ToList())
                {
                    context.CreativeStats.Remove(creativeStat);
                }
                context.CampaignDrops.Remove(drop);
                if (saveChanges) SaveChanges();
            }
            return campaign;
        }

        public void DeleteCampaignDropCopy(int dropId)
        {
            var drop = context.CampaignDrops.Find(dropId);
            if (drop.CopyOf.HasValue)
            {
                var dropOrig = drop.CampaignDropOriginal;
                foreach (var report in drop.CPMReports)
                {
                    report.CampaignDrops.Remove(drop);
                    report.CampaignDrops.Add(dropOrig);
                }
                DeleteCampaignDrop(drop.CampaignDropId);
                SaveChanges();
            }
        }

        public bool DuplicateDropIfNecessary(CampaignDrop drop)
        {
            var sentReports = drop.CPMReports.Where(r => r.DateSent != null);
            if (!sentReports.Any())
                return false;

            // drop exists in sent report(s); need to make a copy for them
            var dropCopy = drop.Duplicate();

            foreach (var report in sentReports)
            {
                report.CampaignDrops.Remove(drop);
                report.CampaignDrops.Add(dropCopy);
            }
            return true;
        }

        // save an existing creativeStat; only called via editing a CampaignDrop
        private bool SaveCreativeStat(CreativeStat inStat)
        {
            bool success = false;
            var stat = context.CreativeStats.Find(inStat.CreativeStatId);
            if (stat != null)
            {
                stat.SetPropertiesFrom(inStat);
                success = true;
            }
            return success;
        }

        public bool AddCreativeStat(int campaignDropId, int creativeId, bool saveChanges = false)
        {
            var creativeStat = new CreativeStat
            {
                CampaignDropId = campaignDropId,
                CreativeId = creativeId
            };
            return AddCreativeStat(creativeStat, saveChanges);
            //TODO: ?do this by getting the creative, adding it to the stat, then adding the stat to the drop?
            //(issue w/ CampaignDropsWizard.AddCreative(): if call Show w/o redirect, creative displayname is blank)
        }

        public bool AddCreativeStat(CreativeStat inStat, bool saveChanges = false)
        {
            // First check if drop is in any sent reports
            var drop = GetCampaignDrop(inStat.CampaignDropId);
            if (drop == null) return false;

            DuplicateDropIfNecessary(drop);

            // Now add the creativeStat (to the original drop)
            context.CreativeStats.Add(inStat);

            if (saveChanges) SaveChanges();
            return true;
        }

        // return campaignDropId, or null if the creativeStat doesn't exist
        public int? DeleteCreativeStat(int statId, bool saveChanges = false)
        {
            var stat = context.CreativeStats.Find(statId);
            if (stat == null) return null;

            int? dropId = null;
            // check if drop is in any sent reports
            var drop = GetCampaignDrop(stat.CampaignDropId);
            if (drop != null)
            {
                dropId = stat.CampaignDropId;

                DuplicateDropIfNecessary(drop);

                // Now remove the original; the dropCopies will preserve a copy of the creativeStat
                context.CreativeStats.Remove(stat);
                if (saveChanges) SaveChanges();
            }
            return dropId;
        }

        public bool UpdateCreativeStatFromSummaries(int statId, bool saveChanges = false)
        {
            var creativeStat = context.CreativeStats.Find(statId);
            if (creativeStat == null)
                return false;

            var creativeSummaries = context.CreativeSummaries.Where(cs => cs.CreativeId == creativeStat.CreativeId);

            creativeStat.Clicks = creativeSummaries.Sum(cs => cs.Clicks);
            creativeStat.Leads = creativeSummaries.Sum(cs => cs.Conversions);

            if (saveChanges) SaveChanges();
            return true;
        }

        public void UpdateCreativeStatsFromSummaries(IEnumerable<CreativeStat> creativeStats, bool saveChanges = false)
        {
            // group such that all creativeStats in a group reference the same creative
            var csGroups = creativeStats.GroupBy(cs => cs.CreativeId);
            foreach (var csGroup in csGroups)
            {
                var creativeId = csGroup.Key;
                var creativeSummaries = context.CreativeSummaries.Where(cs => cs.CreativeId == creativeId);
                int clicks = creativeSummaries.Sum(cs => cs.Clicks);
                int leads = creativeSummaries.Sum(cs => cs.Conversions);
                foreach (var creativeStat in csGroup)
                {
                    creativeStat.Clicks = clicks;
                    creativeStat.Leads = leads;
                }
            }
            if (saveChanges) SaveChanges();
        }
        #endregion

        #region Advertisers & Contacts
        public IQueryable<Advertiser> Advertisers
        {
            get { return context.Advertisers; }
        }
        public IQueryable<Contact> Contacts
        {
            get { return context.Contacts; }
        }

        public void AddAdvertiser(Advertiser entity)
        {
            context.Advertisers.Add(entity);
        }
        public void AddContact(Contact entity)
        {
            context.Contacts.Add(entity);
        }

        private Advertiser GetAdvertiser(int? id)
        {
            if (!id.HasValue)
                return null;
            return GetAdvertiser(id.Value);
        }

        public Advertiser GetAdvertiser(int id)
        {
            var advertiser = context.Advertisers.Find(id);
            return advertiser;
        }

        public Contact GetContact(int id)
        {
            var contact = context.Contacts.Find(id);
            return contact;
        }
        public Contact GetContact(string search) // search by last name, for now
        {
            var contact = context.Contacts.Where(c => c.LastName == search).FirstOrDefault();
            return contact;
        }

        #endregion

        #region SimpleReports
        public IQueryable<SimpleReport> SimpleReports
        {
            get { return context.SimpleReports; }
        }

        public IQueryable<SimpleReport> SearchSimpleReports
        {
            get { return context.SimpleReports.Where(sr => sr.SearchProfile != null); }
        }

        public SimpleReport GetSimpleReport(int id)
        {
            var simpleReport = context.SimpleReports.Find(id);
            return simpleReport;
        }

        public bool InitializeSimpleReport(int id, bool saveChanges = false)
        {
            var simpleReport = GetSimpleReport(id);
            if (simpleReport == null)
                return false;

            simpleReport.InitializePeriodAndNextSend();
            if (saveChanges) SaveChanges();
            return true;
        }

        public void FillExtended_SimpleReport(SimpleReport report)
        {
            if (report.Advertiser == null && report.AdvertiserId.HasValue)
                report.Advertiser = GetAdvertiser(report.AdvertiserId.Value);
            if (report.SearchProfile == null && report.SearchProfileId.HasValue)
                report.SearchProfile = GetSearchProfile(report.SearchProfileId.Value);
        }
        #endregion

        #region Files
        public IQueryable<FileUpload> GetFileUploads(int? advertiserId)
        {   // Note: if advertiserId == null, return the FileUploads where advertiserId is null
            var fileUploads = context.FileUploads.Where(f => f.AdvertiserId == advertiserId);
            return fileUploads;
        }

        public FileUpload GetFileUpload(int id)
        {
            var fileUpload = context.FileUploads.Find(id);
            return fileUpload;
        }

        public void AddFileUpload(FileUpload fileUpload, bool saveChanges = false)
        {
            context.FileUploads.Add(fileUpload);
            if (saveChanges) SaveChanges();
        }

        public void DeleteFileUpload(FileUpload fileUpload, bool saveChanges = false)
        {
            context.FileUploads.Remove(fileUpload);
            if (saveChanges) SaveChanges();
        }
        #endregion

        #region Goals
        public IQueryable<Goal> Goals
        {
            get { return context.Goals; }
        }

        public IQueryable<Goal> GetGoals(int advertiserId)
        {
            var goals = context.Goals.Where(g => g.AdvertiserId == advertiserId);
            return goals;
        }

        public Goal GetGoal(int id)
        {
            var goal = context.Goals.Find(id);
            return goal;
        }

        public void AddGoal(Goal goal, bool saveChanges = false)
        {
            context.Goals.Add(goal);
            if (saveChanges) SaveChanges();
        }

        public bool DeleteGoal(int id, int? advertiserId)
        {
            bool deleted = false;
            var goal = context.Goals.Find(id);
            if (goal != null && (advertiserId == null || goal.AdvertiserId == advertiserId.Value))
            {
                context.Goals.Remove(goal);
                context.SaveChanges();
                deleted = true;
            }
            return deleted;
        }
        #endregion

        #region User methods
        public IQueryable<ClientInfo> ClientInfos()
        {
            var clientInfos = context.ClientInfos;
            return clientInfos;
        }
        public ClientInfo GetClientInfo(int id)
        {
            var clientInfo = context.ClientInfos.Find(id);
            return clientInfo;
        }
        public void AddClientInfo(ClientInfo clientInfo, bool saveChanges = false)
        {
            context.ClientInfos.Add(clientInfo);
            if (saveChanges) SaveChanges();
        }

        public IQueryable<UserProfile> UserProfiles()
        {
            var userProfiles = context.UserProfiles;
            return userProfiles;
        }

        public UserProfile GetUserProfile(int userId)
        {
            var userProfile = context.UserProfiles.Find(userId);
            return userProfile;
        }

        public UserProfile GetUserProfile(string userName)
        {
            var userProfile = context.UserProfiles.SingleOrDefault(up => up.UserName == userName);
            return userProfile;
        }

        public IQueryable<UserEvent> UserEvents
        {
            get { return context.UserEvents; }
        }

        public void AddUserEvent(UserEvent userEvent, bool saveChanges = false)
        {
            context.UserEvents.Add(userEvent);
            if (saveChanges) SaveChanges();
        }

        public void AddUserEvent(string userName, string eventString, bool saveChanges = false)
        {
            var userProfile = context.UserProfiles.SingleOrDefault(up => up.UserName == userName);
            if (userProfile != null)
                AddUserEvent_NoCheck(userProfile.UserId, eventString, saveChanges);
            else
                AddUserEvent_NoCheck(null, eventString + " [" + userName + "]", saveChanges);
        }

        public void AddUserEvent(int userId, string eventString, bool saveChanges = false)
        {
            var userProfile = context.UserProfiles.SingleOrDefault(up => up.UserId == userId);
            if (userProfile != null)
                AddUserEvent_NoCheck(userProfile.UserId, eventString, saveChanges);
            else
                AddUserEvent_NoCheck(null, eventString + "[id: " + userId + "]", saveChanges);
        }

        private void AddUserEvent_NoCheck(int? userId, string eventString, bool saveChanges)
        {
            var userEvent = new UserEvent
            {
                UserId = userId,
                Event = eventString
            };
            context.UserEvents.Add(userEvent);
            if (saveChanges) SaveChanges();
        }
        #endregion

        // --- Helpers ---
        public static int? ParseInt(string stringVal)
        {
            int intVal;
            if (Int32.TryParse(stringVal, out intVal))
                return intVal;
            else
                return null;
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
