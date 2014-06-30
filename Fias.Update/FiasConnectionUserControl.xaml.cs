using System.IO;
using System.Reflection;
using System.Windows;
using Common;
using Fias.Update.Lib;

namespace Fias.Update
{
    public partial class FiasConnectionUserControl
    {
        private readonly Model model = Singleton<Model>.Instance;
        private readonly string configFileName = string.Empty;

        public FiasUpdate Config { get; set; }
        public Model Model { get { return model; } }

        public FiasConnectionUserControl()
        {
            InitializeComponent();
            configFileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Fias.Update.xml";
            Config = File.Exists(configFileName) ? FiasUpdate.LoadFromFile(configFileName) : new FiasUpdate();
            DataContext = Config;
        }
        public void DoNext()
        {
            Config.SaveToFile(configFileName);
        }
        private void RbMsSql_OnChecked(object sender, RoutedEventArgs e)
        {
            Model.SelectedServerType=ServerType.MsSql;
        }

        private void RbFirebird_OnChecked(object sender, RoutedEventArgs e)
        {
            Model.SelectedServerType=ServerType.Firebird;
        }
    }
}
