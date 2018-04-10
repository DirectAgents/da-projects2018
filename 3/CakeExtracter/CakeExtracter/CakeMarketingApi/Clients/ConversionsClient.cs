using System.Globalization;
using System.Text.RegularExpressions;
using CakeExtracter.CakeMarketingApi.Entities;
using RestSharp;
using RestSharp.Deserializers;

namespace CakeExtracter.CakeMarketingApi.Clients
{
    public class ConversionsClient : ApiClient
    {
        public ConversionsClient()
            : base(8, "reports", "Conversions")
        {
        }

        public ConversionReportResponse Conversions(ConversionsRequest request)
        {
            var result = Execute<ConversionReportResponse>(request, new ConversionsDeserializer());
            return result;
        }
    }

    public class ConversionsDeserializer : IDeserializer
    {
        private XmlDeserializer restSharpXmlDeserializer;

        public ConversionsDeserializer()
        {
            Culture = CultureInfo.InvariantCulture;
            restSharpXmlDeserializer = new XmlDeserializer();
        }

        public T Deserialize<T>(IRestResponse response)
        {
            //TODO: find a better way to do this
            response.Content = response.Content.Replace(@"xsi:nil=""true""", "").Replace(@"&#x16;", "").Replace(@"&#x13;", "");
            //response.Content = RemoveInvalidXMLChars(response.Content);
            return restSharpXmlDeserializer.Deserialize<T>(response);
        }

        public string RootElement { get; set; }

        public string Namespace { get; set; }

        public string DateFormat { get; set; }

        public CultureInfo Culture { get; set; }


        //NOT USED:

        // filters control characters but allows only properly-formed surrogate sequences
        private static Regex _invalidXMLChars = new Regex(
            @"(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|[\x00-\x08\x0B\x0C\x0E-\x1F\x7F-\x9F\uFEFF\uFFFE\uFFFF]",
            RegexOptions.Compiled);

        /// <summary>
        /// removes any unusual unicode characters that can't be encoded into XML
        /// </summary>
        public static string RemoveInvalidXMLChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return _invalidXMLChars.Replace(text, "");
        }
    }
}