using System.Collections.Generic;
using System.Linq;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;
using MoreLinq;

namespace CakeExtracter.Etl.CakeMarketing.Loaders
{
    public class AdvertisersLoader : Loader<Advertiser>
    {
        private readonly bool includeContacts;

        public AdvertisersLoader(bool includeContacts)
        {
            this.includeContacts = includeContacts;
        }

        protected override int Load(List<Advertiser> items)
        {
            Logger.Info("Synching {0} advertisers...", items.Count);
            using (var db = new ClientPortal.Data.Contexts.ClientPortalContext())
            {
                var newCakeRoles = new List<ClientPortal.Data.Contexts.CakeRole>();

                foreach (var item in items)
                {
                    var advertiser = db.Advertisers
                        .Where(a => a.AdvertiserId == item.AdvertiserId)
                        .SingleOrFallback(() =>
                        {
                            var newAdvertiser = new ClientPortal.Data.Contexts.Advertiser
                            {
                                AdvertiserId = item.AdvertiserId,
                                Culture = "en-US",
                                ShowCPMRep = false,
                                ShowConversionData = false,
                                ConversionValueName = null,
                                ConversionValueIsNumber = false,
                                HasSearch = false
                            };
                            db.Advertisers.Add(newAdvertiser);
                            return newAdvertiser;
                        });

                    advertiser.AdvertiserName = item.AdvertiserName;

                    if (includeContacts)
                    {   // NOTE: The account manager contacts are all in the 'Direct Agents' advertiser
                        Logger.Info("Synching {0} contacts...", item.Contacts.Count);
                        foreach (var ci in item.Contacts)
                        {
                            var cakeRole = db.CakeRoles.Where(cr => cr.CakeRoleId == ci.Role.RoleId)
                                .SingleOrFallback(() =>
                                {
                                    var newCakeRole = newCakeRoles.SingleOrDefault(cr => cr.CakeRoleId == ci.Role.RoleId);
                                    if (newCakeRole == null)
                                    {
                                        Logger.Info("Adding new CakeRole: {0} ({1})", ci.Role.RoleName, ci.Role.RoleId);
                                        newCakeRole = new ClientPortal.Data.Contexts.CakeRole
                                        {
                                            CakeRoleId = ci.Role.RoleId,
                                            RoleName = ci.Role.RoleName
                                        };
                                        newCakeRoles.Add(newCakeRole);
                                    }
                                    return newCakeRole;
                                });

                            var cakeContact = db.CakeContacts
                                .Where(c => c.CakeContactId == ci.ContactId)
                                .SingleOrFallback(() =>
                                {
                                    var newCakeContact = new ClientPortal.Data.Contexts.CakeContact
                                    {
                                        CakeContactId = ci.ContactId,
                                        CakeRole = cakeRole
                                    };
                                    db.CakeContacts.Add(newCakeContact);
                                    return newCakeContact;
                                });

                            cakeContact.CakeRole = cakeRole;
                            cakeContact.FirstName = ci.FirstName;
                            cakeContact.LastName = ci.LastName;
                            cakeContact.EmailAddress = ci.EmailAddress;
                            cakeContact.Title = ci.Title;
                            cakeContact.PhoneWork = ci.PhoneWork;
                            cakeContact.PhoneCell = ci.PhoneCell;
                            cakeContact.PhoneFax = ci.PhoneFax;
                        }

                        if (item.AdvertiserId == 1) // Direct Agents -> do a save so that account manager contacts are in the db
                        {
                            Logger.Info("(DA found) Advertisers/CakeContacts/CakeRoles: " + db.ChangeCountsAsString());
                            db.SaveChanges();
                        }
                    }

                    if (item.AccountManagers != null && item.AccountManagers.Count > 0)
                    {
                        int accountManagerId = item.AccountManagers[0].ContactId;
                        var cakeContact = db.CakeContacts.SingleOrDefault(c => c.CakeContactId == accountManagerId);
                        if (cakeContact != null)
                            advertiser.AccountManagerId = cakeContact.CakeContactId;
                        else
                            Logger.Info("Advertiser {0}'s AccountManager (CakeContactId {1}) doesn't exist. Leaving AccountManagerId unchanged.", advertiser.AdvertiserId, accountManagerId);
                    }

                }
                Logger.Info("Advertisers/CakeContacts/CakeRoles: " + db.ChangeCountsAsString());
                db.SaveChanges();
            }
            return items.Count;
        }
    }
}
