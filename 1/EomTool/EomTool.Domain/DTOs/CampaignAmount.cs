using EomTool.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EomTool.Domain.DTOs
{
    public class CampaignAmount
    {
        public int AdvId { get; set; }
        public string AdvertiserName { get; set; }

        public int Pid { get; set; }
        public string CampaignName { get; set; }
        public string CampaignDisplayName { get; set; }

        public int? AffId { get; set; }
        public string AffiliateName { get; set; }

        public Currency RevenueCurrency { get; set; }
        public decimal Revenue { get; set; }

        public decimal InvoicedAmount { get; set; }

        public Currency CostCurrency { get; set; }
        public decimal Cost { get; set; }

        public decimal? Margin
        {
            get
            {
                if (RevenueCurrency.id == CostCurrency.id)
                    return Revenue - Cost;
                else
                    return null;
            }
        }
        public decimal? MarginPct
        {
            get
            {
                if (Revenue == 0)
                    return null;
                else
                {
                    var revUSD = Revenue * RevenueCurrency.to_usd_multiplier;
                    var costUSD = Cost * CostCurrency.to_usd_multiplier;
                    return (1 - costUSD / revUSD);
                }
            }
        }

        public int NumUnits { get; set; }
        public int NumAffs { get; set; }
        public UnitType UnitType { get; set; }
        public IEnumerable<int> ItemIds { get; set; }

        public string ItemIdsString
        {
            get { return String.Join(",", ItemIds); }
        }
    }

    // TODO: Make an AdvItem - with stats for an advertiser?
    //    ...Have CampAffItem inherit from that?

    public class CampAffItem
    {
        public int AdvId { get; set; }
        public string AdvName { get; set; }

        public int Pid { get; set; }

        public string CampName { get; set; }
        //private string _campName;
        //public string CampName {
        //    set { _campName = value; }
        //    get { return _campName + " (" + Pid + ")"; }
        //}
        //public string CampDispName { get; set; }

        public int AffId { get; set; }
        public string AffName { get; set; }

        public string RevCurr { get; set; }
        public decimal RevPerUnit { get; set; }
        public decimal Rev { get; set; }
        public decimal RevUSD { get; set; }

        public string CostCurr { get; set; }
        public decimal CostPerUnit { get; set; }
        public decimal Cost { get; set; }
        public decimal CostUSD { get; set; }

        public decimal ProfitUSD
        {
            get { return RevUSD - CostUSD; }
        }

        //public decimal? Margin
        //{
        //    get
        //    {
        //        if (RevCurr == CostCurr)
        //            return Rev - Cost;
        //        else
        //            return null;
        //    }
        //}
        public decimal? MarginPct
        {
            get
            {
                if (RevUSD == 0)
                    return null;
                else
                    return 1 - CostUSD / RevUSD;
            }
        }

        public int Units { get; set; }
        public int NumAffs { get; set; } // unused?
        public int UnitTypeId { get; set; }
        public string UnitTypeName { get; set; }

        //note: could have a private get on ItemIds and do the computation in ItemIdsString.get
        public IEnumerable<int> ItemIds { set { ItemIdsString = String.Join(",", value); } }
        public string ItemIdsString { get; set; }

        public IEnumerable<int> GetItemIds()
        {
            return ItemIdsString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => Convert.ToInt32(id));
        }

        public int CStatusId { get; set; }
        //public string CStatusName { get; set; }
        public int AStatusId { get; set; }
        //public string AStatusName { get; set; }

        public int AdMgrId { get; set; }
        public string AdMgrName { get; set; }
        public int AcctMgrId { get; set; }
        public string AcctMgrName { get; set; }
        public int MediaBuyerId { get; set; }
        public string MediaBuyerName { get; set; }

        public string Notes { get; set; }

        public static string CampaignNotesToString(IEnumerable<CampaignNote> campaignNotes)
        {
            var notesIEnum = campaignNotes.OrderByDescending(cn => cn.created)
                .Select(cn => String.Format("{0:M/d/yy h:mmtt}: {1}", cn.created, cn.note));
            return String.Join(" | ", notesIEnum);
        }
    }

    public class EOMStat
    {
        public decimal RevUSD { get; set; }
        public decimal CostUSD { get; set; }
        public decimal MarginUSD
        {
            get { return RevUSD - CostUSD; }
        }

        public string Name { get; set; }
    }
}
