using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using CakeExtracter.CakeMarketingApi.Entities;
using CakeExtracter.Common;

namespace CakeExtracter.Etl.CakeMarketing.Loaders
{
    public class ConversionsLoader : Loader<Conversion>
    {
        protected override int Load(List<Conversion> items)
        {
            int loadedToDW = LoadToDataWarehouse(items);

            int loadedToCP = LoadToClientPortal(items);

            if (loadedToCP != loadedToDW)
                throw new Exception("Number loaded should be equal: {0} datawarehouse,  {1} clientportal");

            return loadedToDW;
        }

        private int LoadToClientPortal(List<Conversion> items)
        {
            Logger.Info("Loading Conversions to Client Portal..");

            var loaded = 0;
            var added = 0;
            var updated = 0;
            using (var db = new ClientPortal.Data.Contexts.ClientPortalContext())
            {
                foreach (var source in items)
                {
                    var target = db.Conversions.Find(source.ConversionId.ToString());

                    if (target == null)
                    {
                        target = new ClientPortal.Data.Contexts.Conversion
                        {
                            conversion_id = source.ConversionId.ToString()
                        };

                        db.Conversions.Add(target);
                        added++;
                    }
                    else
                    {
                        updated++;
                    }

                    target.conversion_date = source.ConversionDate;
                    target.affiliate_id = source.Affiliate.AffiliateId;
                    target.advertiser_id = source.Advertiser.AdvertiserId;
                    target.offer_id = source.Offer.OfferId;
                    target.received_currency_id = (byte)source.Received.CurrencyId;
                    target.received_amount = source.Received.Amount;
                    target.transaction_id = source.TransactionId;

                    loaded++;
                }

                Logger.Info("Loading {0} Conversions (Client Portal) ({1} updates, {2} additions)..", loaded, updated, added);

                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var entityValidationError in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityValidationError.ValidationErrors)
                        {
                            Logger.Warn("entity validation error for property {0}: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }

                    Logger.Error(ex);

                    throw;
                }
            }

            return loaded;
        }

        private int LoadToDataWarehouse(List<Conversion> conversionsFromSource)
        {
            Logger.Info("Loading Conversions to Data Warehouse..");

            using (var db = new ClientPortal.Data.Contexts.ClientPortalDWContext())
            {
                foreach (var conversionFromSource in conversionsFromSource)
                {
                    var factConversion = db.FactConversions.Find(conversionFromSource.ConversionId);

                    if (factConversion == null)
                    {
                        // Skip if conversion has no click
                        if (conversionFromSource.ClickId == 0)
                        {
                            Logger.Warn("Conversion id {0} has no click, skipping.", conversionFromSource.ConversionId);
                            continue;
                        }

                        factConversion = new ClientPortal.Data.Contexts.FactConversion();
                        factConversion.ConversionKey = conversionFromSource.ConversionId;

                        // DateKey
                        factConversion.DateKey = conversionFromSource.ConversionDate.Date;

                        // ClickDateKey
                        if (conversionFromSource.ClickDate != null)
                        {
                            factConversion.ClickDateKey = conversionFromSource.ClickDate.Date;
                        }

                        // ClickKey, if it exists
                        var factClick = db.FactClicks.Find(conversionFromSource.ClickId);
                        if (factClick == null) // this means we didn't synch up the clicks far back enough
                        {
                            Logger.Warn("No click id {0} to go with conversion id {1}", conversionFromSource.ClickId, factConversion.ConversionKey);
                        }
                        else
                        {
                            factConversion.ClickKey = factClick.ClickKey;
                        }

                        db.FactConversions.Add(factConversion);
                    }
                }
                Logger.Info(db.ChangeCountsAsString());
                try
                {
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var entityValidationError in ex.EntityValidationErrors)
                        foreach (var validationError in entityValidationError.ValidationErrors)
                            Logger.Warn("entity validation error for property {0}: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    Logger.Error(ex);
                    throw;
                }
            }
            return conversionsFromSource.Count;
        }
    }
}
