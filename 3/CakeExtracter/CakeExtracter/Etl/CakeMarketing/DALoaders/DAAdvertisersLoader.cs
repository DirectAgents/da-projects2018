using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CakeExtracter.Common;
using DirectAgents.Domain.Contexts;
using DirectAgents.Domain.Entities.Cake;
using MoreLinq;

namespace CakeExtracter.Etl.CakeMarketing.DALoaders
{
    public class DAAdvertisersLoader : Loader<CakeExtracter.CakeMarketingApi.Entities.Advertiser>
    {
        private readonly bool includeContacts;

        public DAAdvertisersLoader(bool includeContacts)
        {
            this.includeContacts = includeContacts;
        }

        protected override int Load(List<CakeExtracter.CakeMarketingApi.Entities.Advertiser> items)
        {
            Logger.Info("Synching {0} advertisers...", items.Count);
            using (var db = new DAContext())
            {
                var newRoles = new List<Role>();

                foreach (var item in items)
                {
                    var advertiser = db.Set<Advertiser>().Find(item.AdvertiserId);
                    if (advertiser == null)
                    {
                        advertiser = new Advertiser
                        {
                            AdvertiserId = item.AdvertiserId
                        };
                        db.Advertisers.Add(advertiser);
                    }
                    else // Advertiser found (in cache or db)
                    {
                        var entry = db.Entry(advertiser);
                        if (entry.State != EntityState.Unchanged)
                        {
                            Logger.Warn("Encountered duplicate Advertiser {0} - skipping", item.AdvertiserId);
                            continue;
                        }
                    }

                    advertiser.AdvertiserName = item.AdvertiserName;

                    if (includeContacts)
                    {   // NOTE: The account manager contacts are all in the 'Direct Agents' advertiser
                        Logger.Info("Synching {0} contacts...", item.Contacts.Count);
                        foreach (var ci in item.Contacts)
                        {
                            var role = db.Roles.Where(r => r.RoleId == ci.Role.RoleId)
                                .SingleOrFallback(() =>
                                {
                                    var newRole = newRoles.SingleOrDefault(r => r.RoleId == ci.Role.RoleId);
                                    if (newRole == null)
                                    {
                                        Logger.Info("Adding new Role: {0} ({1})", ci.Role.RoleName, ci.Role.RoleId);
                                        newRole = new Role
                                        {
                                            RoleId = ci.Role.RoleId,
                                            RoleName = ci.Role.RoleName
                                        };
                                        newRoles.Add(newRole);
                                    }
                                    return newRole;
                                });

                            var contact = db.Contacts
                                .Where(c => c.ContactId == ci.ContactId)
                                .SingleOrFallback(() =>
                                {
                                    var newContact = new Contact
                                    {
                                        ContactId = ci.ContactId,
                                        Role = role
                                    };
                                    db.Contacts.Add(newContact);
                                    return newContact;
                                });

                            contact.Role = role;
                            contact.FirstName = ci.FirstName;
                            contact.LastName = ci.LastName;
                            contact.EmailAddress = ci.EmailAddress;
                            contact.Title = ci.Title;
                            contact.PhoneWork = ci.PhoneWork;
                            contact.PhoneCell = ci.PhoneCell;
                            contact.PhoneFax = ci.PhoneFax;
                        }

                        if (item.AdvertiserId == 1) // Direct Agents -> do a save so that account manager contacts are in the db
                        {
                            Logger.Info("(DA found) Advertisers/Contacts/Roles: " + db.ChangeCountsAsString());
                            db.SaveChanges();
                        }
                    } // end if (includeContacts)

                    // Set AccountManager
                    if (item.AccountManagers != null && item.AccountManagers.Count > 0)
                    {
                        int accountManagerId = item.AccountManagers[0].ContactId;
                        var contact = db.Contacts.SingleOrDefault(c => c.ContactId == accountManagerId);
                        if (contact != null)
                            advertiser.AccountManagerId = contact.ContactId;
                        else
                            Logger.Info("Advertiser {0}'s AccountManager (ContactId {1}) doesn't exist. Leaving AccountManagerId unchanged.", advertiser.AdvertiserId, accountManagerId);
                    }

                    // Set AdManager
                    if (item.Tags != null && item.Tags.Count > 0)
                    {
                        var tag = item.Tags[0];
                        var tagNames = tag.TagName.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        if (tagNames.Count() >= 2)
                        {
                            string firstName = tagNames[0];
                            string lastName = tagNames[1];
                            var contact = db.Contacts.FirstOrDefault(c => c.FirstName == firstName && c.LastName == lastName);
                            if (contact != null)
                                advertiser.AdManagerId = contact.ContactId;
                            else
                                Logger.Info("Advertiser {0}'s AdManager ({1}) doesn't exist. Leaving AdManagerId unchanged.", advertiser.AdvertiserId, tag.TagName);
                        }
                    }
                }
                Logger.Info("Advertisers/Contacts/Roles: " + db.ChangeCountsAsString());
                db.SaveChanges();
            }
            return items.Count;
        }
    }
}
