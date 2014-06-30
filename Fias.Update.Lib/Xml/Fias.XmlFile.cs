using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Fias.Update.Lib.Xml
{
    public class FiasXmlFile
    {
        public string FileName { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Guid { get; set; }
        public int OrderNum { get; set; }
    }

    public class FiasXmlFileCollection
    {
        private ObservableCollection<FiasXmlFile> _files = new ObservableCollection<FiasXmlFile>();
        public ObservableCollection<FiasXmlFile> Files { get { return _files; } }

        public List<DateTime> GetDateList()
        {
            return _files.Select(a => a.Date).OrderBy(a => a).ToList();
        }

        public List<FiasXmlFile> GetFiles(DateTime ADate)
        {
            return _files.Where(a => a.Date == ADate).OrderBy(a => a.OrderNum).ToList();
        }
    }
}
