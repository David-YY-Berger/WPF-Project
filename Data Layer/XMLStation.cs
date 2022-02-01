using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace DALTools
{
    public class XmlStation
    {
        XElement stationRoot;
        string stationsPath;

        public XmlStation(String _stationsPath)
        {
            stationsPath = _stationsPath;
            if (!File.Exists(stationsPath))
                CreateFiles();
            else
                LoadData();
        }

        private void CreateFiles()
        {
            stationRoot = new XElement("stations");
            stationRoot.Save(stationsPath);
        }

        private void LoadData()
        {
            try
            {
                stationRoot = XElement.Load(stationsPath);
            }
            catch
            {
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SaveStationListLinq(List<DalXml.DO.Station> stationList)
        {
            stationRoot = new XElement("stations",
                            from p in stationList
                            select new XElement("station",
                            new XElement("id", p.Id),
                            new XElement("name", p.Name),
                            new XElement("longitude", p.Longitude),
                            new XElement("latitude", p.Latitude),
                            new XElement("chargeSlots", p.ChargeSlots),
                            new XElement("exists", p.Exists)));
            stationRoot.Save(stationsPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DalXml.DO.Station> GetStationList()
        {
            LoadData();
            IEnumerable<DalXml.DO.Station> listStations;
            try
            {
                listStations = (from s in stationRoot.Elements()
                                select new DalXml.DO.Station()
                                {
                                    Id = Convert.ToInt32(s.Element("id").Value),
                                    Name = Convert.ToInt32(s.Element("name").Value),
                                    ChargeSlots = Convert.ToInt32(s.Element("chargeSlots").Value),
                                    Longitude = Convert.ToDouble(s.Element("longitude").Value),
                                    Latitude = Convert.ToDouble(s.Element("latitude").Value),
                                    Exists = Convert.ToBoolean(s.Element("exists").Value)
                                });
            }
            catch
            {
                listStations = null;
            }
            return listStations;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DalXml.DO.Station GetStation(int _id)
        {
            LoadData();
            DalXml.DO.Station station;
            try
            {
                station = (from s in stationRoot.Elements()
                           where Convert.ToInt32(s.Element("id").Value) == _id
                           select new DalXml.DO.Station()
                           {
                               Id = Convert.ToInt32(s.Element("id").Value),
                               Name = Convert.ToInt32(s.Element("name").Value),
                               ChargeSlots = Convert.ToInt32(s.Element("chargeSlots").Value),
                               Longitude = Convert.ToDouble(s.Element("longitude").Value),
                               Latitude = Convert.ToDouble(s.Element("latitude").Value),
                               Exists = Convert.ToBoolean(s.Element("exists").Value)
                           }).FirstOrDefault();
            }
            catch
            {
                station = new DalXml.DO.Station();
            }
            return station;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddStation(DalXml.DO.Station _station)
        {
            XElement id = new XElement("id", _station.Id);
            XElement name = new XElement("name", _station.Name);
            XElement longitude = new XElement("longitude", _station.Longitude);
            XElement latitude = new XElement("latitude", _station.Latitude);
            XElement chargeSlots = new XElement("chargeSlots", _station.ChargeSlots);
            XElement exists = new XElement("exists", _station.Exists);
            stationRoot.Add(new XElement("station", id, name, longitude, latitude, chargeSlots, exists));
            stationRoot.Save(stationsPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool RemoveStation(int _id)
        {
            XElement stationElement;
            try
            {
                stationElement = (from s in stationRoot.Elements()
                                  where Convert.ToInt32(s.Element("id").Value) == _id
                                  select s).FirstOrDefault();
                stationElement.Element("exists").Value = false.ToString();
                stationRoot.Save(stationsPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool ModifyStation(int _id, int _name = 0, int _totalChargeSlots = 0)
        {
            XElement stationElement;
            stationElement = (from s in stationRoot.Elements()
                              where Convert.ToInt32(s.Element("id").Value) == _id
                              select s).FirstOrDefault();
            //HERE? CHECK BY NAME?
            if (Convert.ToInt32(stationElement.Element("id").Value) == _id && Convert.ToBoolean(stationElement.Element("exists").Value))
            {
                if (_name != 0)
                    stationElement.Element("name").Value = _name.ToString();
                if (_totalChargeSlots != 0)
                    stationElement.Element("chargeSlots").Value = _totalChargeSlots.ToString();
                stationRoot.Save(stationsPath);
                return true;
            }
            return false;
        }
    }
}
