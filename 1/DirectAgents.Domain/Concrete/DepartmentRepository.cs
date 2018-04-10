using System;
using System.Collections.Generic;
using System.Linq;
using DirectAgents.Domain.Abstract;
using DirectAgents.Domain.DTO;

namespace DirectAgents.Domain.Concrete
{
    public class Prog_DeptRepository : IDepartmentRepository
    {
        // Assume this repo is disposed of elsewhere
        public IRevTrackRepository rtRepo { get; set; }

        public Prog_DeptRepository(IRevTrackRepository rtRepo)
        {
            this.rtRepo = rtRepo;
        }

        public IEnumerable<IRTLineItem> StatsByClient(DateTime monthStart, bool includeZeros = false, int? maxClients = null)
        {
            var clients = rtRepo.ProgClients();
            var lineItems = new List<IRTLineItem>();
            foreach (var client in clients.OrderBy(c => c.Name))
            {
                if (maxClients.HasValue && lineItems.Count >= maxClients.Value)
                    break;

                var clientStats = rtRepo.GetProgClientStats(monthStart, client.Id);
                if (includeZeros || clientStats.DACost > 0 || clientStats.TotalRevenue > 0)
                {
                    var lineItem = new RTLineItem(clientStats);
                    lineItems.Add(lineItem);
                }
            }
            return lineItems;
        }

        public IRTLineItem StatSummaryForClient(int abClientId, DateTime monthStart)
        {
            var lineItemList = new List<IRTLineItem>();
            var progClients = rtRepo.ProgClients(ABClientId: abClientId);

            // Usually there's just one...
            foreach (var progClient in progClients)
            {
                var progClientStats = rtRepo.GetProgClientStats(monthStart, progClient);
                var lineItem = new RTLineItem(progClientStats);
                lineItemList.Add(lineItem);
            }
            var summaryLineItem = new RTLineItem(lineItemList)
            {
                Name = "Programmatic (TD/Social)"
            };
            return summaryLineItem;
        }

        public IEnumerable<IRTLineItem> StatBreakdownByLineItem(int abClientId, DateTime monthStart, bool separateFees = false, bool combineFees = false)
        {
            var lineItemList = new List<IRTLineItem>();
            var progClients = rtRepo.ProgClients(ABClientId: abClientId);
            decimal totalFee = 0;

            // Usually there's just one...
            foreach (var progClient in progClients)
            {
                var progClientStats = rtRepo.GetProgClientStats(monthStart, progClient);
                foreach (var tdLineItem in progClientStats.LineItems)
                {
                    var rtLineItem = new RTLineItem(tdLineItem);

                    if (tdLineItem.MgmtFee > 0 && separateFees)
                    {
                        rtLineItem.Revenue -= tdLineItem.MgmtFee; // take out the fee and see if there's anything left
                        if (rtLineItem.Revenue != 0 && rtLineItem.Cost != 0)
                            lineItemList.Add(rtLineItem);

                        if (combineFees)
                            totalFee += tdLineItem.MgmtFee;
                        else
                        {
                            var feeLineItem = new RTLineItem(tdLineItem); // for setting the name
                            feeLineItem.Name = feeLineItem.Name + " - media fee";
                            feeLineItem.Revenue = tdLineItem.MgmtFee;
                            feeLineItem.Cost = 0;
                            lineItemList.Add(feeLineItem);
                        }
                    }
                    else // Not separating fees
                    {
                        lineItemList.Add(rtLineItem);
                    }
                }
            }
            if (separateFees && combineFees && totalFee > 0)
            {
                var feeLI = new RTLineItem
                {
                    Name = "Trading Desk - media fee",
                    Revenue = totalFee
                };
                lineItemList.Add(feeLI);
            }
            return lineItemList;
        }
    }

    public class Cake_DeptRepository : IDepartmentRepository
    {
        // Assume this repo is disposed of elsewhere
        public IMainRepository mainRepo { get; set; }

