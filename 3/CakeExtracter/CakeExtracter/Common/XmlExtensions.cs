using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace CakeExtracter.Common
{
    public static class XmlExtensions
    {
        public static string ToXml<T>(this T[] arr)
        {
            var serializer = new XmlSerializer(typeof(T[]));
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                serializer.Serialize(sw, arr);
            }
            var result = sb.ToString();
            return result;
        }
    }
}
