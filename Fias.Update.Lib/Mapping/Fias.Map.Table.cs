using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Fias.Update.Lib.Mapping
{
    [Serializable]
    public class FiasTableMap
    {
        [XmlAttribute]
        public string XmlName { get; set; }
        [XmlAttribute]
        public string DatabaseName { get; set; }
        [XmlAttribute]
        public string XmlDescription { get; set; }
        [XmlAttribute]
        public int OrderNum { get; set; }
        [XmlAttribute]
        public string RefTable { get; set; }

        private List<FiasFieldMap> _fields = new List<FiasFieldMap>();
        public List<FiasFieldMap> Fields { get { return _fields; } }

        /*public string GetSelectSqlTemplate()
        {
            string MethodResult = "select {0} from {1} where {2} = @{2};";
            MethodResult = string.Format(MethodResult,
                _fields.Select(a => a.DatabaseName).Aggregate((a, b) => a + "," + b),
                DatabaseName,
                _fields.First(a => a.IsPrimaryKey).DatabaseName);
            return MethodResult;
        }

        public string GetInsertSqlTemplate()
        {
            string MethodResult = "insert into {0} ({1}) values ({2});";
            MethodResult = string.Format(MethodResult,
                DatabaseName,
                _fields.Select(a => a.DatabaseName).Aggregate((a, b) => a + "," + b),
                _fields.Select(a => "@" + a.DatabaseName).Aggregate((a, b) => a + "," + b)
            );
            return MethodResult;
        }

        public string GetUpdateSqlTemplate()
        {
            string MethodResult = "update {0} set {1} where {2}=@{2};";
            MethodResult = string.Format(MethodResult,
                DatabaseName,
                _fields.Select(a => a.DatabaseName + "=@" + a.DatabaseName).Aggregate((a, b) => a + "," + b),
                _fields.First(a => a.IsPrimaryKey).DatabaseName
            );
            return MethodResult;
        }*/

        public string GetDatabaseFieldName(string AXmlFieldName)
        {
            FiasFieldMap fieldMap = _fields.FirstOrDefault(a => a.XmlName.ToUpper() == AXmlFieldName.ToUpper());
            if (fieldMap == null)
                return AXmlFieldName;
            else
                return fieldMap.DatabaseName;
        }

        public string GetDatabasePKFieldName()
        {
            FiasFieldMap fieldMap = _fields.FirstOrDefault(a => a.IsPrimaryKey);
            if (fieldMap == null)
                return "";
            else
                return fieldMap.DatabaseName;
        }
    }
}
