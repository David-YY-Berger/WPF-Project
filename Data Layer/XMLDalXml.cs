using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace DalXml
{

    namespace DO
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
            List<DO.Drone> listDrone = new List<DO.Drone>();

            for (int i = 0; i < 5; i++)
            {
                DO.Drone exampleD = new DO.Drone();

                exampleD.Id = i + 1;
                exampleD.MaxWeight = (DalXml.DO.WeightCategories)r.Next(0, 3);
                exampleD.Model = droneModels[r.Next(0, 2)];
                exampleD.Exists = true;
                listDrone.Add(exampleD);
            }
            DALTools.XMLTools.SaveListToXMLSerializer<DalXml.DO.Drone>(listDrone,dronesPath);

            //INITIALIZE STATION
            List<DalXml.DO.Station> listStation = new List<DalXml.DO.Station>();
            for (int i = 0; i < 2; i++)
            {
                DO.Station exampleS = new DalXml.DO.Station();

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

            List<DalXml.DO.Customer> listCustomer = new List<DalXml.DO.Customer>();
            for (int i = 0; i < 10; i++)
            {
                DalXml.DO.Customer exampleC = new DalXml.DO.Customer();
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
            DALTools.XMLTools.SaveListToXMLSerializer<DalXml.DO.Customer>(listCustomer, customersPath);

            //INITIALIZE PARCELS
            List<DalXml.DO.Parcel> listParcel = new List<DalXml.DO.Parcel>();

            for (int i = 0; i < thisConfig.NUM_PARCELS_TO_INITIALIZE; i++)
            {
                DO.Parcel exampleP = new DO.Parcel();
                exampleP.Id = thisConfig.parcelSerialNumber++;
                exampleP.SenderId = listCustomer[r.Next(0, 10)].Id;
                do
                {
                    exampleP.ReceiverId = listCustomer[r.Next(0, 10)].Id;
                } while (exampleP.ReceiverId == exampleP.SenderId);

                exampleP.Weight = (DalXml.DO.WeightCategories)r.Next(0, 3);
                exampleP.Priority = (DalXml.DO.Priorities)r.Next(0, 3);
                exampleP.TimeCreated = DateTime.Now;
                exampleP.Exists = true;

                //no Parcel is collectd/delivered  in Initialization
                listParcel.Add(exampleP);
            }
            DALTools.XMLTools.SaveListToXMLSerializer<DalXml.DO.Parcel>(listParcel, parcelsPath);

            //INITIALIZE USERS
            List<DalXml.DO.User> listUser = new List<DalXml.DO.User>();
            DalXml.DO.User userEmployee = new DalXml.DO.User();
            userEmployee.Id = -1; //employee
            userEmployee.Username = "boss";
            userEmployee.Password = "bPassword";
            listUser.Add(userEmployee);
            DalXml.DO.User userReuven = new DalXml.DO.User();
            userReuven.Id = 1; //customer reuven
            userReuven.Username = "Reuven613";
            userReuven.Password = "rPassword";
            listUser.Add(userReuven);
            DALTools.XMLTools.SaveListToXMLSerializer<DalXml.DO.User>(listUser, usersPath);
            //END OF FUNCTION
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DO.Drone GetDrone(int _id)
        {
            DO.Drone drone = new DO.Drone(0, "", 0);
            IEnumerable<DO.Drone> listDrone = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Drone>(dronesPath);

            foreach (DO.Drone dr in listDrone)
                if (dr.Id == _id && dr.Exists)
                    drone = dr;
            if (drone.Id == 0) throw new DO.EXItemNotFoundException();
            return drone;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DO.Customer GetCustomer(int _id)
        {
            DO.Customer cust = new DO.Customer(0, "", "", 0, 0);
            IEnumerable<DO.Customer> listCustomer = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Customer>(customersPath);

            foreach (DO.Customer cst in listCustomer)
                if (cst.Id == _id)
                   return cst;
           
            throw new DO.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DO.Parcel GetParcel(int _id)
        {
            DO.Parcel parcel = new DO.Parcel(0, 0, 0, 0);// DateTime.MinValue,DateTime.MinValue);
            IEnumerable<DO.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Parcel>(parcelsPath);

            foreach (DO.Parcel prc in listParcel)
                if (prc.Id == _id /*&& prc.Exists*/)
                    parcel = prc;
            if (parcel.Id == 0) throw new DO.EXItemNotFoundException();
            return parcel;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DO.Station GetStation(int _id)
        {
            DALTools.XmlStation xmlStation = new DALTools.XmlStation(stationsPath);
            return xmlStation.GetStation(_id);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DO.DroneCharge GetDroneCharge(int _droneId)
        {
            IEnumerable<DO.DroneCharge> listDroneCharge = DALTools.XMLTools.LoadListFromXMLSerializer<DO.DroneCharge>(droneChargesPath);

            foreach (var item in listDroneCharge)
            {
                if (item.DroneId == _droneId /*&& item.Exists*/)
                    return item;
            }
            throw new DO.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddDrone(DO.Drone drone)
        {
            List<DO.Drone> listDrone = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Drone>(dronesPath) as List<DO.Drone>;
            listDrone.Add(drone);
            DALTools.XMLTools.SaveListToXMLSerializer<DO.Drone>(listDrone, dronesPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddCustomer(DO.Customer custom)
        {
            List<DO.Customer> listCustomer = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Customer>(customersPath) as List<DO.Customer>;
            listCustomer.Add(custom);
            DALTools.XMLTools.SaveListToXMLSerializer<DO.Customer>(listCustomer, customersPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddParcel(DO.Parcel parcel)
        {
            List<DO.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Parcel>(parcelsPath) as List<DO.Parcel>;
            listParcel.Add(parcel);
            DALTools.XMLTools.SaveListToXMLSerializer<DO.Parcel>(listParcel, parcelsPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddStation(DO.Station st)
        {
            DALTools.XmlStation xmlStation = new DALTools.XmlStation(stationsPath);
            xmlStation.AddStation(st);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddDroneCharge(DO.DroneCharge droneCharge)
        {
            List<DO.DroneCharge> listDroneCharge = DALTools.XMLTools.LoadListFromXMLSerializer<DO.DroneCharge>(droneChargesPath).ToList();
            foreach (var item in listDroneCharge)   //checks if this drone already exists
                if (item.DroneId == droneCharge.DroneId)
                {
                    listDroneCharge.Remove(item);
                    listDroneCharge.Add(droneCharge);
                    DALTools.XMLTools.SaveListToXMLSerializer<DO.DroneCharge>(listDroneCharge, droneChargesPath);
                    return;
                }
            listDroneCharge.Add(droneCharge);
            DALTools.XMLTools.SaveListToXMLSerializer<DO.DroneCharge>(listDroneCharge, droneChargesPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddUser(DO.User _user)
        {
            List<DO.User> listUser = DALTools.XMLTools.LoadListFromXMLSerializer<DO.User>(usersPath) as List<DO.User>;
            listUser.Add(_user);
            DALTools.XMLTools.SaveListToXMLSerializer<DO.User>(listUser, usersPath);
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<double> RequestElec()
        {
            List<double> lst = new List<double> { Config.empty, Config.light, Config.mediuim, Config.heavy, Config.chargeRate };
            return lst;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DO.Drone> GetDrones()
        {
            return DALTools.XMLTools.LoadListFromXMLSerializer<DO.Drone>(dronesPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DO.Parcel> GetParcels()
        {
            return DALTools.XMLTools.LoadListFromXMLSerializer<DO.Parcel>(parcelsPath);
      
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DO.Station> GetStations()
        {
            DALTools.XmlStation xmlStation = new DALTools.XmlStation(stationsPath);
            return xmlStation.GetStationList();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DO.Customer> GetCustomers()
        {
            return DALTools.XMLTools.LoadListFromXMLSerializer<DO.Customer>(customersPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DO.DroneCharge> GetDroneCharges()
        {
            return DALTools.XMLTools.LoadListFromXMLSerializer<DO.DroneCharge>(droneChargesPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetIdFromUser(DO.User _user)
        {
            IEnumerable<DO.User> listUser = DALTools.XMLTools.LoadListFromXMLSerializer<DO.User>(usersPath);
            foreach (var item in listUser)
            {
                if (item.Username == _user.Username)
                    return item.Id;
            }
            throw new DO.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DO.User> GetUsers()
        {
            return DALTools.XMLTools.LoadListFromXMLSerializer<DO.User>(usersPath);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void EraseDrone(int droneId)
        {
            List<DO.Drone> listDrone = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Drone>(dronesPath).ToList();
            foreach (var item in listDrone)
            {
                if (item.Id == droneId)
                {
                    DO.Drone copy = item;
                    listDrone.Remove(item);
                    copy.Exists = false;
                    listDrone.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DO.Drone>(listDrone, dronesPath);
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void EraseCustomer(int id)
        {
            List<DO.Customer> listCustomer = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Customer>(customersPath).ToList();
            foreach (var item in listCustomer)
            {
                if (item.Id == id)
                {
                    DO.Customer copy = item;
                    listCustomer.Remove(item);
                    copy.Exists = false;
                    listCustomer.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DO.Customer>(listCustomer, customersPath);
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
            List<DO.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Parcel>(parcelsPath).ToList();
            foreach (var item in listParcel)
            {
                if (item.Id == id)
                {
                    DO.Parcel copy = item;
                    listParcel.Remove(item);
                    copy.Exists = false;
                    listParcel.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DO.Parcel>(listParcel, parcelsPath);
                    return;
                }

            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void EraseDroneCharge(DO.DroneCharge thisDroneCharge)
        {
            //if item not found, no exception is thrown..
            List<DO.DroneCharge> listDroneCharge = DALTools.XMLTools.LoadListFromXMLSerializer<DO.DroneCharge>(droneChargesPath).ToList();
            foreach (var item in listDroneCharge)
            {
                if (item.DroneId == thisDroneCharge.DroneId
                        && item.StationId == thisDroneCharge.StationId)
                {
                    DO.DroneCharge copy = new DO.DroneCharge();
                    copy = item;
                    listDroneCharge.Remove(thisDroneCharge); //FULLY ERASE DRONE CHARGE (unlike other objects)
                    //copy.Exists = false;
                    //listDroneCharge.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DO.DroneCharge>(listDroneCharge, droneChargesPath);
                    break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ModifyDrone(int _id, string _model) //changes drone model
        {
            List<DO.Drone> listDrone = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Drone>(dronesPath).ToList();
            foreach (var item in listDrone)
            {
                if (item.Id == _id && item.Exists)
                {
                    DO.Drone copy = item;
                    listDrone.Remove(item);
                    copy.Model = _model;
                    listDrone.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DO.Drone>(listDrone, dronesPath);
                    return;
                }
            }
            //if not found --> exception
            throw new DalXml.DO.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ModifyCust(int _id, string _name = "", string _phone = "")
        {
            List<DO.Customer> listCustomer = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Customer>(customersPath).ToList();
            foreach (var item in listCustomer)
            {
                if (item.Id == _id && item.Exists)
                {
                    DO.Customer copy = item;
                    listCustomer.Remove(item);
                    if (_name != "")
                        copy.Name = _name;
                    if (_phone != "")
                        copy.Phone = _phone;
                    listCustomer.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DO.Customer>(listCustomer, customersPath);
                    return;
                }
            }
            //if not found --> exception
            throw new DO.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ModifyStation(int _id, int _name = 0, int _totalChargeSlots = 0)
        {
            DALTools.XmlStation xmlStation = new DALTools.XmlStation(stationsPath);
            if (xmlStation.ModifyStation(_id, _name, _totalChargeSlots))
                return;
            else    //if not found --> exception
                throw new DalXml.DO.EXItemNotFoundException();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ModifyParcel(int _id, DO.Priorities? _priority)
        {
            List<DO.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Parcel>(parcelsPath).ToList();
            foreach (var item in listParcel)
            {
                if (item.Id == _id && item.Exists)
                {
                    DO.Parcel copy = item;
                    listParcel.Remove(item);
                    copy.Priority = _priority;
                    listParcel.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DO.Parcel>(listParcel, parcelsPath);
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AssignDroneToParcel(int droneId, int parcelId)
        {
            List<DO.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Parcel>(parcelsPath).ToList();
            foreach (var item in listParcel)
            {
                if (item.Id == parcelId && item.Exists)
                {
                    DO.Parcel copy = item;
                    listParcel.Remove(copy);
                    copy.DroneId = droneId;
                    copy.TimeAssigned = DateTime.Now;
                    listParcel.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DO.Parcel>(listParcel, parcelsPath);
                    return;
                }
            }
            //if not found --> exception
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void PickupParcel(int parcelId)
        {
            List<DO.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Parcel>(parcelsPath).ToList();
            foreach (var item in listParcel)
            {
                if (item.Id == parcelId && item.Exists)
                {
                    DO.Parcel copy = item;
                    listParcel.Remove(copy);
                    copy.TimePickedUp = DateTime.Now;
                    listParcel.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DO.Parcel>(listParcel, parcelsPath);
                    return;
                }
            }
            //if not found --> exception
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DeliverParcel(int parcelId)
        {
            List<DO.Parcel> listParcel = DALTools.XMLTools.LoadListFromXMLSerializer<DO.Parcel>(parcelsPath).ToList();
            foreach (var item in listParcel)
            {
                if (item.Id == parcelId && item.Exists)
                {
                    DalXml.DO.Parcel copy = item;
                    listParcel.Remove(copy);
                    copy.TimeDelivered = DateTime.Now;
                    listParcel.Add(copy);
                    DALTools.XMLTools.SaveListToXMLSerializer<DO.Parcel>(listParcel, parcelsPath);
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
