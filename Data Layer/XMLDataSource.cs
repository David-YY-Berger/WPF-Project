using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace DalXml
{

    namespace DataObject
    {
        public enum WeightCategories { light, medium, heavy };
        public enum Priorities { regular, fast, urgent };
    }

}




namespace DalXml
{
    public sealed class DataSource : IDal
    {
        internal class Config
        {
            //ratios for charging the drone; how many units of battery per second, 
            //according to weight of the Parcel (heavier parcels require more battery
            internal static double empty = 0.1;
            internal static double light = 0.2;
            internal static double mediuim = 0.3;
            internal static double heavy = 0.4;
            internal static double chargeRate = .613; // per second  
            internal int parcelSerialNumber = 1;

            //coordinates for area of jerusalem (Long: 35-37, Lat: 31-33)
            internal int LONGBEGIN = 35;
            internal int LONGEND = 37;
            internal int LATBEGIN = 31;
            internal int LATEND = 33;

            internal int NUM_PARCELS_TO_INITIALIZE = 30;
        }

        internal static Config thisConfig = new Config();

        #region DS XML Files

            string stationsPath = @"StationsXml.xml"; //XElement
            string droneChargesPath = @"DroneChargesXml.xml"; //XMLSerializer
            string dronesPath = @"DronesXml.xml"; //XMLSerializer
            string parcelsPath = @"ParcelsXml.xml"; //XMLSerializer
            string customersPath = @"CustomersXml.xml"; //XMLSerializer
            string usersPath = @"UsersXml.xml"; //XMLSerializer (holds list of username and passwords)

            #endregion


        #region singelton
            //Internal Class - for Lazy Initialization:
            class Nested
            {
                static Nested() { }
                internal static readonly DataSource instance = new DataSource();
            }
            //this field is static- so that it can be accessed even before the object is initialized
            public static DataSource Instance { get { return Nested.instance; } }
            private DataSource() //private CTOR - implemented Singleton Design pattern
            {
                Initialize();
            }

