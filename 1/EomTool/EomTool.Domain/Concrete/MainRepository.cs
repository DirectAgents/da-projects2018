using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using EomTool.Domain.Abstract;
using EomTool.Domain.DTOs;
using EomTool.Domain.Entities;

namespace EomTool.Domain.Concrete
{
    public partial class MainRepository : IMainRepository, IDisposable
    {
        EomEntities context;

        public MainRepository(EomEntities context)
        {
            this.context = context;
        }

        public void SaveChanges()
        {
            context.SaveChanges();
        }

        public void EnableLogging()
        {
            context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }
        //---

        public Advertiser GetAdvertiser(int id)
        {
            return context.Advertisers.FirstOrDefault(a => a.id == id);
        }
        public IQueryable<Advertiser> Advertisers(bool withActivity = false)
        {
            if (withActivity)
                return Campaigns(activeOnly: true).Select(c => c.Advertiser).Distinct();
            else
                return context.Advertisers;
        }

        public AccountManager GetAccountManager(int id)
        {
            return context.AccountManagers.FirstOrDefault(am => am.id == id);
        }
        public IQueryable<AccountManager> AccountManagers(bool withActivityOnly = false)
        {
            if (withActivityOnly)
                return Campaigns(activeOnly: true).Select(c => c.AccountManager).Distinct();
            else
                return context.AccountManagers;
        }

        public Person GetPerson(int id)
        {
            return context.People.Find(id);
        }
        public IQueryable<Person> People()
        {
            return context.People;
        }
        public bool SavePerson(Person inPerson)
        {
            var person = GetPerson(inPerson.id);
            if (person == null)
                return false;

            person.first_name = inPerson.first_name;
            person.last_name = inPerson.last_name;
            SaveChanges();
            return true;
        }
        public Person NewPerson(string first_name = "zFirst", string last_name = "zLast")
        {
            int id = context.People.Select(p => p.id).DefaultIfEmpty(0).Max() + 1;
            var person = new Person
            {
                id = id,
                first_name = first_name,
                last_name = last_name
            };
            context.People.Add(person);
            SaveChanges();
            return person;
        }

        // Returns: the people who are not Analysts for the pid/affid combination
        public IQueryable<Person> AvailableAnalystPeople(int pid, int affid)
        {
            var aRoles = AnalystRoles(pid: pid, affid: affid);
            var analystPersonIds = aRoles.Select(x => x.person_id).ToArray();
            var availablePeople = context.People.Where(x => !analystPersonIds.Contains(x.id));
            return availablePeople;
        }

        public IQueryable<AnalystRole> AnalystRoles(int? personId = null, int? pid = null, int? affid = null)
        {
            var aRoles = context.AnalystRoles.AsQueryable();
            if (personId.HasValue)
                aRoles = aRoles.Where(x => x.person_id == personId.Value);
            if (pid.HasValue)
                aRoles = aRoles.Where(x => x.pid == pid.Value);
            if (affid.HasValue)
                aRoles = aRoles.Where(x => x.affid == affid.Value);
            return aRoles;
        }
        public void AddAnalystRole(AnalystRole analystRole)
        {
            context.AnalystRoles.Add(analystRole);
            SaveChanges();
        }
        public bool DeleteAnalystRole(int pid, int affid, int personId)
        {
            var aRole = context.AnalystRoles.SingleOrDefault(x => x.pid == pid && x.affid == affid && x.person_id == personId);
            if (aRole == null)
                return false;

            context.AnalystRoles.Remove(aRole);
            SaveChanges();
            return true;
        }

        // ---

