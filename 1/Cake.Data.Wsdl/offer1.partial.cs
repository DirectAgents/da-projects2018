using System.Linq;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Xml;
using System.Collections.Generic;

namespace Cake.Data.Wsdl.ExportService
{
    public partial class offer1
    {
        public int StatusId
        {
            get
            {
                return this.offer_status.offer_status_id;
            }
        }

        public string StatusName
        {
            get
            {
                return this.offer_status.offer_status_name;
            }
        }

        public string AdvertiserId
        {
            get
            {
                return this.advertiser.advertiser_id.ToString();
            }
        }

        public string AllowedCountries
        {
            get
            {
                // Get a list of allowed countries
                var countryCodes = new List<string>();
                foreach (var contract in this.offer_contracts)
                {
                    if (contract.geo_targeting != null)
                    {
                        countryCodes.AddRange(
                            contract.geo_targeting.allowed_countries.Select(c => c.country.country_code)
                        );
                    }
                }
                // Convert the list to a CSV string
                string countryCodesCSV;
                if (countryCodes.Count > 0)
                {
                    countryCodesCSV = string.Join(",", countryCodes.ToArray());
                }
                else
                {
                    countryCodesCSV = "US"; // If there are no allowed countries, default to US
                }
                return countryCodesCSV;
            }
        }

        public string VerticalName
        {
            get
            {
                return this.vertical.vertical_name;
            }
        }

        public string DefaultPriceFormat
        {
            get
            {
                return this.offer_contracts[0].price_format.price_format_name;
            }
        }

        public string AllowedMediaTypeNames
        {
            get
            {
                return string.Join(",", this.allowed_media_types.Select(c => c.media_type_name));
            }
        }

        public string Xml
        {
            get
            {
                var serializer = new XmlSerializer(typeof(offer1));
                var xdoc = new XDocument();
                XmlWriter writer = xdoc.CreateWriter();
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("offer", "http://cakemarketing.com/api/3/");
                serializer.Serialize(writer, this, ns);
                writer.Close();
                string result = xdoc.ToString();
                return result;
            }
        }
    }
}
