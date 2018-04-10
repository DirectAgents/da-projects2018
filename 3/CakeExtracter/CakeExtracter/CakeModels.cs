using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace ClientPortal.Web.Models.Cake
{
    [WebServiceBinding(Name = "reportsSoap", Namespace = "http://cakemarketing.com/api/5/")]
    [XmlInclude(typeof(base_response))]
    public partial class reports : SoapHttpClientProtocol
    {
        public reports()
        {
            this.Url = "https://login.directagents.com/api/5/reports.asmx";
        }

        protected override System.Xml.XmlReader GetReaderForMessage(SoapClientMessage message, int bufferSize)
        {
            return base.GetReaderForMessage(message, bufferSize);
        }

        protected override System.Net.WebRequest GetWebRequest(Uri uri)
        {
            return base.GetWebRequest(uri);
        }

        protected override System.Net.WebResponse GetWebResponse(System.Net.WebRequest request)
        {
            return base.GetWebResponse(request);
        }

        protected override System.Xml.XmlWriter GetWriterForMessage(SoapClientMessage message, int bufferSize)
        {
            return base.GetWriterForMessage(message, bufferSize);
        }

        [SoapDocumentMethod("http://cakemarketing.com/api/5/Conversions", 
            RequestNamespace = "http://cakemarketing.com/api/5/", 
            ResponseNamespace = "http://cakemarketing.com/api/5/", 
            Use = SoapBindingUse.Literal, 
            ParameterStyle = SoapParameterStyle.Wrapped)
        ]
        public conversion_report_response Conversions(
                                                string api_key,
                                                System.DateTime start_date,
                                                System.DateTime end_date,
                                                int affiliate_id,
                                                int advertiser_id,
                                                int offer_id,
                                                int campaign_id,
                                                int creative_id,
                                                bool include_tests,
                                                int start_at_row,
                                                int row_limit,
                                                ConversionsSortFields sort_field,
                                                bool sort_descending)
        {
            object[] results = this.Invoke("Conversions", new object[] {
                        api_key,
                        start_date,
                        end_date,
                        affiliate_id,
                        advertiser_id,
                        offer_id,
                        campaign_id,
                        creative_id,
                        include_tests,
                        start_at_row,
                        row_limit,
                        sort_field,
                        sort_descending});
            return ((conversion_report_response)(results[0]));
        }

        [SoapDocumentMethod("http://cakemarketing.com/api/5/ConversionChanges", 
            RequestNamespace = "http://cakemarketing.com/api/5/", 
            ResponseNamespace = "http://cakemarketing.com/api/5/", 
            Use = SoapBindingUse.Literal, 
            ParameterStyle = SoapParameterStyle.Wrapped)
        ]
        public conversion_report_response ConversionChanges(
                                                string api_key,
                                                System.DateTime changes_since,
                                                bool include_new_conversions,
                                                int affiliate_id,
                                                int advertiser_id,
                                                int offer_id,
                                                int campaign_id,
                                                int creative_id,
                                                bool include_tests,
                                                int start_at_row,
                                                int row_limit,
                                                ConversionsSortFields sort_field,
                                                bool sort_descending)
        {
            object[] results = this.Invoke("ConversionChanges", new object[] {
                        api_key,
                        changes_since,
                        include_new_conversions,
                        affiliate_id,
                        advertiser_id,
                        offer_id,
                        campaign_id,
                        creative_id,
                        include_tests,
                        start_at_row,
                        row_limit,
                        sort_field,
                        sort_descending});
            return ((conversion_report_response)(results[0]));
        }

        [SoapDocumentMethod("http://cakemarketing.com/api/5/Clicks", 
            RequestNamespace = "http://cakemarketing.com/api/5/", 
            ResponseNamespace = "http://cakemarketing.com/api/5/", 
            Use = SoapBindingUse.Literal, 
            ParameterStyle = SoapParameterStyle.Wrapped)
        ]
        public click_report_response Clicks(
                                        string api_key,
                                        System.DateTime start_date,
                                        System.DateTime end_date,
                                        int affiliate_id,
                                        int advertiser_id,
                                        int offer_id,
                                        int campaign_id,
                                        int creative_id,
                                        bool include_tests,
                                        int start_at_row,
                                        int row_limit)
        {
            object[] results = this.Invoke("Clicks", new object[] {
                        api_key,
                        start_date,
                        end_date,
                        affiliate_id,
                        advertiser_id,
                        offer_id,
                        campaign_id,
                        creative_id,
                        include_tests,
                        start_at_row,
                        row_limit});
            return ((click_report_response)(results[0]));
        }
    }

    [Serializable]
    [XmlType(Namespace = "http://cakemarketing.com/api/5/")]
    public enum ConversionsSortFields
    {
        conversion_id,
        visitor_id,
        request_session_id,
        click_id,
        conversion_date,
        transaction_id,
        last_updated,
    }

    [Serializable]
    [XmlType(Namespace = "http://cakemarketing.com/api/5/")]
    public partial class conversion_report_response : get_response
    {
        public conversion[] conversions;
    }

    [Serializable]
    [XmlType(Namespace = "http://cakemarketing.com/api/5/")]
    [Table("Conversion")]
    public partial class conversion
    {
        public conversion()
        {
            disposition = new disposition 
            { 
                approved = false,
                contact = string.Empty, 
                disposition_date = null, 
                disposition_name = string.Empty 
            };
        }

        [Key]
        public string conversion_id { get; set; }

        public int visitor_id { get; set; }

        public int request_session_id { get; set; }

        [XmlElement(IsNullable = true)]
        public int? click_id { get; set; }

        public DateTime conversion_date { get; set; }

        [XmlElement(IsNullable = true)]
        public DateTime? last_updated { get; set; }

        [XmlElement(IsNullable = true)]
        public DateTime? click_date { get; set; }

        public affiliate affiliate { get; set; }

        public advertiser advertiser { get; set; }

        public offer offer { get; set; }

        public int campaign_id { get; set; }

        public creative creative { get; set; }

        public string sub_id_1 { get; set; }

        public string sub_id_2 { get; set; }

        public string sub_id_3 { get; set; }

        public string sub_id_4 { get; set; }

        public string sub_id_5 { get; set; }

        public string conversion_type { get; set; }

        public payment paid { get; set; }

        public payment received { get; set; }

        public byte step_reached { get; set; }

        public bool pixel_dropped { get; set; }

        public bool suppressed { get; set; }

        public bool returned { get; set; }

        public bool test { get; set; }

        public string transaction_id { get; set; }

        public string conversion_ip_address { get; set; }

        public string click_ip_address { get; set; }

        public string country { get; set; }

        public string conversion_referrer_url { get; set; }

        public string click_referrer_url { get; set; }

        public string conversion_user_agent { get; set; }

        public string click_user_agent { get; set; }

        public disposition disposition { get; set; }

        public string note { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "API:id_name_store")]
    public partial class affiliate
    {
        public int affiliate_id { get; set; }

        public string affiliate_name { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://cakemarketing.com/api/5/")]
    public partial class browser
    {
        public int browser_id { get; set; }

        public string browser_name { get; set; }

        public version browser_version;
        public int browser_version_version_id { get { return browser_version.version_id; } }
        public string browser_version_version_name { get { return browser_version.version_name; } }

        public version browser_version_minor;
        public int browser_version_minor_version_id { get { return browser_version_minor.version_id; } }
        public string browser_version_minor_version_name { get { return browser_version_minor.version_name; } }
    }

    [Serializable]
    [XmlType(Namespace = "API:id_name_store")]
    public partial class version
    {
        public short version_id { get; set; }

        public string version_name { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://cakemarketing.com/api/5/")]
    public partial class operating_system
    {

        public int operating_system_id;

        public string operating_system_name { get; set; }

        public version operating_system_version;
        public int operating_system_version_version_id { get { return operating_system_version.version_id; } }
        public string operating_system_version_version_name { get { return operating_system_version.version_name; } }

        public version operating_system_version_minor;
        public int operating_system_version_minor_version_id { get { return operating_system_version_minor.version_id; } }
        public string operating_system_version_minor_version_name { get { return operating_system_version_minor.version_name; } }
    }

    [Serializable]
    [XmlType(Namespace = "API:id_name_store")]
    public partial class device
    {
        public short device_id { get; set; }

        public string device_name { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "API:id_name_store")]
    public partial class isp
    {
        public int isp_id { get; set; }

        public string isp_name { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "API:id_name_store")]
    public partial class language
    {
        public byte language_id { get; set; }

        public string language_name { get; set; }

        public string language_abbr { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "API:id_name_store")]
    public partial class region
    {
        public string region_code { get; set; }

        public string region_name { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "API:id_name_store")]
    public partial class country
    {
        public string country_code { get; set; }

        public string country_name { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://cakemarketing.com/api/5/")]
    [Table("Click")]
    public partial class click
    {
        public click()
        {
            device = new device
            {
                device_id = 0,
                device_name = string.Empty
            };
            language = new language
            {
                language_abbr = string.Empty,
                language_id = 0,
                language_name = string.Empty
            };
            operating_system = new operating_system
            {
                operating_system_id = 0,
                operating_system_name = string.Empty,
                operating_system_version = new version 
                { 
                    version_id = 0, 
                    version_name = ""
                }
            };
            browser = new browser
            {
                browser_id = 0,
                browser_name = string.Empty,
                browser_version = new version
                {
                    version_id = 0, 
                    version_name = ""
                },
                browser_version_minor = new version 
                { 
                    version_id = 0, 
                    version_name = "" 
                }
            };
            region = new region
            {
                region_code = string.Empty,
                region_name = string.Empty
            };
            isp = new isp
            {
                isp_id = 0,
                isp_name = string.Empty
            };
            country = new country
            {
                country_code = string.Empty,
                country_name = string.Empty
            };
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int click_id { get; set; }

        public int visitor_id { get; set; }

        public int request_session_id { get; set; }

        public DateTime click_date { get; set; }

        public affiliate affiliate { get; set; }

        public advertiser advertiser { get; set; }

        public offer offer { get; set; }

        public int campaign_id { get; set; }

        public creative creative { get; set; }

        public string sub_id_1 { get; set; }

        public string sub_id_2 { get; set; }

        public string sub_id_3 { get; set; }

        public string sub_id_4 { get; set; }

        public string sub_id_5 { get; set; }

        public string ip_address { get; set; }

        public string user_agent { get; set; }

        public string referrer_url { get; set; }

        public string request_url { get; set; }

        public string redirect_url { get; set; }

        public country country { get; set; }

        public region region { get; set; }

        public language language { get; set; }

        public isp isp { get; set; }

        public device device { get; set; }

        public operating_system operating_system { get; set; }

        public browser browser { get; set; }

        public string disposition { get; set; }

        public string paid_action { get; set; }

        public int total_clicks { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "API:id_name_store")]
    public partial class advertiser
    {
        public int advertiser_id { get; set; }

        public string advertiser_name { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "API:id_name_store")]
    public partial class offer
    {
        public int offer_id { get; set; }

        public string offer_name { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "API:id_name_store")]
    public partial class creative
    {
        public int creative_id { get; set; }

        public string creative_name { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://cakemarketing.com/api/5/")]
    public partial class disposition
    {
        public bool approved { get; set; }

        public string disposition_name { get; set; }

        public string contact { get; set; }

        public DateTime? disposition_date { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://cakemarketing.com/api/5/")]
    public partial class payment
    {
        public byte currency_id { get; set; }

        public decimal amount { get; set; }

        public string formatted_amount { get; set; }
    }

    [XmlInclude(typeof(get_response))]
    [XmlInclude(typeof(click_report_response))]
    [XmlInclude(typeof(conversion_report_response))]
    [Serializable]
    [XmlType(Namespace = "http://cakemarketing.com/api/5/")]
    public partial class base_response
    {
        public bool success;

        public string message;
    }

    [XmlInclude(typeof(click_report_response))]
    [XmlInclude(typeof(conversion_report_response))]
    [Serializable]
    [XmlType(Namespace = "http://cakemarketing.com/api/5/")]
    public partial class get_response : base_response
    {
        public int row_count;
    }

    [Serializable]
    [XmlType(Namespace = "http://cakemarketing.com/api/5/")]
    public partial class click_report_response : get_response
    {
        public click[] clicks;
    }
}