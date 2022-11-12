using System.Xml.Serialization;

namespace OpenWorld.Server.Data
{
    public class ModMetaData
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("packageId")]
        public string PackageId { get; set; }
    }
}
