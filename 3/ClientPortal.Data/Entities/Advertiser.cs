using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ClientPortal.Data.Contexts
{
    public partial class Advertiser
    {
        [NotMapped]
        public IOrderedEnumerable<AdvertiserContact> AdvertiserContactsOrdered
        {
            get { return this.AdvertiserContacts.OrderBy(ac => ac.Order); }
        }

        [NotMapped]
        public Contact AccountManagerContact
        {
            get
            {
                var advContacts = this.AdvertiserContactsOrdered.ToList();
                if (advContacts.Count >= 2)
                    return advContacts[1].Contact;
                else if (advContacts.Count == 1)
                    return advContacts[0].Contact;
                else
                    return null;
            }
        }

        [NotMapped]
        public IEnumerable<UserProfile> UserProfiles { get; set; }

        public DateTime GetNext_WeekStartDate(bool includeToday = false)
        {
            return Common.GetNext_WeekStartDate((DayOfWeek)this.StartDayOfWeek, includeToday);
        }
    }

    public class AdvertiserComparer : EqualityComparer<Advertiser>
    {
        public override bool Equals(Advertiser x, Advertiser y)
        {
            return x.AdvertiserId == y.AdvertiserId;
        }

        public override int GetHashCode(Advertiser obj)
        {
            return obj.AdvertiserId;
        }
    }
}
