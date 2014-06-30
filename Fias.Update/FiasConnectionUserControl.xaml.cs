using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fias.Update.Lib;

namespace Fias.Update
{
    public partial class FiasConnectionUserControl : UserControl
    {
        private string _configFileName = string.Empty;
        public FiasUpdate Config { get; set; }        
        public FiasConnectionUserControl()
        {
            InitializeComponent();
            _configFileName = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Fias.Update.xml";
            if (File.Exists(_configFileName))
                Config = FiasUpdate.LoadFromFile(_configFileName);
            else
                Config = new FiasUpdate();
            this.DataContext = Config;
        }
        public void DoNext()
        {
            Config.SaveToFile(_configFileName);
        }
    }
}
