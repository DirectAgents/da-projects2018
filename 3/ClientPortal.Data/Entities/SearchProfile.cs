using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ClientPortal.Data.Contexts
{
    public class SearchProfile_Validation
    {
        [Range(0, 6)]
        public int StartDayOfWeek { get; set; }
    }

    [MetadataType(typeof(SearchProfile_Validation))]
    public partial class SearchProfile
    {
        [NotMapped]
        public decimal RevPerViewThru
        {
            get { return 20; } //TODO: put col in DB + admin
        }

        [NotMapped]
        public bool UseAnalytics // Google Analytics, that is
        {
            get { return false; } //TODO: move column from Advertiser to SearchProfile
        }

        [NotMapped]
        public bool ShowCalls
        {
            get { return !String.IsNullOrWhiteSpace(this.LCaccid); }
        }

        [NotMapped]
        public IOrderedEnumerable<SearchProfileContact> SearchProfileContactsOrdered
        {
            get { return this.SearchProfileContacts.OrderBy(sc => sc.Order); }
        }

        [NotMapped]
        public IEnumerable<SearchAccount> GoogleSearchAccounts
        {
            get { return this.SearchAccounts == null ? null : this.SearchAccounts.Where(sa => sa.Channel == SearchAccount.GoogleChannel); }
        }
        [NotMapped]
        public IEnumerable<SearchAccount> BingSearchAccounts
        {
            get { return this.SearchAccounts == null ? null : this.SearchAccounts.Where(sa => sa.Channel == SearchAccount.BingChannel); }
        }
        [NotMapped]
        public IEnumerable<SearchAccount> AppleSearchAccounts
        {
            get { return this.SearchAccounts == null ? null : this.SearchAccounts.Where(sa => sa.Channel == SearchAccount.AppleChannel); }
        }
        [NotMapped]
        public IEnumerable<SearchAccount> CriteoSearchAccounts
        {
            get { return this.SearchAccounts == null ? null : this.SearchAccounts.Where(sa => sa.Channel == SearchAccount.CriteoChannel); }
        }

        public DateTime GetNext_WeekStartDate(bool includeToday = false)
        {
            return Common.GetNext_WeekStartDate((DayOfWeek)this.StartDayOfWeek, includeToday);
        }

        public DateTime GetLast_WeekEndDate(bool includeToday = false)
        {
            return Common.GetLast_WeekEndDate((DayOfWeek)this.StartDayOfWeek, includeToday);
        }

        public bool HasChannelWithMultipleSearchAccounts()
        {
            var channelGroups = this.SearchAccounts.GroupBy(sa => sa.Channel);
            return channelGroups.Any(g => g.Count() > 1);
        }
    }
}
