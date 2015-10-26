using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Fias.Net4
{    
    public class FiasModelProxy :IDisposable
    {
        private readonly FiasModel model = new FiasModel();
        private string guid = "";
        private string kladr = "";
        private string level = "";
        private readonly FiasProxyModelDetail details = new FiasProxyModelDetail();

        public string ConnectionString { get; set; }
        public string Guid { get { return guid; } set { guid = value; } }
        public string Kladr { get { return kladr; } set { kladr = value; } }
        public string Level { get { return level; } }
        public FiasProxyModelDetail Details { get { return details; } }

        public bool Show(bool exceptHouses = false)
        {
            var card = new FiasCard(ConnectionString);
            var splash = new FiasSplash();
            splash.Show();
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            card.Model.Open(guid, kladr);

            if (exceptHouses)
            {
                card.HouseGroup.Visibility = Visibility.Collapsed;
                card.Model.SelectedHouse = null;
            }

            splash.Close();
            card.ShowDialog();
            
            var dialogResult = card.DialogResult ?? false;
            if (!dialogResult) return dialogResult;
            var result = card.Model.GetBottomSelectedItem();
            if (result == null) return dialogResult;
            guid = result.Id;
            kladr = result.KladrId;
            level = result.Level;

            Details.Region = card.Model.SelectedRegion;
            Details.Auto = card.Model.SelectedAuto;
            Details.Area = card.Model.SelectedArea;
            Details.City = card.Model.SelectedCity;
            Details.Ctar = card.Model.SelectedCtar;
            Details.Place = card.Model.SelectedPlace;
            Details.Street = card.Model.SelectedStreet;
            Details.House = card.Model.SelectedHouse;
            return dialogResult;
        }

        private void SetResult()
        {
            var result = model.GetBottomSelectedItem();
            if (result == null) return ;
            guid = result.Id;
            kladr = result.KladrId;
            level = result.Level;

            Details.Region = model.SelectedRegion ?? new FiasModelItem();
            Details.Auto = model.SelectedAuto ?? new FiasModelItem();
            Details.Area = model.SelectedArea ?? new FiasModelItem();
            Details.City = model.SelectedCity ?? new FiasModelItem();
            Details.Ctar = model.SelectedCtar ?? new FiasModelItem();
            Details.Place = model.SelectedPlace ?? new FiasModelItem();
            Details.Street = model.SelectedStreet ?? new FiasModelItem();
            Details.House = model.SelectedHouse ?? new FiasModelItem();
        }

        public void SearchByDetails()
        {
            model.ConnectionString = ConnectionString;
            model.Open("","");
            if (Details.Region.FullName != string.Empty)
                model.SelectedRegion =
                    model.RegionCollection.FirstOrDefault(
                        a => a.FullName.ToUpper().Contains(Details.Region.FullName.ToUpper()));
            if (Details.Auto.FullName != string.Empty)
                model.SelectedAuto =
                    model.AutoCollection.FirstOrDefault(
                        a => a.FullName.ToUpper().Contains(Details.Auto.FullName.ToUpper()));
            if (Details.Area.FullName != string.Empty)
                model.SelectedArea =
                    model.AreaCollection.FirstOrDefault(
                        a => a.FullName.ToUpper().Contains(Details.Area.FullName.ToUpper()));
            if (Details.City.FullName != string.Empty)
                model.SelectedCity =
                    model.CityCollection.FirstOrDefault(
                        a => a.FullName.ToUpper().Contains(Details.City.FullName.ToUpper()));
            if (Details.Ctar.FullName != string.Empty)
                model.SelectedCtar =
                    model.CtarCollection.FirstOrDefault(
                        a => a.FullName.ToUpper().Contains(Details.Ctar.FullName.ToUpper()));
            if (Details.Place.FullName != string.Empty)
                model.SelectedPlace =
                    model.PlaceCollection.FirstOrDefault(
                        a => a.FullName.ToUpper().Contains(Details.Place.FullName.ToUpper()));
            if (Details.Street.FullName != string.Empty)
                model.SelectedStreet =
                    model.StreetCollection.FirstOrDefault(
                        a => a.FullName.ToUpper().Contains(Details.Street.FullName.ToUpper()));
            if (Details.House.FullName != string.Empty)
                model.SelectedHouse =
                    model.HouseCollection.FirstOrDefault(
                        a =>
                            String.Equals(a.FullName, details.House.FullName, StringComparison.CurrentCultureIgnoreCase));

            SetResult();
        }

        public void SearchByGuid()
        {
            model.ConnectionString = ConnectionString;
            model.Open(guid, kladr);
            SetResult();
        }

        public int TestConnection()
        { 
            model.ConnectionString = ConnectionString;
            return model.TestConnection();
        }

        public void Dispose()
        {
            model.Dispose();
        }
    }
}
