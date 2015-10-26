using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Collections.ObjectModel;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using Invent.Entities;
using Rade.DbTools;
using System.Data;

namespace Fias.Net4
{
    internal class FiasModel : VirtualNotifyPropertyChanged, IDisposable
    {
        private readonly DbConnection connection = new FbConnection();
        
        private FiasModelItem selectedRegion;
        private FiasModelItem selectedAuto;
        private FiasModelItem selectedArea;
        private FiasModelItem selectedCity;
        private FiasModelItem selectedCtar;
        private FiasModelItem selectedPlace;
        private FiasModelItem selectedStreet;
        private FiasModelItem selectedHouse;
        private readonly ObservableCollection<FiasModelItem> regionCollection = new ObservableCollection<FiasModelItem>();
        private readonly ObservableCollection<FiasModelItem> autoCollection = new ObservableCollection<FiasModelItem>();
        private readonly ObservableCollection<FiasModelItem> areaCollection = new ObservableCollection<FiasModelItem>();
        private readonly ObservableCollection<FiasModelItem> cityCollection = new ObservableCollection<FiasModelItem>();
        private readonly ObservableCollection<FiasModelItem> ctarCollection = new ObservableCollection<FiasModelItem>();
        private readonly ObservableCollection<FiasModelItem> placeCollection = new ObservableCollection<FiasModelItem>();
        private readonly ObservableCollection<FiasModelItem> streetCollection = new ObservableCollection<FiasModelItem>();
        private readonly ObservableCollection<FiasModelItem> houseCollection = new ObservableCollection<FiasModelItem>();

        public string ConnectionString { get { return connection.ConnectionString; } set { connection.ConnectionString = value; } }
        public FiasModelItem SelectedRegion { get { return selectedRegion; } set { selectedRegion = value; OverrideNotifyPropertyChanged("SelectedRegion"); } }
        public FiasModelItem SelectedAuto { get { return selectedAuto; } set { selectedAuto = value; OverrideNotifyPropertyChanged("SelectedAuto"); } }
        public FiasModelItem SelectedArea { get { return selectedArea; } set { selectedArea = value; OverrideNotifyPropertyChanged("SelectedArea"); } }
        public FiasModelItem SelectedCity { get { return selectedCity; } set { selectedCity = value; OverrideNotifyPropertyChanged("SelectedCity"); } }
        public FiasModelItem SelectedCtar { get { return selectedCtar; } set { selectedCtar = value; OverrideNotifyPropertyChanged("SelectedCtar"); } }
        public FiasModelItem SelectedPlace { get { return selectedPlace; } set { selectedPlace = value; OverrideNotifyPropertyChanged("SelectedPlace"); } }
        public FiasModelItem SelectedStreet { get { return selectedStreet; } set { selectedStreet = value; OverrideNotifyPropertyChanged("SelectedStreet"); } }
        public FiasModelItem SelectedHouse { get { return selectedHouse; } set { selectedHouse = value; OverrideNotifyPropertyChanged("SelectedHouse"); } }
        public ObservableCollection<FiasModelItem> RegionCollection { get { return regionCollection; } }
        public ObservableCollection<FiasModelItem> AutoCollection { get { return autoCollection; } }
        public ObservableCollection<FiasModelItem> AreaCollection { get { return areaCollection; } }
        public ObservableCollection<FiasModelItem> CityCollection { get { return cityCollection; } }
        public ObservableCollection<FiasModelItem> CtarCollection { get { return ctarCollection; } }
        public ObservableCollection<FiasModelItem> PlaceCollection { get { return placeCollection; } }
        public ObservableCollection<FiasModelItem> StreetCollection { get { return streetCollection; } }
        public ObservableCollection<FiasModelItem> HouseCollection { get { return houseCollection; } }
        
        private void CustomReloadCollection(ICollection<FiasModelItem> collection, string sql, string parent)
        {
            collection.Clear();
            using (var transaction = connection.BeginTransaction())
            {
                using (var query = new DbQuery(connection, transaction))
                {
                    query.SqlText = string.Format(sql, parent);
                    query.ExecuteDataReader();
                    while (query.DataReader.Read())
                    {
                        var item = new FiasModelItem
                        {
                            Id = query.DataReader["id"].ToString(),
                            KladrId = query.DataReader["KLADR"].ToString(),
                            FullName = query.DataReader["full_name"].ToString(),
                            FormalName = query.DataReader["formal_name"].ToString(),
                            ScName = query.DataReader["SCNAME"].ToString(),
                            ShortName = query.DataReader["SOCRNAME"].ToString(),
                            Level = query.DataReader["ADDR_LEVEL"].ToString(),
                            IdDetail = query.DataReader["ID_DETAIL"].ToString(),
                            LiveStatus = query.DataReader["id_live_status"].ToString()
                        };
                        collection.Add(item);
                    }
                }
            }
        }

