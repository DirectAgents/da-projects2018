using System.Collections.Generic;
using System.IO;
using System.Text;
using CakeExtracter.CakeMarketingApi.Entities;

namespace CakeExtracter.Etl.CakeMarketing.Loaders
{
    public class ClicksLoader2 : Loader<Click>
    {
        StringBuilder sb = new StringBuilder();

        protected override int Load(List<Click> clicks)
        {
            Logger.Info("{0} clicks", clicks.Count);
            foreach (var source in clicks)
            {
                source.Region = source.Region ?? new Region
                {
                    RegionCode = "unknown",
                    RegionName = "unknown"
                };
                source.Region.RegionCode = string.IsNullOrWhiteSpace(source.Region.RegionCode)
                                                ? "unknown"
                                                : source.Region.RegionCode;
                source.Device = source.Device ?? new Device
                {
                    DeviceId = 0,
                    DeviceName = "unknown"
                };
                source.Browser = source.Browser ?? new Browser
                {
                    BrowserId = 0,
                    BrowserName = "unknown"
                };
                sb.AppendFormat("{0}\t", source.ClickId);
                sb.AppendFormat("{0}\t", source.TotalClicks);
                sb.AppendFormat("{0}\t", source.ClickDate.Date);
                sb.AppendFormat("{0}\t", source.Affiliate.AffiliateId);
                sb.AppendFormat("{0}\t", source.Affiliate.AffiliateName);
                sb.AppendFormat("{0}\t", source.Advertiser.AdvertiserId);
                sb.AppendFormat("{0}\t", source.Advertiser.AdvertiserName);
                sb.AppendFormat("{0}\t", source.Offer.OfferId);
                sb.AppendFormat("{0}\t", source.Offer.OfferName);
                //sb.AppendFormat("{0}\t", source.Offer.DefaultOfferContractId);
                //sb.AppendFormat("{0}\t", source.Offer.OfferContracts[0].PriceFormat.PriceFormatId);
                sb.AppendFormat("{0}\t", "1");

                //sb.AppendFormat("{0}\t", source.Offer.OfferContracts[0].PriceFormat.PriceFormatName);
                sb.AppendFormat("{0}\t", "CPA");

                //sb.AppendFormat("{0}\t", source.Offer.Currency.CurrencyAbbr);
                sb.AppendFormat("{0}\t", "USD");

                sb.AppendFormat("{0}\t", source.Country.CountryCode);
                sb.AppendFormat("{0}\t", source.Country.CountryName);
                sb.AppendFormat("{0}\t", source.Region.RegionCode);
                sb.AppendFormat("{0}\t", source.Region.RegionName);
                sb.AppendFormat("{0}\t", source.Device.DeviceId);
                sb.AppendFormat("{0}\t", source.Device.DeviceName);
                sb.AppendFormat("{0}\t", source.Browser.BrowserId);
                sb.AppendFormat("{0}\t", source.Browser.BrowserName);

                sb.AppendLine();
            };

            return clicks.Count;
        }

        protected override void AfterLoad()
        {
            Logger.Info("writing clicks file..");
            File.WriteAllText("c:\\scratch\\clicks.txt", this.sb.ToString());
        }
    }

}
