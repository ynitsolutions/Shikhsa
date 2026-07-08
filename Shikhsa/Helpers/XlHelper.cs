using System.IO;
using System.Xml.Serialization;

namespace Shikhsa.Helpers
{
    public static class XmlHelper
    {
        //public static string Serialize<T>(T obj)
        //{
        //    if (obj == null)
        //        return "";

        //    XmlSerializer serializer = new XmlSerializer(typeof(T));

        //    using StringWriter writer = new StringWriter();

        //    serializer.Serialize(writer, obj);

        //    return writer.ToString();
        //} 
        public static string Serialize<T>(T obj)
        {
            var serializer = new XmlSerializer(typeof(T));

            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, obj);

            string xml = stringWriter.ToString();

            xml = xml.Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");

            return xml;
        }
    }
}