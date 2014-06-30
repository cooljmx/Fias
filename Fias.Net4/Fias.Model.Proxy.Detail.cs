
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Fias.Net4
{
    [Serializable]
    public class FiasProxyModelDetail
    {
        private FiasModelItem region = new FiasModelItem();
        private FiasModelItem auto = new FiasModelItem();
        private FiasModelItem area = new FiasModelItem();
        private FiasModelItem city = new FiasModelItem();
        private FiasModelItem ctar = new FiasModelItem();
        private FiasModelItem place = new FiasModelItem();
        private FiasModelItem street = new FiasModelItem();
        private FiasModelItem house = new FiasModelItem();

        public FiasModelItem Region {
            get { return region; }
            set { region = value; }
        }
        public FiasModelItem Auto
        {
            get { return auto; }
            set { auto = value; }
        }
        public FiasModelItem Area
        {
            get { return area; }
            set { area = value; }
        }
        public FiasModelItem City
        {
            get { return city; }
            set { city = value; }
        }
        public FiasModelItem Ctar {
            get { return ctar; }
            set { ctar = value; }
        }
        public FiasModelItem Place {
            get { return place; }
            set { place = value; }
        }
        public FiasModelItem Street {
            get { return street; }
            set { street = value; }
        }
        public FiasModelItem House {
            get { return house; }
            set { house = value; }
        }
    }
}
