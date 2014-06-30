using System.Windows;
using DevExpress.Xpf.Core;namespace Fias.Net4
{
    internal partial class FiasCard : DXWindow
    {
        private readonly FiasModel model = new FiasModel();

        public FiasModel Model { get { return model; } }
        public FiasModelItem Result = null;

        public FiasCard(string connectionString)
        {
            InitializeComponent();
            model.ConnectionString = connectionString;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            //model.Open();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            model.Dispose();}        

        #region combo clearing
        private void ClearRegion_Click(object sender, RoutedEventArgs e)
        {
            model.SelectedRegion = null;
        }
        private void ClearAuto_Click(object sender, RoutedEventArgs e)
        {
            model.SelectedAuto = null;
        }
        private void ClearArea_Click(object sender, RoutedEventArgs e)
        {
            model.SelectedArea = null;
        }
        private void ClearCity_Click(object sender, RoutedEventArgs e)
        {
            model.SelectedCity = null;
        }
        private void ClearCtar_Click(object sender, RoutedEventArgs e)
        {
            model.SelectedCtar = null;
        }
        private void ClearPlace_Click(object sender, RoutedEventArgs e)
        {
            model.SelectedPlace = null;
        }
        private void ClearStreet_Click(object sender, RoutedEventArgs e)
        {
            model.SelectedStreet = null;
        }
        private void ClearHouse_Click(object sender, RoutedEventArgs e)
        {
            model.SelectedHouse = null;
        }
        #endregion

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