        public Cake_DeptRepository(IMainRepository mainRepo)
        {
            this.mainRepo = mainRepo;
        }

        // Get one lineItem per client
        public IEnumerable<IRTLineItem> StatsByClient(DateTime monthStart, bool includeZeros = false, int? maxClients = null)
        {
            var advertisers = mainRepo.GetAdvertisers().OrderBy(a => a.AdvertiserName).ToList(); //TODO: include? (e.g. AcctMgr)
            var lineItems = StatsForAdvertisers(advertisers, monthStart, includeZeros, maxClients);
            return lineItems;
        }
        private IEnumerable<IRTLineItem> StatsForAdvertisers(IEnumerable<DirectAgents.Domain.Entities.Cake.Advertiser> advertisers, DateTime monthStart, bool includeZeros = false, int? maxClients = null)
        {
            DateTime monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var offerDailySummaries = mainRepo.GetOfferDailySummaries(null, monthStart, monthEnd);
            var lineItems = new List<IRTLineItem>();

            foreach (var adv in advertisers)
            {
                if (maxClients.HasValue && lineItems.Count >= maxClients.Value)
                    break;

                adv.OfferIds = mainRepo.OfferIds(advId: adv.AdvertiserId);
                var ods = offerDailySummaries.Where(o => adv.OfferIds.Contains(o.OfferId));
                if (ods.Any())
                {
                    var lineItem = new RTLineItem
                    {
                        ABId = adv.ABClientId,
                        RTId = adv.AdvertiserId,
                        Name = adv.AdvertiserName,
                        Revenue = ods.Sum(o => o.Revenue),
                        Cost = ods.Sum(o => o.Cost)
                    };
                    // if maxClients is specified, get at most that number of non-zero abStats
                    if (includeZeros || lineItem.Revenue > 0 || lineItem.Cost > 0)
                        lineItems.Add(lineItem);
                }
            }
            return lineItems;
        }

        public IRTLineItem StatSummaryForClient(int abClientId, DateTime monthStart)
        {
            // Usually there's just one...
            var advertisers = mainRepo.GetAdvertisers(ABClientId: abClientId);
            var advLineItems = StatsForAdvertisers(advertisers, monthStart);

            var summaryLineItem = new RTLineItem(advLineItems)
            {
                Name = "Cake (Advertising/MediaBuying/Mobile)"
            };
            return summaryLineItem;
        }

        public IEnumerable<IRTLineItem> StatBreakdownByLineItem(int abClientId, DateTime monthStart, bool separateFees = false, bool combineFees = false)
        {
            // Usually there's just one...
            var advertisers = mainRepo.GetAdvertisers(ABClientId: abClientId);
            foreach (var adv in advertisers)
            {
                var LIs = LineItemsForAdvertiser(adv, monthStart);
                foreach (var li in LIs)
                    yield return li;
            }
        }
        private IEnumerable<IRTLineItem> LineItemsForAdvertiser(DirectAgents.Domain.Entities.Cake.Advertiser advertiser, DateTime monthStart)
        {
            var campSums = mainRepo.GetCampSums(advertiserId: advertiser.AdvertiserId, monthStart: monthStart);
            var csGroups = campSums.GroupBy(x => new { x.OfferId, x.RevenuePerUnit, x.RevCurr });
            var offers = mainRepo.GetOffers();

            var li = from g in csGroups
                     join o in offers on g.Key.OfferId equals o.OfferId
                     select new RTLineItem
                     {
                         Name = o.OfferName,
                         Revenue = g.Sum(cs => cs.Revenue),
                         RevPerUnit = g.Key.RevenuePerUnit,
                         RevCurr = g.Key.RevCurr.Abbr,
                         Units = g.Sum(cs => cs.Units),
                         Cost = g.Sum(cs => cs.Cost),
                         CostCurrFromIEnumerable = g.Select(cs => cs.CostCurr.Abbr).Distinct()
                         //Cost, CostCurr - not used / grouped on
                     };
            return li;
        }

    }
}
