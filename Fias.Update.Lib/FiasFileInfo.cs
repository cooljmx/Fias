using System.ComponentModel;

namespace Fias.Update.Lib
{
    public class FiasFileInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Private fields
        private int _version = 0;
        private string _description = string.Empty;
        private string _url = string.Empty;
        private string _fileName = string.Empty;
        private long _length = 0;private bool _checked = false;
        private bool _isExists = false;
        private bool _extracted = false;
        private string _status = string.Empty;
        private long _statusPos = 0;
        private long _statusMax = 100;
        #endregion

        #region Properies
        public int Version { get { return _version; } set { _version = value; NotifyPropertyChanged("Version"); } }        
        public string Description { get { return _description; } set { _description = value; NotifyPropertyChanged("Description"); } }        
        public string Url { get { return _url; } set { _url = value; NotifyPropertyChanged("Url"); } }
        public string FileName { get { return _fileName; } set { _fileName = value; NotifyPropertyChanged("LocalPath"); } }
        public long Length { get { return _length; } set { _length = value; NotifyPropertyChanged("Length"); NotifyPropertyChanged("LengthStr"); } }
        public string LengthStr
        {
            get
            {
                if (_length < 1024)
                    return string.Format("{0} байт", _length);
                else if (_length < 1024 * 1024)
                    return string.Format("{0,-3:f} Кбайт", _length / 1024.0);
                else if (_length < 1024 * 1024 * 1024)
                    return string.Format("{0,-3:f} Мбайт", _length / (1024.0 * 1024));
                else
                    return string.Format("{0,-3:f} Гбайт", _length / (1024.0 * 1024 * 1024));
            }
        }
        public bool Checked { get { return _checked; } set { _checked = value; NotifyPropertyChanged("Checked"); } }
        public bool IsExists { get { return _isExists; } set { _isExists = value; NotifyPropertyChanged("IsExists"); } }
        public string Status { get { return _status; } set { _status = value; NotifyPropertyChanged("Status"); } }
        public long StatusPos { get { return _statusPos; } set { _statusPos = value; NotifyPropertyChanged("StatusPos"); } }
        public long StatusMax { get { return _statusMax; } set { _statusMax = value; NotifyPropertyChanged("StatusMax"); } }
        public bool Extracted { get { return _extracted; } set { _extracted = value; NotifyPropertyChanged("Extracted"); } }
        #endregion
    }
}
