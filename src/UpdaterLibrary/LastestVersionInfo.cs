using System.IO;
using System.Xml.Serialization;

namespace UpdaterLibrary
{
    public class LastestVersionInfo
    {
        public string Version { get; set; }
        public string LinkDownloadZipFile { get; set; }

        public static LastestVersionInfo LoadFromXml(string xml)
        {
            using (var stringReader = new StringReader(xml))
            {
                var xmlSerializer = new XmlSerializer(typeof(LastestVersionInfo));
                var lastestInfo = xmlSerializer.Deserialize(stringReader) as LastestVersionInfo;
                return lastestInfo;
            }
        }

        public string SaveAsXml()
        {
            using (var ms = new StringWriter())
            {
                var xmlSerializer = new XmlSerializer(typeof(LastestVersionInfo));
                xmlSerializer.Serialize(ms, this);
                var xml = ms.ToString();
                return xml;
            }

        }
    }
}