        public FiasModelItem GetBottomSelectedItem()
        {
            if (SelectedHouse != null) return SelectedHouse;
            else if (SelectedStreet != null) return SelectedStreet;
            else if (SelectedPlace != null) return SelectedPlace;
            else if (SelectedCtar != null) return SelectedCtar;
            else if (SelectedCity != null) return SelectedCity;
            else if (SelectedArea != null) return SelectedArea;
            else if (SelectedAuto != null) return SelectedAuto;
            else if (SelectedRegion != null) return SelectedRegion;
            else return null;
        }

        protected void OverrideNotifyPropertyChanged(string name)
        {
            NotifyPropertyChanged(name);

            FiasModelItem bottomSelectedItem = null;

            switch (name)
            {
                case "SelectedRegion":
                {
                    SelectedAuto = null;
                    bottomSelectedItem = GetBottomSelectedItem() ?? new FiasModelItem {Id = "0"};
                    if (SelectedRegion == null)
                    {
                        CustomReloadCollection(AutoCollection, FiasSql.AutoSql, bottomSelectedItem.Id);
                    }
                    else
                    {
                        CustomReloadCollection(AutoCollection, FiasSql.AutoSql, bottomSelectedItem.Id);
                        CustomReloadCollection(AreaCollection, FiasSql.AreaSql, bottomSelectedItem.Id);
                        CustomReloadCollection(CityCollection, FiasSql.CitySql, bottomSelectedItem.Id);
                        CustomReloadCollection(CtarCollection, FiasSql.CtarSql, bottomSelectedItem.Id);
                        CustomReloadCollection(PlaceCollection, FiasSql.PlaceSql, bottomSelectedItem.Id);
                        CustomReloadCollection(StreetCollection, FiasSql.StreetSql, bottomSelectedItem.Id);
                        CustomReloadCollection(HouseCollection, FiasSql.HouseSql, bottomSelectedItem.Id);
                    }
                    break;
                }
                case "SelectedAuto":
                {
                    SelectedArea = null;
                    bottomSelectedItem = GetBottomSelectedItem() ?? new FiasModelItem { Id = "0" };
                    if (SelectedAuto == null)
                    {
                        CustomReloadCollection(AreaCollection, FiasSql.AreaSql, bottomSelectedItem.Id);
                    }
                    else
                    {
                        CustomReloadCollection(AreaCollection, FiasSql.AreaSql, bottomSelectedItem.Id);
                        CustomReloadCollection(CityCollection, FiasSql.CitySql, bottomSelectedItem.Id);
                        CustomReloadCollection(CtarCollection, FiasSql.CtarSql, bottomSelectedItem.Id);
                        CustomReloadCollection(PlaceCollection, FiasSql.PlaceSql, bottomSelectedItem.Id);
                        CustomReloadCollection(StreetCollection, FiasSql.StreetSql, bottomSelectedItem.Id);
                        CustomReloadCollection(HouseCollection, FiasSql.HouseSql, bottomSelectedItem.Id);
                    }
                    break;
                }
                case "SelectedArea":
                {
                    SelectedCity = null;
                    bottomSelectedItem = GetBottomSelectedItem() ?? new FiasModelItem { Id = "0" };
                    if (SelectedArea == null)
                    {
                        CustomReloadCollection(CityCollection, FiasSql.CitySql, bottomSelectedItem.Id);
                    }
                    else
                    {
                        CustomReloadCollection(CityCollection, FiasSql.CitySql, bottomSelectedItem.Id);
                        CustomReloadCollection(CtarCollection, FiasSql.CtarSql, bottomSelectedItem.Id);
                        CustomReloadCollection(PlaceCollection, FiasSql.PlaceSql, bottomSelectedItem.Id);
                        CustomReloadCollection(StreetCollection, FiasSql.StreetSql, bottomSelectedItem.Id);
                        CustomReloadCollection(HouseCollection, FiasSql.HouseSql, bottomSelectedItem.Id);
                    }
                    break;
                }
                case "SelectedCity":
                {
                    SelectedCtar = null;
                    bottomSelectedItem = GetBottomSelectedItem() ?? new FiasModelItem { Id = "0" };
                    if (SelectedCity == null)
                    {
                        CustomReloadCollection(CtarCollection, FiasSql.CtarSql, bottomSelectedItem.Id);
                    }
                    else
                    {
                        CustomReloadCollection(CtarCollection, FiasSql.CtarSql, bottomSelectedItem.Id);
                        CustomReloadCollection(PlaceCollection, FiasSql.PlaceSql, bottomSelectedItem.Id);
                        CustomReloadCollection(StreetCollection, FiasSql.StreetSql, bottomSelectedItem.Id);
                        CustomReloadCollection(HouseCollection, FiasSql.HouseSql, bottomSelectedItem.Id);
                    }
                    break;
                }
                case "SelectedCtar":
                {
                    SelectedPlace = null;
                    bottomSelectedItem = GetBottomSelectedItem() ?? new FiasModelItem { Id = "0" };
                    if (SelectedCtar == null)
                    {
                        CustomReloadCollection(PlaceCollection, FiasSql.PlaceSql, bottomSelectedItem.Id);
                    }
                    else
                    {
                        CustomReloadCollection(PlaceCollection, FiasSql.PlaceSql, bottomSelectedItem.Id);
                        CustomReloadCollection(StreetCollection, FiasSql.StreetSql, bottomSelectedItem.Id);
                        CustomReloadCollection(HouseCollection, FiasSql.HouseSql, bottomSelectedItem.Id);
                    }
                    break;
                }
                case "SelectedPlace":
                {
                    SelectedStreet = null;
                    bottomSelectedItem = GetBottomSelectedItem() ?? new FiasModelItem { Id = "0" };
                    if (SelectedPlace == null)
                    {
                        CustomReloadCollection(StreetCollection, FiasSql.StreetSql, bottomSelectedItem.Id);
                    }
                    else
                    {
                        CustomReloadCollection(StreetCollection, FiasSql.StreetSql, bottomSelectedItem.Id);
                        CustomReloadCollection(HouseCollection, FiasSql.HouseSql, bottomSelectedItem.Id);
                    }
                    break;
                }
                case "SelectedStreet":
                {
                    SelectedHouse = null;
                    bottomSelectedItem = GetBottomSelectedItem() ?? new FiasModelItem { Id = "0" };
                    if (SelectedStreet == null)
                    {
                        CustomReloadCollection(HouseCollection, FiasSql.HouseSql, bottomSelectedItem.Id);
                    }
                    else
                    {
                        CustomReloadCollection(HouseCollection, FiasSql.HouseSql, bottomSelectedItem.Id);
                    }
                    break;
                }
            }
        }

