using System.Xml.Linq;
using System.Xml.Serialization;
using Cake.Data.Wsdl.ExportService;

namespace Cake.Model.Staging
{
    public partial class CakeOffer
    {
        public static explicit operator offer1(CakeOffer cakeOffer)
        {
            if (string.IsNullOrWhiteSpace(cakeOffer.Xml))
                return null;

            var serializer = new XmlSerializer(typeof(offer1));
            var xdoc = XDocument.Parse(cakeOffer.Xml);
            var reader = xdoc.CreateReader();
            offer1 result = (offer1)serializer.Deserialize(reader);
            return result;
        }
    }
}
