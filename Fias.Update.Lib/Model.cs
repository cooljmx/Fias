using Invent.Entities;

namespace Fias.Update.Lib
{
    public class Model : VirtualNotifyPropertyChanged
    {
        private ServerType selectedServerType;

        protected Model()
        {
            
        }

        public ServerType SelectedServerType { get { return selectedServerType; } set { selectedServerType = value; NotifyPropertyChanged("SelectedServerType"); } }
    }
}