        public void Dispose()
        {
            if (connection.State == ConnectionState.Connecting)
                connection.Close();
            connection.Dispose();
        }

        public int TestConnection()
        {
            try
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                connection.Close();
                return 0;
            }
            catch (DbException ex)
            {
                return ex.ErrorCode;
            }
        }
        
        public void Open(string guid, string kladr)
        {
            connection.ConnectionString = ConnectionString;
            connection.Open();
            CustomReloadCollection(RegionCollection, FiasSql.RegionSql, null);

            var searchResult = new Dictionary<string, string>();
            using (var transaction = connection.BeginTransaction())
            {
                using (var query = new DbQuery(connection, transaction))
                {
                    query.SqlText = "select ID_REGION, ID_AUTO, ID_AREA, ID_CITY, ID_CTAR, ID_PLACE, ID_STREET, ID_HOUSE from PR_FIAS_EXTRACT(@ID_GUID, @ID_KLADR)";
                    query.Parameters.Add(query.GetNewParameter("ID_GUID", guid == string.Empty ? null : guid));
                    query.Parameters.Add(query.GetNewParameter("ID_KLADR", kladr == string.Empty ? null : kladr));
                    query.ExecuteDataReader();
                    if (query.DataReader.Read())
                    {
                        for (var i = 0; i < query.DataReader.FieldCount; i++)
                        {
                            searchResult.Add(query.DataReader.GetName(i), query.DataReader[i].ToString());
                        }
                    }
                }
            }
            if (searchResult.Count == 0) return;
            if (searchResult["ID_REGION"] != string.Empty) SelectedRegion = RegionCollection.FirstOrDefault(a => a.Id == searchResult["ID_REGION"]);
            if (searchResult["ID_AUTO"] != string.Empty) SelectedAuto = AutoCollection.FirstOrDefault(a => a.Id == searchResult["ID_AUTO"]);
            if (searchResult["ID_AREA"] != string.Empty) SelectedArea = AreaCollection.FirstOrDefault(a => a.Id == searchResult["ID_AREA"]);
            if (searchResult["ID_CITY"] != string.Empty) SelectedCity = CityCollection.FirstOrDefault(a => a.Id == searchResult["ID_CITY"]);
            if (searchResult["ID_CTAR"] != string.Empty) SelectedCtar = CtarCollection.FirstOrDefault(a => a.Id == searchResult["ID_CTAR"]);
            if (searchResult["ID_PLACE"] != string.Empty) SelectedPlace = PlaceCollection.FirstOrDefault(a => a.Id == searchResult["ID_PLACE"]);
            if (searchResult["ID_STREET"] != string.Empty) SelectedStreet = StreetCollection.FirstOrDefault(a => a.Id == searchResult["ID_STREET"]);
            if (searchResult["ID_HOUSE"] != string.Empty) SelectedHouse = HouseCollection.FirstOrDefault(a => a.Id == searchResult["ID_HOUSE"]);
        }
    }
}
