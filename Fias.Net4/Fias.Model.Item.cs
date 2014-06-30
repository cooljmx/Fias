using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Fias.Net4
{
    [Serializable]
    public class FiasModelItem
    {
        [XmlAttribute]
        public string Id { get; set; }
        [XmlAttribute]
        public string KladrId { get; set; }
        [XmlAttribute]
        public string FullName { get; set; }
        [XmlAttribute]
        public string FormalName { get; set; }
        [XmlAttribute]
        public string ScName { get; set; }
        [XmlAttribute]
        public string ShortName { get; set; }
        [XmlAttribute]
        public string Level { get; set; }
        [XmlAttribute]
        public string IdDetail { get; set; }
        [XmlAttribute]
        public string LiveStatus { get; set; }
    }
}
