using System;
using System.Xml.Serialization;

namespace Fias.Update.Lib.Mapping
{
    [Serializable]
    public class FiasFieldMap
    {
        [XmlAttribute]
        public string XmlName { get; set; }
        [XmlAttribute]
        public string DatabaseName { get; set; }

        private bool _isPrimaryKey = false;
        [XmlAttribute]
        public bool IsPrimaryKey { get { return _isPrimaryKey; } set { _isPrimaryKey = value; } }
    }    
}
