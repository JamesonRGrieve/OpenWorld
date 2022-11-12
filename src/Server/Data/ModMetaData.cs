using System.Xml.Serialization;

namespace OpenWorldServer.Data
{
    public class ModMetaData
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("packageId")]
        public string PackageId { get; set; }
    }
}
