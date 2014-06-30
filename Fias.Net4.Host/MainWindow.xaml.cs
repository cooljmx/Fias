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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fias.Net4;

namespace Fias.Net4.Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSearchByAddress_Click(object sender, RoutedEventArgs e)
        {
            var instance = new FiasModelProxy {ConnectionString = txtConnectionString.Text};
            instance.Details.Region.FullName = saiRegion.Text;
            instance.Details.Auto.FullName = saiAuto.Text;
            instance.Details.Area.FullName = saiArea.Text;
            instance.Details.City.FullName = saiCity.Text;
            instance.Details.Ctar.FullName = saiCtar.Text;
            instance.Details.Place.FullName = saiPlace.Text;
            instance.Details.Street.FullName = saiStreet.Text;
            instance.Details.House.FullName = saiHouse.Text;
            instance.SearchByDetails();
            sarGUID.Text = instance.Guid;
            sarKladr.Text = instance.Kladr;
            sarLevel.Text = instance.Level;
        }

        private void btnSearchByGuid_Click(object sender, RoutedEventArgs e)
        {
            var instance = new FiasModelProxy
            {
                ConnectionString = txtConnectionString.Text,
                Guid = sgiGUID.Text,
                Kladr = sgiKladr.Text
            };
            instance.SearchByGuid();
            sgrRegion.Text = instance.Details.Region.FullName;
            sgrRegionSocr.Text = instance.Details.Region.ShortName;
            sgrAuto.Text = instance.Details.Auto.FullName;
            sgrAutoSocr.Text = instance.Details.Auto.ShortName;
            sgrArea.Text = instance.Details.Area.FullName;
            sgrAreaSocr.Text = instance.Details.Area.ShortName;
            sgrCity.Text = instance.Details.City.FullName;
            sgrCitySocr.Text = instance.Details.City.ShortName;
            sgrCtar.Text = instance.Details.Ctar.FullName;
            sgrCtarSocr.Text = instance.Details.Ctar.ShortName;
            sgrPlace.Text = instance.Details.Place.FullName;
            sgrPlaceSocr.Text = instance.Details.Place.ShortName;
            sgrStreet.Text = instance.Details.Street.FullName;
            sgrStreetSocr.Text = instance.Details.Street.ShortName;
            sgrHouse.Text = instance.Details.House.FullName;
        }

        private void btnGuiShow_Click(object sender, RoutedEventArgs e)
        {
            var instance = new FiasModelProxy
            {
                ConnectionString = txtConnectionString.Text,
                Guid = gsiGUID.Text,
                Kladr = gsiKladr.Text
            };
            if (ExceptCheckBox.IsChecked != null && !instance.Show((bool) ExceptCheckBox.IsChecked)) return;
            gsrGUID.Text = instance.Guid;
            gsrKladr.Text = instance.Kladr;
        }

        private void btnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            var instance = new FiasModelProxy {ConnectionString = txtConnectionString.Text};
            MessageBox.Show(string.Format("Error code: {0}", instance.TestConnection()));
        }
    }
}