        #endregion

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Initialize()
        {
            Random r = new Random();

            //INITIALIZE DRONE

            string[] droneModels = { "Merkava", "Namer" };
            List<DataObject.Drone> listDrone = new List<DataObject.Drone>();

            for (int i = 0; i < 5; i++)
            {
                DataObject.Drone exampleD = new DataObject.Drone();

                exampleD.Id = i + 1;
                exampleD.MaxWeight = (DalXml.DataObject.WeightCategories)r.Next(0, 3);
                exampleD.Model = droneModels[r.Next(0, 2)];
                exampleD.Exists = true;
                listDrone.Add(exampleD);
            }
            DALTools.XMLTools.SaveListToXMLSerializer<DalXml.DataObject.Drone>(listDrone,dronesPath);

            //INITIALIZE STATION
            List<DalXml.DataObject.Station> listStation = new List<DalXml.DataObject.Station>();
            for (int i = 0; i < 2; i++)
            {
                DataObject.Station exampleS = new DalXml.DataObject.Station();

                exampleS.Id = i + 1;
                exampleS.Name = r.Next(20, 100);
                //coordinates for Jerusalem area..
                exampleS.Longitude = r.Next(thisConfig.LONGBEGIN, thisConfig.LONGEND) + r.NextDouble();
                exampleS.Latitude = r.Next(thisConfig.LATBEGIN, thisConfig.LATEND) + r.NextDouble();
                exampleS.Longitude = Math.Round(exampleS.Longitude, 5);
                exampleS.Latitude = Math.Round(exampleS.Latitude, 5);
                exampleS.ChargeSlots = r.Next(7, 13);
                exampleS.Exists = true;
                listStation.Add(exampleS);
                //thisConfig.indexAvailStation++;
            }
            DALTools.XmlStation xmlStation = new DALTools.XmlStation(stationsPath);
            xmlStation.SaveStationListLinq(listStation);

            //INITIALIZE CUSTOMER
            string[] customerNames = new string[12] { "Reuven", "Shimon", "Levi",
            "Yehuda", "Yissachar", "Zevulun", "Asher", "Gad", "Dan", "Naftali",
            "Yosef", "Binyamin" };
            string[] customerPhones = new string[10] { "0552255518", "0525553455",
            "0552355577", "0557155580", "0557155548", "0559555755",
            "0556555137", "0545558684", "0556555731", "0552255513" };

            List<DalXml.DataObject.Customer> listCustomer = new List<DalXml.DataObject.Customer>();
            for (int i = 0; i < 10; i++)
            {
                DalXml.DataObject.Customer exampleC = new DalXml.DataObject.Customer();
                exampleC.Id = i + 1;
                exampleC.Longitude = r.Next(thisConfig.LONGBEGIN, thisConfig.LONGEND) + r.NextDouble();
                exampleC.Latitude = r.Next(thisConfig.LATBEGIN, thisConfig.LATEND) + r.NextDouble();
                exampleC.Longitude = Math.Round(exampleC.Longitude,5);
                exampleC.Latitude = Math.Round(exampleC.Latitude, 5);
                exampleC.Name = customerNames[i];
                exampleC.Phone = customerPhones[i];
                exampleC.Exists = true;

                listCustomer.Add(exampleC);
            }
            DALTools.XMLTools.SaveListToXMLSerializer<DalXml.DataObject.Customer>(listCustomer, customersPath);

            //INITIALIZE PARCELS
            List<DalXml.DataObject.Parcel> listParcel = new List<DalXml.DataObject.Parcel>();

            for (int i = 0; i < thisConfig.NUM_PARCELS_TO_INITIALIZE; i++)
            {
                DataObject.Parcel exampleP = new DataObject.Parcel();
                exampleP.Id = thisConfig.parcelSerialNumber++;
                exampleP.SenderId = listCustomer[r.Next(0, 10)].Id;
                do
                {
                    exampleP.ReceiverId = listCustomer[r.Next(0, 10)].Id;
                } while (exampleP.ReceiverId == exampleP.SenderId);

                exampleP.Weight = (DalXml.DataObject.WeightCategories)r.Next(0, 3);
                exampleP.Priority = (DalXml.DataObject.Priorities)r.Next(0, 3);
                exampleP.TimeCreated = DateTime.Now;
                exampleP.Exists = true;

                //no Parcel is collectd/delivered  in Initialization
                listParcel.Add(exampleP);
            }
            DALTools.XMLTools.SaveListToXMLSerializer<DalXml.DataObject.Parcel>(listParcel, parcelsPath);

            //INITIALIZE USERS
            List<DalXml.DataObject.User> listUser = new List<DalXml.DataObject.User>();
            DalXml.DataObject.User userEmployee = new DalXml.DataObject.User();
            userEmployee.Id = -1; //employee
            userEmployee.Username = "boss";
            userEmployee.Password = "bPassword";
            listUser.Add(userEmployee);
            DalXml.DataObject.User userReuven = new DalXml.DataObject.User();
            userReuven.Id = 1; //customer reuven
            userReuven.Username = "Reuven613";
            userReuven.Password = "rPassword";
            listUser.Add(userReuven);
            DALTools.XMLTools.SaveListToXMLSerializer<DalXml.DataObject.User>(listUser, usersPath);

            //reset drone charge..
            List<DataObject.DroneCharge> empty = new List<DataObject.DroneCharge>();
            DALTools.XMLTools.SaveListToXMLSerializer<DalXml.DataObject.DroneCharge>(empty, droneChargesPath);
            //END OF FUNCTION
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DataObject.Drone GetDrone(int _id)
        {
            DataObject.Drone drone = new DataObject.Drone(0, "", 0);
            IEnumerable<DataObject.Drone> listDrone = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Drone>(dronesPath);

            foreach (DataObject.Drone dr in listDrone)
                if (dr.Id == _id && dr.Exists)
                    drone = dr;
            if (drone.Id == 0) throw new DataObject.EXItemNotFoundException();
            return drone;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DataObject.Customer GetCustomer(int _id)
        {
            DataObject.Customer cust = new DataObject.Customer(0, "", "", 0, 0);
            IEnumerable<DataObject.Customer> listCustomer = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Customer>(customersPath);

            foreach (DataObject.Customer cst in listCustomer)
                if (cst.Id == _id)
                   return cst;
           
            throw new DataObject.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DataObject.Parcel GetParcel(int _id)
        {
            DataObject.Parcel parcel = new DataObject.Parcel(0, 0, 0, 0);// DateTime.MinValue,DateTime.MinValue);
            IEnumerable<DataObject.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Parcel>(parcelsPath);

            foreach (DataObject.Parcel prc in listParcel)
                if (prc.Id == _id /*&& prc.Exists*/)
                    parcel = prc;
            if (parcel.Id == 0) throw new DataObject.EXItemNotFoundException();
            return parcel;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DataObject.Station GetStation(int _id)
        {
            DALTools.XmlStation xmlStation = new DALTools.XmlStation(stationsPath);
            return xmlStation.GetStation(_id);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DataObject.DroneCharge GetDroneCharge(int _droneId)
        {
            IEnumerable<DataObject.DroneCharge> listDroneCharge = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.DroneCharge>(droneChargesPath);

            foreach (var item in listDroneCharge)
            {
                if (item.DroneId == _droneId /*&& item.Exists*/)
                    return item;
            }
            throw new DataObject.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddDrone(DataObject.Drone drone)
        {
            List<DataObject.Drone> listDrone = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Drone>(dronesPath) as List<DataObject.Drone>;
            listDrone.Add(drone);
            DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Drone>(listDrone, dronesPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddCustomer(DataObject.Customer custom)
        {
            List<DataObject.Customer> listCustomer = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Customer>(customersPath) as List<DataObject.Customer>;
            listCustomer.Add(custom);
            DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Customer>(listCustomer, customersPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddParcel(DataObject.Parcel parcel)
        {
            List<DataObject.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Parcel>(parcelsPath) as List<DataObject.Parcel>;
            listParcel.Add(parcel);
            DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Parcel>(listParcel, parcelsPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddStation(DataObject.Station st)
        {
            DALTools.XmlStation xmlStation = new DALTools.XmlStation(stationsPath);
            xmlStation.AddStation(st);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddDroneCharge(DataObject.DroneCharge droneCharge)
        {
            List<DataObject.DroneCharge> listDroneCharge = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.DroneCharge>(droneChargesPath).ToList();
            foreach (var item in listDroneCharge)   //checks if this drone already exists
                if (item.DroneId == droneCharge.DroneId)
                {
                    listDroneCharge.Remove(item);
                    listDroneCharge.Add(droneCharge);
                    DALTools.XMLTools.SaveListToXMLSerializer<DataObject.DroneCharge>(listDroneCharge, droneChargesPath);
                    return;
                }
            listDroneCharge.Add(droneCharge);
            DALTools.XMLTools.SaveListToXMLSerializer<DataObject.DroneCharge>(listDroneCharge, droneChargesPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddUser(DataObject.User _user)
        {
            List<DataObject.User> listUser = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.User>(usersPath) as List<DataObject.User>;
            listUser.Add(_user);
            DALTools.XMLTools.SaveListToXMLSerializer<DataObject.User>(listUser, usersPath);
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<double> RequestElec()
        {
            List<double> lst = new List<double> { Config.empty, Config.light, Config.mediuim, Config.heavy, Config.chargeRate };
            return lst;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DataObject.Drone> GetDrones()
        {
            return DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Drone>(dronesPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DataObject.Parcel> GetParcels()
        {
            return DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Parcel>(parcelsPath);
      
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DataObject.Station> GetStations()
        {
            DALTools.XmlStation xmlStation = new DALTools.XmlStation(stationsPath);
            return xmlStation.GetStationList();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DataObject.Customer> GetCustomers()
        {
            return DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Customer>(customersPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DataObject.DroneCharge> GetDroneCharges()
        {
            return DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.DroneCharge>(droneChargesPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetIdFromUser(DataObject.User _user)
        {
            IEnumerable<DataObject.User> listUser = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.User>(usersPath);
            foreach (var item in listUser)
            {
                if (item.Username == _user.Username)
                    return item.Id;
            }
            throw new DataObject.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DataObject.User> GetUsers()
        {
            return DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.User>(usersPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void EraseDrone(int droneId)
        {
            List<DataObject.Drone> listDrone = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Drone>(dronesPath).ToList();
            foreach (var item in listDrone)
            {
                if (item.Id == droneId)
                {
                    DataObject.Drone copy = item;
                    listDrone.Remove(item);
                    copy.Exists = false;
                    listDrone.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Drone>(listDrone, dronesPath);
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void EraseCustomer(int id)
        {
            List<DataObject.Customer> listCustomer = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Customer>(customersPath).ToList();
            foreach (var item in listCustomer)
            {
                if (item.Id == id)
                {
                    DataObject.Customer copy = item;
                    listCustomer.Remove(item);
                    copy.Exists = false;
                    listCustomer.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Customer>(listCustomer, customersPath);
                    return;
                }

            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void EraseStation(int id)
        {
            DALTools.XmlStation xmlStation = new DALTools.XmlStation(stationsPath);
            xmlStation.RemoveStation(id);

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void EraseParcel(int id)
        {
            List<DataObject.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Parcel>(parcelsPath).ToList();
            foreach (var item in listParcel)
            {
                if (item.Id == id)
                {
                    DataObject.Parcel copy = item;
                    listParcel.Remove(item);
                    copy.Exists = false;
                    listParcel.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Parcel>(listParcel, parcelsPath);
                    return;
                }

            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void EraseDroneCharge(DataObject.DroneCharge thisDroneCharge)
        {
            //if item not found, no exception is thrown..
            List<DataObject.DroneCharge> listDroneCharge = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.DroneCharge>(droneChargesPath).ToList();
            foreach (var item in listDroneCharge)
            {
                if (item.DroneId == thisDroneCharge.DroneId
                        && item.StationId == thisDroneCharge.StationId)
                {
                    DataObject.DroneCharge copy = new DataObject.DroneCharge();
                    copy = item;
                    listDroneCharge.Remove(thisDroneCharge); //FULLY ERASE DRONE CHARGE (unlike other objects)
                    //copy.Exists = false;
                    //listDroneCharge.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DataObject.DroneCharge>(listDroneCharge, droneChargesPath);
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ModifyDrone(int _id, string _model) //changes drone model
        {
            List<DataObject.Drone> listDrone = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Drone>(dronesPath).ToList();
            foreach (var item in listDrone)
            {
                if (item.Id == _id && item.Exists)
                {
                    DataObject.Drone copy = item;
                    listDrone.Remove(item);
                    copy.Model = _model;
                    listDrone.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Drone>(listDrone, dronesPath);
                    return;
                }
            }
            //if not found --> exception
            throw new DalXml.DataObject.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ModifyCust(int _id, string _name = "", string _phone = "")
        {
            List<DataObject.Customer> listCustomer = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Customer>(customersPath).ToList();
            foreach (var item in listCustomer)
            {
                if (item.Id == _id && item.Exists)
                {
                    DataObject.Customer copy = item;
                    listCustomer.Remove(item);
                    if (_name != "")
                        copy.Name = _name;
                    if (_phone != "")
                        copy.Phone = _phone;
                    listCustomer.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Customer>(listCustomer, customersPath);
                    return;
                }
            }
            //if not found --> exception
            throw new DataObject.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ModifyStation(int _id, int _name = 0, int _totalChargeSlots = 0)
        {
            DALTools.XmlStation xmlStation = new DALTools.XmlStation(stationsPath);
            if (xmlStation.ModifyStation(_id, _name, _totalChargeSlots))
                return;
            else    //if not found --> exception
                throw new DalXml.DataObject.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ModifyParcel(int _id, DataObject.Priorities? _priority)
        {
            List<DataObject.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Parcel>(parcelsPath).ToList();
            foreach (var item in listParcel)
            {
                if (item.Id == _id && item.Exists)
                {
                    DataObject.Parcel copy = item;
                    listParcel.Remove(item);
                    copy.Priority = _priority;
                    listParcel.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Parcel>(listParcel, parcelsPath);
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AssignDroneToParcel(int droneId, int parcelId)
        {
            List<DataObject.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Parcel>(parcelsPath).ToList();
            foreach (var item in listParcel)
            {
                if (item.Id == parcelId && item.Exists)
                {
                    DataObject.Parcel copy = item;
                    listParcel.Remove(copy);
                    copy.DroneId = droneId;
                    copy.TimeAssigned = DateTime.Now;
                    listParcel.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Parcel>(listParcel, parcelsPath);
                    return;
                }
            }
            //if not found --> exception
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void PickupParcel(int parcelId)
        {
            List<DataObject.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Parcel>(parcelsPath).ToList();
            foreach (var item in listParcel)
            {
                if (item.Id == parcelId && item.Exists)
                {
                    DataObject.Parcel copy = item;
                    listParcel.Remove(copy);
                    copy.TimePickedUp = DateTime.Now;
                    listParcel.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Parcel>(listParcel, parcelsPath);
                    return;
                }
            }
            //if not found --> exception
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DeliverParcel(int parcelId)
        {
            List<DataObject.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DataObject.Parcel>(parcelsPath).ToList();
            foreach (var item in listParcel)
            {
                if (item.Id == parcelId && item.Exists)
                {
                    DalXml.DataObject.Parcel copy = item;
                    listParcel.Remove(copy);
                    copy.TimeDelivered = DateTime.Now;
                    listParcel.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DataObject.Parcel>(listParcel, parcelsPath);
                    return;
                }
            }
            //if not found --> exception
        }
        public double GetLongitudeBegin()
        {
            return thisConfig.LONGBEGIN;
        }
        public double GetLongitudeEnd()
        {
            return thisConfig.LONGEND;
        }
        public double GetLatitudeBegin()
        {
            return thisConfig.LATBEGIN;
        }
        public double GetLatitudeEnd()
        {
            return thisConfig.LATEND;
        }

        //end of class...
    }
}
