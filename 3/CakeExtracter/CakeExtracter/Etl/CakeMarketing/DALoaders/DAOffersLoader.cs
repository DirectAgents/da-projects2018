using System.Collections.Generic;
using System.Linq;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;
using MoreLinq;

namespace CakeExtracter.Etl.CakeMarketing.DALoaders
{
    public class DAOffersLoader : Loader<Offer>
    {
        private readonly bool loadInactive;

        public DAOffersLoader(bool loadInactive)
        {
            this.loadInactive = loadInactive;
        }

        protected override int Load(List<Offer> items)
        {
            int numInactiveSkipped = 0;
            int numInactiveUpdated = 0;
            Logger.Info("Synching {0} offers...", items.Count);
            using (var db = new DirectAgents.Domain.Contexts.DAContext())
            {
                var newVerticals = new List<DirectAgents.Domain.Entities.Cake.Vertical>();
                var newOfferTypes = new List<DirectAgents.Domain.Entities.Cake.OfferType>();
                var newOfferStatuses = new List<DirectAgents.Domain.Entities.Cake.OfferStatus>();

                foreach (var item in items)
                {
                    if (item.OfferStatus.OfferStatusId == DirectAgents.Domain.Entities.Cake.OfferStatus.Inactive && !loadInactive)
                    {
                        // See if already loaded. If so, update it. If not, skip.
                        if (db.Offers.Any(o => o.OfferId == item.OfferId))
                        {
                            numInactiveUpdated++;
                        }
                        else
                        {
                            numInactiveSkipped++;
                            continue;
                        }
                    }
                    var vertical = db.Verticals.Where(v => v.VerticalId == item.Vertical.VerticalId)
                        .SingleOrFallback(() =>
                        {
                            var newVertical = newVerticals.SingleOrDefault(v => v.VerticalId == item.Vertical.VerticalId);
                            if (newVertical == null)
                            {
                                newVertical = new DirectAgents.Domain.Entities.Cake.Vertical()
                                {
                                    VerticalId = item.Vertical.VerticalId,
                                    VerticalName = item.Vertical.VerticalName
                                };
                                newVerticals.Add(newVertical);
                            }
                            return newVertical;
                        });
                    var offerType = db.OfferTypes.Where(o => o.OfferTypeId == item.OfferType.OfferTypeId)
                        .SingleOrFallback(() =>
                        {
                            var newOfferType = newOfferTypes.SingleOrDefault(o => o.OfferTypeId == item.OfferType.OfferTypeId);
                            if (newOfferType == null)
                            {
                                newOfferType = new DirectAgents.Domain.Entities.Cake.OfferType()
                                {
                                    OfferTypeId = item.OfferType.OfferTypeId,
                                    OfferTypeName = item.OfferType.OfferTypeName
                                };
                                newOfferTypes.Add(newOfferType);
                            }
                            return newOfferType;
                        });
                    var offerStatus = db.OfferStatuses.Where(o => o.OfferStatusId == item.OfferStatus.OfferStatusId)
                        .SingleOrFallback(() =>
                        {
                            var newOfferStatus = newOfferStatuses.SingleOrDefault(o => o.OfferStatusId == item.OfferStatus.OfferStatusId);
                            if (newOfferStatus == null)
                            {
                                newOfferStatus = new DirectAgents.Domain.Entities.Cake.OfferStatus()
                                {
                                    OfferStatusId = item.OfferStatus.OfferStatusId,
                                    OfferStatusName = item.OfferStatus.OfferStatusName
                                };
                                newOfferStatuses.Add(newOfferStatus);
                            }
                            return newOfferStatus;
                        });

                    //TODO: check if advertiser exists??

                    var offer = db.Offers.Where(o => o.OfferId == item.OfferId)
                        .SingleOrFallback(() =>
                        {
                            var newOffer = new DirectAgents.Domain.Entities.Cake.Offer()
                            {
                                OfferId = item.OfferId,

                                //TODO: remove these? will be set below
                                Vertical = vertical,
                                OfferType = offerType,
                                OfferStatus = offerStatus
                            };
                            db.Offers.Add(newOffer);
                            return newOffer;
                        });

                    offer.Vertical = vertical;
                    offer.OfferType = offerType;
                    offer.OfferStatus = offerStatus;

                    offer.AdvertiserId = item.Advertiser.AdvertiserId;
                    offer.OfferName = item.OfferName;
                    offer.Hidden = item.Hidden;
                    offer.CurrencyAbbr = item.Currency.CurrencyAbbr;
                    offer.DefaultPriceFormatName = (from c in item.OfferContracts
                                                    where c.OfferContractId == item.DefaultOfferContractId
                                                    select c.PriceFormat.PriceFormatName).SingleOrDefault();
                    offer.DateCreated = item.DateCreated;

                    if (item.OfferContracts != null)
                    {
                        foreach (var ocInfo in item.OfferContracts)
                        {
                            var offerContract = db.OfferContracts.Where(x => x.OfferContractId == ocInfo.OfferContractId)
                                .SingleOrFallback(() =>
                                {
                                    var newOfferContract = new DirectAgents.Domain.Entities.Cake.OfferContract
                                    {
                                        OfferContractId = ocInfo.OfferContractId
                                    };
                                    db.OfferContracts.Add(newOfferContract);
                                    return newOfferContract;
                                });
                            offerContract.Offer = offer;
                            offerContract.PriceFormatName = ocInfo.PriceFormat.PriceFormatName;
                            offerContract.ReceivedAmount = ocInfo.Received.Amount;
                        }
                    }
                }

                if (numInactiveSkipped > 0 || numInactiveUpdated > 0)
                    Logger.Info("Inactive Offers: {0} skipped, {1} updated(already loaded)", numInactiveSkipped, numInactiveUpdated);
                Logger.Info("ChangeCount: " + db.ChangeCountsAsString());

                db.SaveChanges();
            }
            return items.Count;
        }
    }
}