        public Analyst GetAnalyst(int id)
        {
            return context.Analysts.Find(id);
        }
        public IQueryable<Analyst> Analysts()
        {
            return context.Analysts;
        }
        public bool SaveAnalyst(Analyst analyst)
        {
            if (context.Analysts.Any(x => x.id == analyst.id))
            {
                var entry = context.Entry(analyst);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        public Analyst NewAnalyst(string name = "zNew") //comm_rate?
        {
            var analyst = new Analyst
            {
                name = name
            };
            context.Analysts.Add(analyst);
            SaveChanges();
            return analyst;
        }

        public AnalystManager GetAnalystManager(int id)
        {
            return context.AnalystManagers.Find(id);
        }
        public IQueryable<AnalystManager> AnalystManagers()
        {
            return context.AnalystManagers;
        }
        public bool SaveAnalystManager(AnalystManager anMgr)
        {
            if (context.AnalystManagers.Any(x => x.id == anMgr.id))
            {
                var entry = context.Entry(anMgr);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        public AnalystManager NewAnalystManager(string name = "zNew")
        {
            var anMgr = new AnalystManager
            {
                name = name
            };
            context.AnalystManagers.Add(anMgr);
            SaveChanges();
            return anMgr;
        }

        public Strategist GetStrategist(int id)
        {
            return context.Strategists.Find(id);
        }
        public IQueryable<Strategist> Strategists()
        {
            return context.Strategists;
        }
        public bool SaveStrategist(Strategist strategist)
        {
            if (context.Strategists.Any(x => x.id == strategist.id))
            {
                var entry = context.Entry(strategist);
                entry.State = EntityState.Modified;
                context.SaveChanges();
                return true;
            }
            return false;
        }
        public Strategist NewStrategist(string name = "zNew")
        {
            var strategist = new Strategist
            {
                name = name
            };
            context.Strategists.Add(strategist);
            SaveChanges();
            return strategist;
        }

        public CampAff GetCampAff(int pid, int affid)
        {
            return context.CampAffs.Find(pid, affid);
        }
        public IQueryable<CampAff> CampAffs()
        {
            return context.CampAffs;
        }
        public void FillExtended(CampAff campAff)
        {
            if (campAff.Campaign == null)
                campAff.Campaign = GetCampaign(campAff.pid);
            if (campAff.Affiliate == null)
                campAff.Affiliate = GetAffiliate(campAff.affid);
            if (campAff.Analyst == null && campAff.analyst_id.HasValue)
                campAff.Analyst = GetAnalyst(campAff.analyst_id.Value);
            if (campAff.Strategist == null && campAff.strategist_id.HasValue)
                campAff.Strategist = GetStrategist(campAff.strategist_id.Value);
            //TODO: set Analyst/Strategist to null if id is null ?
        }
        public bool SaveCampAff(CampAff campAff)
        {
            if (context.CampAffs.Any(x => x.pid == campAff.pid && x.affid == campAff.affid))
            {
                // Save existing
                var entry = context.Entry(campAff);
                entry.State = EntityState.Modified;
            }
            else
            {
                // Save New
                context.CampAffs.Add(campAff);
            }
            context.SaveChanges();
            return true;
        }

        public IQueryable<Campaign> Campaigns(int? amId = null, int? advertiserId = null, int? affId = null, bool activeOnly = false)
        {
            // Note: specifying an affId only works when activeOnly==true

            var campaigns = context.Campaigns.AsQueryable();
            if (amId.HasValue)
                campaigns = campaigns.Where(c => c.account_manager_id == amId.Value);
            if (advertiserId.HasValue)
                campaigns = campaigns.Where(c => c.advertiser_id == advertiserId.Value);

            if (activeOnly && affId == null)
            {
                return (from c in campaigns
                        join i in context.Items on c.pid equals i.pid
                        select c).Distinct();
            }
            else if (activeOnly && affId.HasValue)
            {
                return (from c in campaigns
                        join i in context.Items on c.pid equals i.pid
                        where i.affid == affId.Value
                        select c).Distinct();
            }
            else
                return campaigns;
        }

        public IEnumerable<CampaignAmount> CampaignAmounts(int pid, int? campaignStatus)
        {
            var campaigns = context.Campaigns.Where(c => c.pid == pid);
            return CampaignAmounts(campaigns, true, campaignStatus);
        }
        public IEnumerable<CampaignAmount> CampaignAmounts(int? amId, int? advertiserId, bool byAffiliate, int? campaignStatus)
        {
            var campaigns = Campaigns(amId, advertiserId);
            return CampaignAmounts(campaigns, byAffiliate, campaignStatus);
        }
        private IEnumerable<CampaignAmount> CampaignAmounts(IQueryable<Campaign> campaigns, bool byAffiliate, int? campaignStatus)
        {
            var items = Items(campaignStatus).Where(i => (i.total_revenue.HasValue && i.total_revenue != 0) ||
                                                         (i.total_cost.HasValue && i.total_cost != 0));

            IEnumerable<CampaignAmount> amounts;
            if (!byAffiliate) // by campaign
            {
                var itemGroups = items.GroupBy(i => new { i.pid, i.revenue_currency_id });
                var rawAmounts = from ig in itemGroups
                                 select new
                                 {
                                     ig.Key.pid,
                                     ig.Key.revenue_currency_id,
                                     revenue = ig.Sum(g => g.total_revenue.HasValue ? g.total_revenue.Value : 0),
                                     numUnits = (int)ig.Sum(g => g.num_units),
                                     numAffs = ig.Select(g => g.affid).Distinct().Count()
                                 };

                //var invItems = InvoiceItems(pid, affId)
                var invItemGroups = context.InvoiceItems.Where(i => i.pid.HasValue)
                                                        .GroupBy(i => new { pid = i.pid.Value, i.currency_id });
                var invoicedAmounts = from ig in invItemGroups
                                      select new
                                      {
                                          ig.Key.pid,
                                          ig.Key.currency_id,
                                          amount = ig.Sum(g => g.total_amount.HasValue ? g.total_amount.Value : 0)
                                      };

                var query = from ra in rawAmounts
                            join c in campaigns on ra.pid equals c.pid
                            join rc in context.Currencies on ra.revenue_currency_id equals rc.id
                            join ia in invoicedAmounts on new { ra.pid, currency_id = ra.revenue_currency_id } equals
                                                          new { ia.pid, ia.currency_id } into gj
                            from invoicedAmount in gj.DefaultIfEmpty()
                            select new { ra, c, rc, invoicedAmount };

                amounts = query.ToList().Select(q => new CampaignAmount()
                          {
                              AdvId = q.c.advertiser_id,
                              AdvertiserName = q.c.Advertiser.name,
                              Pid = q.ra.pid,
                              CampaignName = q.c.campaign_name,
                              CampaignDisplayName = q.c.DisplayName,
                              RevenueCurrency = q.rc,
                              Revenue = q.ra.revenue,
                              InvoicedAmount = (q.invoicedAmount == null ? 0 : q.invoicedAmount.amount),
                              NumUnits = q.ra.numUnits,
                              NumAffs = q.ra.numAffs
                          });
            }
            else // by affiliate
            {
                var itemGroups = items.GroupBy(i => new { i.pid, i.affid, i.revenue_currency_id, i.cost_currency_id });
                var rawAmounts = from ig in itemGroups
                                 select new
                                 {
                                     ig.Key.pid,
                                     ig.Key.affid,
                                     ig.Key.revenue_currency_id,
                                     revenue = ig.Sum(g => g.total_revenue.HasValue ? g.total_revenue.Value : 0),
                                     numUnits = (int)ig.Sum(g => g.num_units),
                                     ig.Key.cost_currency_id,
                                     cost = ig.Sum(g => g.total_cost.HasValue ? g.total_cost.Value : 0)
                                 };

                var invItemGroups = context.InvoiceItems.Where(i => i.pid.HasValue && i.affid.HasValue)
                                                        .GroupBy(i => new { pid = i.pid.Value, affid = i.affid.Value, i.currency_id });
                var invoicedAmounts = from ig in invItemGroups
                                      select new
                                      {
                                          ig.Key.pid,
                                          ig.Key.affid,
                                          ig.Key.currency_id,
                                          amount = ig.Sum(g => g.total_amount.HasValue ? g.total_amount.Value : 0)
                                      };

                var query = from ra in rawAmounts
                            join c in campaigns on ra.pid equals c.pid
                            join a in context.Affiliates on ra.affid equals a.affid
                            join rc in context.Currencies on ra.revenue_currency_id equals rc.id
                            join cc in context.Currencies on ra.cost_currency_id equals cc.id
                            join ia in invoicedAmounts on new { ra.pid, ra.affid, currency_id = ra.revenue_currency_id } equals
                                                          new { ia.pid, ia.affid, ia.currency_id } into gj
                            from invoicedAmount in gj.DefaultIfEmpty()
                            select new { ra, c, a, rc, cc, invoicedAmount };

                amounts = query.AsEnumerable().Select(q => new CampaignAmount()
                          {
                              AdvId = q.c.advertiser_id,
                              AdvertiserName = q.c.Advertiser.name,
                              Pid = q.ra.pid,
                              CampaignName = q.c.campaign_name,
                              CampaignDisplayName = q.c.DisplayName,
                              AffId = q.ra.affid,
                              AffiliateName = q.a.name2,
                              RevenueCurrency = q.rc,
                              Revenue = q.ra.revenue,
                              InvoicedAmount = (q.invoicedAmount == null ? 0 : q.invoicedAmount.amount),
                              CostCurrency = q.cc,
                              Cost = q.ra.cost,
                              NumUnits = q.ra.numUnits,
                              NumAffs = 1
                          });
            }
            return amounts;
        }
        // version2: no joining to invoiced amounts; include unittype and *item ids*
        public IEnumerable<CampaignAmount> CampaignAmounts2(int? campaignStatus)
        {
            var items = Items(campaignStatus);
            var itemGroups = items.GroupBy(i => new { i.pid, i.affid, i.revenue_currency_id, i.cost_currency_id, i.unit_type_id });
            var rawAmounts = from ig in itemGroups
                             select new
                             {
                                 ig.Key.pid,
                                 ig.Key.affid,
                                 ig.Key.unit_type_id,
                                 numUnits = (int)ig.Sum(g => g.num_units),
                                 ig.Key.revenue_currency_id,
                                 revenue = ig.Sum(g => g.total_revenue.HasValue ? g.total_revenue.Value : 0),
                                 ig.Key.cost_currency_id,
                                 cost = ig.Sum(g => g.total_cost.HasValue ? g.total_cost.Value : 0),
                                 itemIds = ig.Select(i => i.id)
                             };
            var query = from ra in rawAmounts.ToList()
                        join c in context.Campaigns on ra.pid equals c.pid
                        join a in context.Affiliates on ra.affid equals a.affid
                        join u in context.UnitTypes on ra.unit_type_id equals u.id
                        join rc in context.Currencies on ra.revenue_currency_id equals rc.id
                        join cc in context.Currencies on ra.cost_currency_id equals cc.id
                        select new { ra, c, a, u, rc, cc };
            var amounts = query.Select(q => new CampaignAmount
            {
                AdvId = q.c.advertiser_id,
                AdvertiserName = q.c.Advertiser.name,
                Pid = q.ra.pid,
                CampaignName = q.c.campaign_name,
                CampaignDisplayName = q.c.DisplayName,
                AffId = q.ra.affid,
                AffiliateName = q.a.name2,
                RevenueCurrency = q.rc,
                Revenue = q.ra.revenue,
                CostCurrency = q.cc,
                Cost = q.ra.cost,
                NumUnits = q.ra.numUnits,
                NumAffs = 1,
                UnitType = q.u,
                ItemIds = q.ra.itemIds
            });
            return amounts;
        }
        // version3: include campaign and account statuses, AcctMgr
        public IEnumerable<CampAffItem> CampAffItems(bool includeNotes, int? campaignStatus = null, int? unitType = null, int? source = null)
        {
            var items = Items(campaignStatus, unitType, source);
            var itemGroups = items.GroupBy(i => new { i.pid, i.affid, i.revenue_currency_id, i.revenue_per_unit, i.cost_currency_id, i.cost_per_unit, i.unit_type_id, i.campaign_status_id, i.item_accounting_status_id });
            var rawAmounts = from ig in itemGroups
                             select new
                             {
                                 ig.Key.pid,
                                 ig.Key.affid,
                                 ig.Key.unit_type_id,
                                 numUnits = (int)ig.Sum(g => g.num_units),
                                 ig.Key.revenue_currency_id,
                                 ig.Key.revenue_per_unit,
                                 revenue = ig.Sum(g => g.total_revenue.HasValue ? g.total_revenue.Value : 0),
                                 ig.Key.cost_currency_id,
                                 ig.Key.cost_per_unit,
                                 cost = ig.Sum(g => g.total_cost.HasValue ? g.total_cost.Value : 0),
                                 itemIds = ig.Select(i => i.id),
                                 ig.Key.campaign_status_id,
                                 ig.Key.item_accounting_status_id
                             };
            var query = from ra in rawAmounts.ToList()
                        join c in context.Campaigns on ra.pid equals c.pid
                        join a in context.Affiliates on ra.affid equals a.affid
                        join u in context.UnitTypes on ra.unit_type_id equals u.id
                        join rc in context.Currencies on ra.revenue_currency_id equals rc.id
                        join cc in context.Currencies on ra.cost_currency_id equals cc.id
                        join cstatus in context.CampaignStatuses on ra.campaign_status_id equals cstatus.id
                        join astatus in context.ItemAccountingStatuses on ra.item_accounting_status_id equals astatus.id
                        //join am in context.AccountManagers on c.account_manager_id equals am.id
                        //join adm in context.AdManagers on c.ad_manager_id equals adm.id
                        select new { ra, c, a, u, rc, cc, cstatus, astatus };
            var caItems = query.Select(q => new CampAffItem
            {
                AdvId = q.c.advertiser_id,
                AdvName = q.c.Advertiser.name,
                Pid = q.ra.pid,
                CampName = q.c.campaign_name,
                //CampDispName = q.c.DisplayName,
                AffId = q.ra.affid,
                AffName = q.a.name2,
                RevCurr = q.rc.name,
                RevPerUnit = q.ra.revenue_per_unit,
                Rev = q.ra.revenue,
                RevUSD = q.ra.revenue * q.rc.to_usd_multiplier,
                CostCurr = q.cc.name,
                CostPerUnit = q.ra.cost_per_unit,
                Cost = q.ra.cost,
                CostUSD = q.ra.cost * q.cc.to_usd_multiplier,
                Units = q.ra.numUnits,
                NumAffs = 1,
                UnitTypeId = q.u.id,
                UnitTypeName = q.u.name,
                ItemIds = q.ra.itemIds,
                CStatusId = q.ra.campaign_status_id,
                //CStatusName = q.cstatus.name,
                AStatusId = q.ra.item_accounting_status_id,
                //AStatusName = q.astatus.name,
                AdMgrId = q.c.ad_manager_id,
                AdMgrName = q.c.AdManager.name,
                AcctMgrId = q.c.account_manager_id,
                AcctMgrName = q.c.AccountManager.name,
                MediaBuyerId = q.a.media_buyer_id,
                MediaBuyerName = q.a.MediaBuyer.name
            });
            if (includeNotes)
            {
                caItems = caItems.ToList();
                var allCampaignNotes = context.CampaignNotes.AsQueryable();
                foreach (var caItemGroup in caItems.GroupBy(i => i.Pid))
                {
                    var campaignNotes = allCampaignNotes.Where(cn => cn.pid == caItemGroup.Key).ToList();
                    if (campaignNotes.Count() > 0)
                    {
                        var noteString = CampAffItem.CampaignNotesToString(campaignNotes);
                        foreach (var caItem in caItemGroup)
                            caItem.Notes = noteString;
                    }
                }
            }
            return caItems;
        }

        //TODO: bypass CampAffItems() in the following method. Query just what's needed.

        public IEnumerable<EOMStat> EOMStatsByAdvertiser(int? unitType)
        {
            // get campaigns & their activity
            var campAffItems = CampAffItems(false, null, unitType);

            // group by advertiser
            var advGroups = campAffItems.GroupBy(ca => new { ca.AdvId, ca.AdvName });
            var advStats = advGroups.Select(g => new EOMStat
            {
                Name = g.Key.AdvName,
                RevUSD = g.Sum(ca => ca.RevUSD),
                CostUSD = g.Sum(ca => ca.CostUSD)
            });
            return advStats;
        }

        // unused...
        public IEnumerable<CampaignAmount> CampaignAmounts(IEnumerable<CampAffId> campAffIds)
        {
            var pids = campAffIds.Select(ca => ca.pid).Distinct();
            var campaigns = context.Campaigns.Where(c => pids.Contains(c.pid));
            var campaignAmountsAll = CampaignAmounts(campaigns, true, null);

            //now, narrow down to just the specified campaign/affiliate combos
            List<CampaignAmount> campaignAmounts = new List<CampaignAmount>();
            var caGroups = campaignAmountsAll.GroupBy(ca => ca.Pid);
            foreach (var caGroup in caGroups)
            {   // doing one pid at a time...
                var affids = campAffIds.Where(ca => ca.pid == caGroup.Key).Select(ca => ca.affid);
                var amounts = caGroup.Where(ca => affids.Contains(ca.AffId.Value));
                campaignAmounts.AddRange(amounts);
            }
            return campaignAmounts;
        }

        public IQueryable<InvoiceItem> InvoiceItems(int? pid, int? affId)
        {
            var invItems = context.InvoiceItems.AsQueryable();
            if (pid.HasValue)
                invItems = invItems.Where(i => i.pid == pid.Value);
            if (affId.HasValue)
                invItems = invItems.Where(i => i.affid == affId.Value);
            return invItems;
        }

        // Given the specified pid/affid pairs, generate an Invoice with InvoiceItems
        public Invoice GenerateInvoice(IEnumerable<CampAffId> campAffIds)
        {
            var invoice = new Invoice();

            //var itemGroups = Items(true).GroupBy(i => new { i.pid, i.affid, i.revenue_currency_id });
            // could do a where contains using a list of distinct pids... then do aggregates

            //var items = Items(CampaignStatus.Default); // only unfinalized amounts will be included
            var items = Items().Where(i => i.total_revenue.HasValue && i.total_revenue != 0);

            foreach (var campAffId in campAffIds)
            {
                var itemGroups = items.Where(i => i.pid == campAffId.pid && i.affid == campAffId.affid)
                                      .GroupBy(i => new { i.revenue_currency_id, i.revenue_per_unit, i.unit_type_id });
                foreach (var itemGroup in itemGroups) // usually just one (currency/revenue_per_unit/unit_type)
                {
                    var ii = new InvoiceItem()
                    {
                        pid = campAffId.pid,
                        affid = campAffId.affid,
                        currency_id = itemGroup.Key.revenue_currency_id,
                        CurrencyName = CurrencyName(itemGroup.Key.revenue_currency_id),
                        amount_per_unit = itemGroup.Key.revenue_per_unit,
                        unit_type_id = itemGroup.Key.unit_type_id,
                        UnitTypeName = UnitTypeName(itemGroup.Key.unit_type_id),
                        num_units = (int)itemGroup.Sum(i => i.num_units)
                    };
                    // check if any amounts for this pid/affid/currency/amount_per_unit/unit_type_id have already been invoiced
                    var existingInvoiceItems = context.InvoiceItems.Where(i =>
                        i.pid == ii.pid && i.affid == ii.affid && i.currency_id == ii.currency_id &&
                        i.amount_per_unit == ii.amount_per_unit && i.unit_type_id == ii.unit_type_id);
                    if (existingInvoiceItems.Any())
                    {   // if so, subtract that many units
                        int numUnits = existingInvoiceItems.Sum(i => i.num_units);
                        ii.num_units -= numUnits;
                    }
                    if (ii.num_units > 0)
                    {
                        ii.total_amount = ii.amount_per_unit * ii.num_units;
                        invoice.InvoiceItems.Add(ii);
                    }
                }
            }
            GenerateInvoiceLineItems(invoice, true);

            return invoice;
        }

        private void GenerateInvoiceLineItems(Invoice invoice, bool setExtended)
        {
            bool firstGroup = true;

            // group the items by pid/currency/amount_per_unit/unit_type and generate a LineItem for each (with subitems for the various affiliates)
            var itemGroups = invoice.InvoiceItems.GroupBy(i => new { i.pid, i.currency_id, i.amount_per_unit, i.unit_type_id });
            List<InvoiceLineItem> invoiceLineItems = new List<InvoiceLineItem>();
            foreach (var itemGroup in itemGroups)
            {
                // if pid == null, skip ?

                var lineItem = new InvoiceLineItem()
                {
                    Campaign = GetCampaign(itemGroup.Key.pid.Value),
                    Currency = GetCurrency(itemGroup.Key.currency_id),
                    ItemCode = itemGroup.Key.unit_type_id.HasValue ? ItemCode(itemGroup.Key.unit_type_id.Value) : null,
                    SubItems = itemGroup.ToList()
                };
                if (firstGroup && setExtended)
                    SetInvoiceExtended(invoice, lineItem.Campaign);
                foreach (var subItem in lineItem.SubItems)
                {
                    if (subItem.affid.HasValue)
                        subItem.AffiliateName = AffiliateName(subItem.affid.Value, true);
                }
                context.Entry(lineItem.Campaign).State = EntityState.Detached;

                invoiceLineItems.Add(lineItem);
                firstGroup = false;
            }
            invoice.LineItems = invoiceLineItems.OrderBy(li => li.Campaign.DisplayName).ThenBy(li => li.ItemCode).ThenByDescending(li => li.AmountPerUnit).ToList();
        }

        // Set the extended properties of an invoice. Pass in an example campaign, if known.
        private void SetInvoiceExtended(Invoice invoice, Campaign campaign = null)
        {
            if (campaign == null)
            {   // Assume all items' campaigns are for the same advertiser (& account manager). Use the first one.
                var itemWithPid = invoice.InvoiceItems.FirstOrDefault(i => i.pid.HasValue);
                if (itemWithPid != null)
                    campaign = GetCampaign(itemWithPid.pid.Value);
            }
            if (campaign != null)
            {
                invoice.Advertiser = campaign.Advertiser;
                context.Entry(invoice.Advertiser).State = EntityState.Detached;

                invoice.Advertiser.AccountManager = campaign.AccountManager;
                context.Entry(invoice.Advertiser.AccountManager).State = EntityState.Detached;
            }
        }

        public void SaveInvoice(Invoice invoice, bool markSentToAccounting = false)
        {
            if (markSentToAccounting)
                invoice.invoice_status_id = InvoiceStatus.AccountingReview;

            context.Invoices.Add(invoice);
            SaveChanges();
        }

        public IQueryable<Invoice> Invoices(bool fillExtended)
        {
            var invoices = context.Invoices.AsQueryable();
            if (fillExtended)
            {
                foreach (var invoice in invoices)
                {
                    SetInvoiceExtended(invoice);
                }
            }
            return invoices;
        }

        public Invoice GetInvoice(int id, bool fillLineItems = false)
        {
            var invoice = context.Invoices.FirstOrDefault(i => i.id == id);
            if (fillLineItems && invoice != null)
            {
                GenerateInvoiceLineItems(invoice, true);
            }
            return invoice;
        }

        public bool SetInvoiceStatus(int id, int statusId)
        {
            bool success = false;
            var invoice = GetInvoice(id);
            if (invoice != null)
            {
                invoice.invoice_status_id = statusId;
                SaveChanges();
                success = true;
            }
            return success;
        }

        //---

        public IQueryable<MarginApproval> MarginApprovals(bool fillExtended)
        {
            var marginApprovals = context.MarginApprovals.AsQueryable();
            if (fillExtended)
            {
                foreach (var marginApproval in marginApprovals)
                    SetMarginApprovalExtended(marginApproval);
            }
            return marginApprovals;
        }

        private void SetMarginApprovalExtended(MarginApproval marginApproval)
        {
            if (marginApproval.Campaign == null && marginApproval.pid.HasValue)
                marginApproval.Campaign = GetCampaign(marginApproval.pid.Value);

            if (marginApproval.Affiliate == null && marginApproval.affid.HasValue)
                marginApproval.Affiliate = GetAffiliate(marginApproval.affid.Value);
        }

        public void SaveMarginApproval(MarginApproval marginApproval)
        {
            context.MarginApprovals.Add(marginApproval);
        }

        //public void SaveMarginApproval(int pid, int affid, string comment, string userIdentity)
        //{
        //    var items = Items(CampaignStatus.Default).Where(i => i.pid == pid && i.affid == affid);

        //    // Usually there will be just one group (per pid/affid)
        //    var itemGroups = items.GroupBy(i => new { i.revenue_currency_id, i.cost_currency_id });
        //    var groupSummaries = from ig in itemGroups
        //                         select new
        //                         {
        //                             ig.Key.revenue_currency_id,
        //                             revenue = ig.Sum(i => i.total_revenue),
        //                             ig.Key.cost_currency_id,
        //                             cost = ig.Sum(i => i.total_cost)
        //                         };

        //    var query = from gs in groupSummaries
        //                join rc in context.Currencies on gs.revenue_currency_id equals rc.id
        //                join cc in context.Currencies on gs.cost_currency_id equals cc.id
        //                select new { gs, rc, cc };

        //    var now = DateTime.Now;

        //    foreach (var queryItem in query.AsEnumerable())
        //    {
        //        var marginApproval = new MarginApproval
        //        {
        //            pid = pid,
        //            affid = affid,
        //            revenue_currency_id = queryItem.gs.revenue_currency_id,
        //            total_revenue = queryItem.gs.revenue,
        //            cost_currency_id = queryItem.gs.cost_currency_id,
        //            total_cost = queryItem.gs.cost,
        //            comment = comment,
        //            added_by = userIdentity,
        //            created = now
        //        };
        //        var revenueUSD = (queryItem.gs.revenue ?? 0) * queryItem.rc.to_usd_multiplier;
        //        var costUSD = (queryItem.gs.cost ?? 0) * queryItem.cc.to_usd_multiplier;
        //        if (revenueUSD != 0)
        //        {
        //            marginApproval.margin_pct = decimal.Round(1 - costUSD / revenueUSD, 3);
        //        }
        //        context.MarginApprovals.Add(marginApproval);
        //    }
        //    context.SaveChanges();
        //}

        // Use this if margin is computed by the database
        //public void SaveMarginApproval(int pid, int affid, string comment, string userIdentity)
        //{
        //    var items = Items(CampaignStatus.Default).Where(i => i.pid == pid && i.affid == affid);
        //    var itemGroups = items.GroupBy(i => new { i.revenue_currency_id, i.cost_currency_id });
        //    var now = DateTime.Now;

        //    // Usually there will be just one group (per pid/affid)
        //    foreach (var itemGroup in itemGroups)
        //    {
        //        var marginApproval = new MarginApproval
        //        {
        //            pid = pid,
        //            affid = affid,
        //            revenue_currency_id = itemGroup.Key.revenue_currency_id,
        //            total_revenue = itemGroup.Sum(i => i.total_revenue ?? 0),
        //            cost_currency_id = itemGroup.Key.cost_currency_id,
        //            total_cost = itemGroup.Sum(i => i.total_cost ?? 0),
        //            comment = comment,
        //            added_by = userIdentity,
        //            created = now
        //        };
        //    }
        //}

        //---

        private IQueryable<Item> Items(int? campaignStatus = null, int? unitType = null, int? source = null)
        {
            var items = context.Items.AsQueryable();
            if (campaignStatus.HasValue)
                items = items.Where(i => i.campaign_status_id == campaignStatus);
            if (unitType.HasValue)
                items = items.Where(i => i.unit_type_id == unitType);
            if (source.HasValue)
                items = items.Where(i => i.source_id == source);
            return items;
        }
        //private IQueryable<Item> Items(int[] campaignStatuses)
        //{
        //    var items = context.Items.Where(i => campaignStatuses.Contains(i.campaign_status_id));
        //    return items;
        //}

        //---
        public void ChangeUnitType(IEnumerable<int> itemIds, int unitTypeId)
        {
            var items = context.Items.Where(i => itemIds.Contains(i.id));
            foreach(var item in items)
            {
                item.unit_type_id = unitTypeId;
            }
            SaveChanges();
        }


        public IncomeType GetIncomeType(int id)
        {
            return context.IncomeTypes.Find(id);
        }
        public IQueryable<IncomeType> IncomeTypes()
        {
            return context.IncomeTypes;
        }
        public bool SaveIncomeType(IncomeType inIncomeType)
        {
            var incomeType = GetIncomeType(inIncomeType.id);
            if (incomeType == null)
                return false;

            incomeType.name = inIncomeType.name;
            incomeType.qb_code = inIncomeType.qb_code;
            SaveChanges();
            return true;
        }
        public IncomeType NewIncomeType(string name = "zNew")
        {
            int id = context.IncomeTypes.Select(x => x.id).DefaultIfEmpty(0).Max() + 1;
            var incomeType = new IncomeType
            {
                id = id,
                name = name
            };
            context.IncomeTypes.Add(incomeType);
            SaveChanges();
            return incomeType;
        }
        public bool NewIncomeType(IncomeType incomeType)
        {
            if (context.IncomeTypes.Any(x => x.id == incomeType.id))
                return false; // an entity with that id already exists
            context.IncomeTypes.Add(incomeType);
            SaveChanges();
            return true;
        }

        private List<UnitType> _unitTypeList;
        public List<UnitType> UnitTypeList
        {
            get
            {
                if (_unitTypeList == null)
                {
                    _unitTypeList = context.UnitTypes.ToList();
                }
                return _unitTypeList;
            }
        }

        public string UnitTypeName(int unitTypeId)
        {
            var unitType = UnitTypeList.FirstOrDefault(ut => ut.id == unitTypeId);
            if (unitType != null)
                return unitType.name;
            else
                return null;
        }
        public string ItemCode(int unitTypeId)
        {
            string unitTypeName = UnitTypeName(unitTypeId);
            return UnitType.ToItemCode(unitTypeName);
        }

        public bool UnitTypeExists(int unitTypeId)
        {
            return UnitTypeList.Any(u => u.id == unitTypeId);
        }
        public UnitType GetUnitType(int unitTypeId)
        {
            return UnitTypeList.FirstOrDefault(u => u.id == unitTypeId);
        }
        public UnitType GetUnitType(string unitTypeName)
        {
            return UnitTypeList.FirstOrDefault(ut => ut.name.ToLower() == unitTypeName.ToLower());
        }

        public IQueryable<UnitType> UnitTypes()
        {
            return context.UnitTypes;
        }
        public bool SaveUnitType(UnitType inUnitType)
        {
            var unitType = GetUnitType(inUnitType.id);
            if (unitType == null)
                return false;

            unitType.name = inUnitType.name;
            unitType.income_type_id = inUnitType.income_type_id;
            SaveChanges();
            return true;
        }
        public UnitType NewUnitType(string name = "zNew")
        {
            var unitType = new UnitType
            {
                name = name
            };
            context.UnitTypes.Add(unitType);
            SaveChanges();
            return unitType;
        }

        //---

        private List<CampaignStatus> _campaignStatusList;
        public List<CampaignStatus> CampaignStatusList
        {
            get
            {
                if (_campaignStatusList == null) _campaignStatusList = context.CampaignStatuses.ToList();
                return _campaignStatusList;
            }
        }

        private List<ItemAccountingStatus> _accountingStatusList;
        public List<ItemAccountingStatus> AccountingStatusList
        {
            get
            {
                if (_accountingStatusList == null) _accountingStatusList = context.ItemAccountingStatuses.ToList();
                return _accountingStatusList;
            }
        }

        //---

        private List<Currency> _currencyList;
        public List<Currency> CurrencyList
        {
            get
            {
                if (_currencyList == null)
                {
                    _currencyList = context.Currencies.ToList();
                }
                return _currencyList;
            }
        }

        public string CurrencyName(int currencyId)
        {
            string currencyName = null;
            var currency = GetCurrency(currencyId);
            if (currency != null)
                currencyName = currency.name;
            return currencyName;
        }
        public int CurrencyId(string currencyName)
        {
            int currencyId = 0;
            var currency = GetCurrency(currencyName);
            if (currency != null)
                currencyId = currency.id;
            return currencyId;
        }

        public bool CurrencyExists(int currency)
        {
            return CurrencyList.Any(c => c.id == currency);
        }
        public Currency GetCurrency(int currencyId)
        {
            return CurrencyList.FirstOrDefault(c => c.id == currencyId);
        }
        public Currency GetCurrency(string currencyName)
        {
            return CurrencyList.FirstOrDefault(c => c.name.ToLower() == currencyName.ToLower());
        }

        //---

        public bool CampaignExists(int pid)
        {
            return context.Campaigns.Any(c => c.pid == pid);
        }
        public Campaign GetCampaign(int pid)
        {
            return context.Campaigns.FirstOrDefault(c => c.pid == pid);
        }
        public bool SaveCampaign(Campaign inCampaign)
        {
            var campaign = GetCampaign(inCampaign.pid);
            if (campaign == null)
                return false;

            campaign.display_name = inCampaign.display_name;
            SaveChanges();
            return true;
        }

        public IQueryable<Affiliate> Affiliates(bool withActivity = false)
        {
            if (withActivity)
            {
                return (from a in context.Affiliates
                        join i in context.Items on a.affid equals i.affid
                        select a).Distinct();
            }
            else
                return context.Affiliates;
        }
        public bool AffiliateExists(int affId)
        {
            return context.Affiliates.Any(a => a.affid == affId);
        }
        public Affiliate GetAffiliate(int affId)
        {
            return context.Affiliates.FirstOrDefault(a => a.affid == affId);
        }
        public Affiliate GetAffiliateById(int id)
        {
            return context.Affiliates.FirstOrDefault(a => a.id == id);
        }
        public string AffiliateName(int affId, bool withId = false)
        {
            var affiliate = GetAffiliate(affId);
            if (affiliate != null)
                return (withId ? affiliate.name2 : affiliate.name);
            else
                return null;
        }

        public Source GetSource(int sourceId)
        {
            return context.Sources.FirstOrDefault(s => s.id == sourceId);
        }
        public Source GetSource(string sourceName)
        {
            return context.Sources.FirstOrDefault(s => s.name == sourceName);
        }

        public Item GetItem(int id, bool fillExtended = false)
        {
            var item = context.Items.FirstOrDefault(i => i.id == id);
            if (fillExtended) SetItemExtended(item);
            return item;
        }
        private void SetItemExtended(Item item)
        {
            if (item != null)
            {
                var campaign = GetCampaign(item.pid);
                if (campaign != null)
                {
                    item.CampaignName = campaign.campaign_name;
                    if (campaign.Advertiser != null)
                        item.AdvertiserName = campaign.Advertiser.name;
                    if (campaign.AccountManager != null)
                        item.AccountManagerName = campaign.AccountManager.name;
                    if (campaign.AdManager != null)
                        item.AdManagerName = campaign.AdManager.name;
                }
                var affiliate = GetAffiliate(item.affid);
                if (affiliate != null)
                {
                    item.AffiliateName = affiliate.name;
                    if (affiliate.MediaBuyer != null)
                        item.MediaBuyerName = affiliate.MediaBuyer.name;
                }
            }
        }

        public IQueryable<Item> GetItems(IEnumerable<int> ids)
        {
            return context.Items.Where(i => ids.Contains(i.id));
        }
        public IQueryable<Item> GetItems(int? pid = null, int? affId = null, int? advertiserId = null)
        {
            var items = context.Items.AsQueryable();
            if (pid.HasValue)
                items = items.Where(i => i.pid == pid.Value);
            if (affId.HasValue)
                items = items.Where(i => i.affid == affId.Value);
            if (advertiserId.HasValue)
            {
                var campaigns = Campaigns(advertiserId: advertiserId);
                var pids = campaigns.Select(c => c.pid).ToArray();
                items = items.Where(i => pids.Contains(i.pid));
            }
            return items;
        }

        public void AddItem(Item item)
        {
            context.Items.Add(item);
        }
        public bool ItemExists(Item item)
        {
            return context.Items.Any(i =>
                i.pid == item.pid &&
                i.affid == item.affid &&
                i.unit_type_id == item.unit_type_id &&
                i.num_units == item.num_units &&
                i.revenue_currency_id == item.revenue_currency_id &&
                i.revenue_per_unit == item.revenue_per_unit &&
                i.cost_currency_id == item.cost_currency_id &&
                i.cost_per_unit == item.cost_per_unit &&
                i.notes == item.notes &&
                i.source_id == item.source_id);
        }

        // Are there any unfinalized items with the specified (item's) pid and affid?
        //public bool AnyUnfinalizedItems(Item item, bool extraOnly)
        //{
        //    if (extraOnly) // only check extra items
        //        return context.Items.Any(i =>
        //            i.pid == item.pid &&
        //            i.affid == item.affid &&
        //            i.campaign_status_id == CampaignStatus.Default &&
        //            i.source_id == Source.Other);
        //    else
        //        return context.Items.Any(i =>
        //            i.pid == item.pid &&
        //            i.affid == item.affid &&
        //            i.campaign_status_id == CampaignStatus.Default);
        //}

        // --- Auditing ---

        // A quick and dirty restore...
        public void RestoreX()
        {
            var audits = context.Audits.Where(a => a.Operation == "d" && a.AuditDate > new DateTime(2017, 9, 8, 11, 45, 0)
                            && (a.PrimaryKey.CompareTo("2900") < 0 || (a.PrimaryKey.CompareTo("3800") > 0 && a.PrimaryKey.CompareTo("5900") < 0)));
            var aGroups = audits.GroupBy(a => a.PrimaryKey);
            int count = 0;
            foreach (var g in aGroups)
            {
                count++;
                var item = new Item
                {
                    //name = g.Single(x => x.ColumnName == "[name]").OldValue,
                    pid = int.Parse(g.Single(x => x.ColumnName == "[pid]").OldValue),
                    affid = int.Parse(g.Single(x => x.ColumnName == "[affid]").OldValue),
                    source_id = int.Parse(g.Single(x => x.ColumnName == "[source_id]").OldValue),
                    unit_type_id = int.Parse(g.Single(x => x.ColumnName == "[unit_type_id]").OldValue),
                    revenue_currency_id = int.Parse(g.Single(x => x.ColumnName == "[revenue_currency_id]").OldValue),
                    cost_currency_id = int.Parse(g.Single(x => x.ColumnName == "[cost_currency_id]").OldValue),
                    revenue_per_unit = decimal.Parse(g.Single(x => x.ColumnName == "[revenue_per_unit]").OldValue),
                    cost_per_unit = decimal.Parse(g.Single(x => x.ColumnName == "[cost_per_unit]").OldValue),
                    num_units = decimal.Parse(g.Single(x => x.ColumnName == "[num_units]").OldValue),
                    notes = g.Single(x => x.ColumnName == "[notes]").OldValue,
                    accounting_notes = g.Single(x => x.ColumnName == "[accounting_notes]").OldValue,
                    item_accounting_status_id = int.Parse(g.Single(x => x.ColumnName == "[item_accounting_status_id]").OldValue),
                    item_reporting_status_id = int.Parse(g.Single(x => x.ColumnName == "[item_reporting_status_id]").OldValue),
                    campaign_status_id = int.Parse(g.Single(x => x.ColumnName == "[campaign_status_id]").OldValue),
                    media_buyer_approval_status_id = int.Parse(g.Single(x => x.ColumnName == "[media_buyer_approval_status_id]").OldValue),
                };
                var audit_name = g.SingleOrDefault(x => x.ColumnName == "[name]");
                if (audit_name != null)
                    item.name = audit_name.OldValue;
                var audit_batch_id = g.SingleOrDefault(x => x.ColumnName == "[batch_id]");
                if (audit_batch_id != null)
                    item.batch_id = int.Parse(audit_batch_id.OldValue);
                context.Items.Add(item);
            }
            context.SaveChanges();
        }

        // this was slower (took about 5 secs)
        public IQueryable<AuditSummary> AuditSummariesX()
        {
            var audits = context.Audits;
            var aSums = audits.GroupBy(a => DbFunctions.TruncateTime(a.AuditDate))
                .Select(g => new AuditSummary
                {
                    Date = g.Key,
                    NumUpdates = g.Where(a => a.Operation == "u").Count(),
                    NumInserts = g.Where(a => a.Operation == "i").Count(),
                    NumDeletes = g.Where(a => a.Operation == "d" && a.ColumnName == "[id]").Count()
                });
            return aSums;
        }
        public IQueryable<AuditSummary> AuditSummaries()
        {
            var uSums = Audits(operation: "u").GroupBy(a => DbFunctions.TruncateTime(a.AuditDate))
                .Select(g => new AuditSummary
                {
                    Date = g.Key,
                    NumUpdates = g.Count(), NumInserts = 0, NumDeletes = 0
                });
            var iSums = Audits(operation: "i").GroupBy(a => DbFunctions.TruncateTime(a.AuditDate))
                .Select(g => new AuditSummary
                {
                    Date = g.Key,
                    NumUpdates = 0, NumInserts = g.Count(), NumDeletes = 0
                });
            var dSums = Audits(operation: "d").Where(a => a.ColumnName == "[id]").GroupBy(a => DbFunctions.TruncateTime(a.AuditDate))
                .Select(g => new AuditSummary
                {
                    Date = g.Key,
                    NumUpdates = 0, NumInserts = 0, NumDeletes = g.Count()
                });

            var aSums = uSums.Concat(iSums.Concat(dSums)).GroupBy(s => s.Date)
                .Select(g => new AuditSummary
                {
                    Date = g.Key,
                    NumUpdates = g.Sum(s => s.NumUpdates),
                    NumInserts = g.Sum(s => s.NumInserts),
                    NumDeletes = g.Sum(s => s.NumDeletes)
                });
            return aSums;
        }

        public IQueryable<Audit> Audits(DateTime? date = null, string operation = null, string primaryKey = null, string sysUser = null)
        {
            var audits = context.Audits.AsQueryable();
            if (date.HasValue)
                audits = audits.Where(a => DbFunctions.TruncateTime(a.AuditDate) == date);
            if (!String.IsNullOrWhiteSpace(operation))
                audits = audits.Where(a => a.Operation == operation);
            if (!String.IsNullOrWhiteSpace(primaryKey))
                audits = audits.Where(a => a.PrimaryKey == primaryKey);
            if (!String.IsNullOrWhiteSpace(sysUser))
                audits = audits.Where(a => a.SysUser == sysUser);
            return audits;
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
