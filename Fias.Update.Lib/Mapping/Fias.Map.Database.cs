using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

namespace Fias.Update.Lib.Mapping
{
    [Serializable]
    public class FiasDatabaseMap
    {
        private HashSet<string> _tableSet = null;
        private List<FiasTableMap> _tables = new List<FiasTableMap>();
        public List<FiasTableMap> Tables { get { return _tables; } }

        public FiasTableMap GetTable(string ADescription)
        {
            return _tables.Where(a => a.XmlDescription == ADescription).FirstOrDefault();
        }

        public bool TableExists(string AXmlName)
        {
            if (_tableSet == null)
                _tableSet = new HashSet<string>(_tables.Select(a=>a.XmlName.ToUpper()));
            return _tableSet.Contains(AXmlName.ToUpper());
        }
        
        public void SaveToFile(string AFileName)
        {
            using (FileStream stream = new FileStream(AFileName, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(FiasDatabaseMap));
                serializer.Serialize(stream, this);
            }
        }

        public static FiasDatabaseMap LoadFromFile(string AFileName)
        {
            FiasDatabaseMap MethodResult = null;
            using (FileStream stream = new FileStream(AFileName, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(FiasDatabaseMap));
                MethodResult = (FiasDatabaseMap)serializer.Deserialize(stream);
            }
            return MethodResult;
        }
    }
}
