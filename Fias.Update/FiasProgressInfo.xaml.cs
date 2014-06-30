using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Fias.Update
{   
    public partial class FiasProgressInfo : Window
    {
        public bool CancelClicked = false;
        public FiasProgressInfo()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CancelClicked = true;
            btnCancel.IsEnabled = false;
        }
    }
}
