using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using ClientPortal.Data.Contexts;
using ClientPortal.Data.Entities.TD.AdRoll;
using ClientPortal.Data.Entities.TD.DBM;

namespace ClientPortal.Data.Entities.TD
{
    public class TradingDeskAccount
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TradingDeskAccountId { get; set; }

        public bool ShowConversions { get; set; }
        public string FixedMetricName { get; set; }
        public decimal? FixedMetricValue { get; set; }
        public decimal? ManagementFeePct { get; set; }

        public virtual ICollection<AdRollProfile> AdRollProfiles { get; set; }
        public virtual ICollection<InsertionOrder> InsertionOrders { get; set; }

        public bool HasDBM()
        {
            return InsertionOrders != null && InsertionOrders.Count > 0;
        }
        public bool HasAdRoll()
        {
            return AdRollProfiles != null && AdRollProfiles.Count > 0;
        }

        [NotMapped]
        public IEnumerable<UserProfile> UserProfiles { get; set; }
        [NotMapped]
        public IEnumerable<Advertiser> Advertisers { get; set; }

        [NotMapped]
        public DayOfWeek StartDayOfWeek
        {
            get { return DayOfWeek.Monday; } //TODO: put in db
        }

        [NotMapped]
        public string DisplayName
        {
            get
            {
                var displayNames = this.DisplayNames;
                return String.Join(" | ", displayNames);
            }
        }
        [NotMapped]
        public IEnumerable<string> DisplayNames
        {
            get
            {
                var displayNames = new List<string>();
                if (InsertionOrders != null)
                    foreach (var io in InsertionOrders)
                    {
                        if (!String.IsNullOrWhiteSpace(io.InsertionOrderName) && !displayNames.Contains(io.InsertionOrderName))
                            displayNames.Add(io.InsertionOrderName);
                    }
                if (AdRollProfiles != null)
                    foreach (var arp in AdRollProfiles)
                    {
                        if (!String.IsNullOrWhiteSpace(arp.Name) && !displayNames.Contains(arp.Name))
                            displayNames.Add(arp.Name);
                    }
                if (displayNames.Count == 0)
                {
                    if (UserProfiles != null)
                        foreach (var up in UserProfiles)
                        {
                            displayNames.Add(up.UserName);
                        }
                    if (displayNames.Count == 0)
                        displayNames.Add(TradingDeskAccountId.ToString());
                }
                return displayNames;
            }
        }

        [NotMapped]
        public string Login
        {
            get
            {
                if (UserProfiles == null)
                    return "";
                return String.Join(", ", UserProfiles.Select(u => u.UserName).ToArray());
                // (normally there should be just one)
            }
        }

        [NotMapped]
        public string FixedMetricDisplay
        {
            get
            {
                if (String.IsNullOrWhiteSpace(FixedMetricName))
                    return "(none)";
                //else if (FixedMetricName == "CPM" || FixedMetricName == "CPC")
                //    return String.Format("{0}: {1:C}", FixedMetricName, FixedMetricValue);
                else
                    return String.Format("{0}: {1:0.##########}", FixedMetricName, FixedMetricValue);
            }
        }
        [NotMapped]
        public decimal? SpendMultiplier
        {
            get { return FixedMetricName == "SpendMult" ? FixedMetricValue : null; }
        }
        [NotMapped]
        public decimal? FixedCPM
        {
            get { return FixedMetricName == "CPM" ? FixedMetricValue : null; }
        }
        [NotMapped]
        public decimal? FixedCPC
        {
            get { return FixedMetricName == "CPC" ? FixedMetricValue : null; }
        }

        //Usually there is just one AdRollProfile and one InsertionOrder per TDAccount
        public int? AdRollProfileId()
        {
            int? profileId = null;
            if (AdRollProfiles.Count > 0)
            {
                profileId = AdRollProfiles.First().Id;
            }
            return profileId;
        }
        public int? InsertionOrderID()
        {
            int? ioID = null;
            if (InsertionOrders.Count > 0)
            {
                ioID = InsertionOrders.First().InsertionOrderID;
            }
            return ioID;
        }

        public IEnumerable<int> AdvertiserIds()
        {
            if (UserProfiles == null)
                return new List<int>();
            var advIds = UserProfiles.Where(up => up.CakeAdvertiserId.HasValue).Select(up => up.CakeAdvertiserId.Value);
            return advIds;
        }
    }
}
