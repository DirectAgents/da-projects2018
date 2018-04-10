using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cake.Data.Wsdl.ExportService;
using Cake.Data.Wsdl.ReportsService;

namespace Cake.Data.Wsdl
{
    public class CakeService : ICakeService
    {
        private string apiKey;
        private int defaultRowLimit = 0;

        public CakeService(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public Cake.Data.Wsdl.SignupService.MediaType[] ExportMediaTypes()
        {
            Cake.Data.Wsdl.SignupService.MediaType[] result = null;
            var service = new Cake.Data.Wsdl.SignupService.signupSoapClient("signupSoap");
            var response = service.GetMediaTypes(this.apiKey);
            return result;
        }

        public Cake.Data.Wsdl.ExportService.advertiser[] ExportAdvertisers()
        {
            Cake.Data.Wsdl.ExportService.advertiser[] result = null;
            var service = new exportSoapClient("exportSoap");
            var response = service.Advertisers(
                                    api_key: this.apiKey,
                                    advertiser_id: 0,
                                    advertiser_name: null,
                                    account_manager_id: 0,
                                    tag_id: 0,
                                    start_at_row: 0,
                                    row_limit: 0,
                                    sort_field: AdvertisersSortFields.advertiser_id,
                                    sort_descending: false);
            if (response.success)
            {
                result = response.advertisers;
            }
            else
            {
                throw new Exception("Error exporting advertisers: " + response.message);
            }
            return result;
        }

        public Cake.Data.Wsdl.ExportService.affiliate[] ExportAffiliates()
        {
            Cake.Data.Wsdl.ExportService.affiliate[] result = null;
            var service = new exportSoapClient("exportSoap");
            var response = service.Affiliates(
                                api_key: this.apiKey,
                                affiliate_id: 0,
                                affiliate_name: null,
                                account_manager_id: 0,
                                tag_id: 0,
                                start_at_row: 0,
                                row_limit: 0,
                                sort_field: AffiliatesSortFields.affiliate_id,
                                sort_descending: false);
            if (response.success)
            {
                result = response.affiliates;
            }
            else
            {
                throw new Exception("Error exporting affiliates: " + response.message);
            }
            return result;
        }

        public offer1[] ExportOffers()
        {
            offer1[] result = null;

            var service = new exportSoapClient("exportSoap");

            var response = service.Offers(
                                    api_key: this.apiKey,
                                    offer_id: 0,
                                    offer_name: null,
                                    advertiser_id: 0,
                                    vertical_id: 0,
                                    offer_type_id: 0,
                                    media_type_id: 0,
                                    offer_status_id: 0,
                                    tag_id: 0,
                                    start_at_row: 0,
                                    row_limit: this.defaultRowLimit,
                                    sort_field: OffersSortFields.offer_id,
                                    sort_descending: false);

            if (response.success)
            {
                result = response.offers;
            }
            else
            {
                throw new Exception("Error exporting affiliates: " + response.message);
            }

            return result;
        }

        public IEnumerable<campaign[]> ExportCampaigns()
        {
            var service = new exportSoapClient("exportSoap");

            object locker = new object();

            var results = new List<campaign[]>();

            Parallel.ForEach(this.ExportOffers(), offer =>
            {
                Console.WriteLine("Getting campaigns for offer " + offer.offer_id);

                var response = service.Campaigns(
                                    api_key: this.apiKey,
                                    campaign_id: 0,
                                    offer_id: offer.offer_id,
                                    affiliate_id: 0,
                                    account_status_id: 0,
                                    media_type_id: 0,
                                    start_at_row: 0,
                                    row_limit: 0,
                                    sort_field: CampaignsSortFields.campaign_id,
                                    sort_descending: false);

                if (response.success)
                {
                    lock (locker)
                    {
                        results.Add(response.campaigns);

                        Console.WriteLine("Got campaigns for offer " + offer.offer_id);
                    }
                }
                else
                {
                    throw new Exception("Error exporting affiliates: " + response.message);
                }
            });

            foreach (var result in results)
            {
                yield return result;
            }
        }

        public conversion[] Conversions()
        {
            var service = new ReportsService.reports();
            DateTime from = new DateTime(2012, 5, 1); // TODO: unhardcode
            DateTime to = new DateTime(2012, 6, 1); // TODO: unhardcode

            var result = service.Conversions(
                                        api_key: this.apiKey,
                                        start_date: from,
                                        end_date: to,
                                        affiliate_id: 0,
                                        advertiser_id: 0,
                                        offer_id: 0,
                                        campaign_id: 0,
                                        creative_id: 0,
                                        include_tests: false,
                                        start_at_row: 0,
                                        row_limit: 0,
                                        sort_field: ConversionsSortFields.conversion_date,
                                        sort_descending: false);

            return result.conversions;
        }
    }
}
