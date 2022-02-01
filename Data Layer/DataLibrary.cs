//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;


//THIS FILE IS A C# VERSION OF OUR xml FILES... NO LONGER IN USE


//namespace DalXml
//{

//    namespace DO
//    {
//        //public enum WeightCategories { light, medium, heavy };
//        //public enum DroneStatus         { available, work_in_progress, sent};
//        //work_in_progress - this Drone is charging...
//       // public enum Priorities { regular, fast, urgent };
        
//    }

//}


////public static class FactoryDL
////{
////    public static DalXml.IDal GetDL()
////    {
////        return DalObject.DalApi.DataSource.Instance;
////    }
////}




//namespace DalObject
//{ 
//    namespace DalApi
//    {
//        public sealed class DataSource :DalXml.IDal
//    {
//        internal class Config
//        {
//            //ratios for charging the drone; how many units of battery per minute, 
//            //according to weigth of the Parcel (heavier parcels require more battery
//            internal static double empty = 0.1;
//            internal static double light = 0.2;
//            internal static double mediuim =0.3;
//            internal static double heavy = 0.4;
//            internal static double chargeRate = 5.7; // per minute
//            internal int parcelSerialNumber = 1;
//        }

//        //internal fields:
//        internal static ObservableCollection<DalXml.DO.Station> listStation = new ObservableCollection<DalXml.DO.Station>();
//        internal static List<DalXml.DO.DroneCharge> listDroneCharge = new List<DalXml.DO.DroneCharge>();
//        internal static ObservableCollection<DalXml.DO.Drone> listDrone = new ObservableCollection<DalXml.DO.Drone>();
//        internal static ObservableCollection<DalXml.DO.Parcel> listParcel = new ObservableCollection<DalXml.DO.Parcel>();
//        internal static ObservableCollection<DalXml.DO.Customer> listCustomer = new ObservableCollection<DalXml.DO.Customer>();
            

//        internal static List<DalXml.DO.User> listUser = new List<DalXml.DO.User>(); //holds list of username and passwords

//        internal static Config thisConfig = new Config();

//        //Internal Class - for Lazy Initialization:
//        class Nested
//        {
//            static Nested() { }
//            internal static readonly DataSource instance = new DataSource();
//        }
//        //this field is static- so that it can be accessed even before the object is initialized
//        public static DataSource Instance { get { return Nested.instance; } }
//        private DataSource() //private CTOR - implemented Singleton Design pattern
//        {
//            Initialize();
//        }

//        public DalXml.DO.Drone GetDrone(int _id) {
//            DalXml.DO.Drone  drone = new DalXml.DO.Drone(0,"",0);
//            for (int i = 0; i < listDrone.Count; i++)
//                if (listDrone[i].Id == _id  && listDrone[i].Exists)
//                    drone = listDrone[i];
//            return drone;
//        }

//        public DalXml.DO.Customer GetCustomer(int _id) {
//            DalXml.DO.Customer cust = new DalXml.DO.Customer(0, "", "", 0, 0);
//                for (int i = 0; i < listCustomer.Count; i++)
//                    if (listCustomer[i].Id == _id && listCustomer[i].Exists)
//                    {
//                        cust = listCustomer[i];
//                        return cust;
//                    }
//            throw new DalXml.DO.EXItemNotFoundException();
//        }
//        public DalXml.DO.Parcel GetParcel(int _id)
//        {
//            DalXml.DO.Parcel parcel = new DalXml.DO.Parcel(0, 0, 0, 0);
//            for (int i = 0; i < listParcel.Count; i++)
//                if (listParcel[i].Id == _id /*&& listParcel[i].Exists*/)
//                    parcel = listParcel[i];
//            if (parcel.Id == 0) throw new DalXml.DO.EXItemNotFoundException();
//            return parcel;
//        }
//        public DalXml.DO.Station GetStation(int _id)
//        {
//            DalXml.DO.Station st = new DalXml.DO.Station(0, 0, 0, 0, 0);
//            for (int i = 0; i < listStation.Count; i++)
//                if (listStation[i].Id == _id) 
//                    st = listStation[i];
//            if (st.Id == 0) throw new DalXml.DO.EXItemNotFoundException();
//            return st;
//        }
//        public DalXml.DO.DroneCharge GetDroneCharge(int _droneId)
//        {
//            DalXml.DO.DroneCharge dc = new DalXml.DO.DroneCharge(0, 0);
//            foreach (var item in listDroneCharge)
//            {
//                if (item.DroneId == _droneId /*&& item.Exists*/)
//                    return item;
//            }
//            throw new DalXml.DO.EXItemNotFoundException();
//        }

//        public void AddDrone(DalXml.DO.Drone drone)
//        { 
//            listDrone.Add(drone);
//        }
//        public void AddCustomer(DalXml.DO.Customer custom)
//        {
//            listCustomer.Add(custom);
//        }
//        public void AddParcel(DalXml.DO.Parcel parcel)
//        {
//            listParcel.Add(parcel);
//        }
//        public void AddStation(DalXml.DO.Station st)
//        {
//            listStation.Add(st);
//        }
//        public void AddDroneCharge(DalXml.DO.DroneCharge droneCharge)
//        {
//            listDroneCharge.Add(droneCharge);
//        }
//        public void AddUser(DalXml.DO.User _user)
//            {
//                listUser.Add(_user);
//            }
//       public IEnumerable<double> RequestElec() {
//            List<double> lst = new List<double> {Config.empty, Config.light, Config.mediuim, Config.heavy ,Config.chargeRate};
//            return lst;
//        }

//        public IEnumerable<DalXml.DO.Drone> GetDrones()
//        {
//            return listDrone;
//        }
//        public IEnumerable<DalXml.DO.Parcel> GetParcels ()
//        {
//            return listParcel;
//        }
//        public IEnumerable<DalXml.DO.Station> GetStations()
//        {
//            return listStation;
//        }
//        public IEnumerable<DalXml.DO.Customer> GetCustomers()
//        {
//            return listCustomer;
//        }

//        public IEnumerable<DalXml.DO.DroneCharge> GetDroneCharges()
//        {
//            return listDroneCharge;
//        }

//         public int GetIdFromUser(DalXml.DO.User _user)
//            {
//                foreach (var item in listUser)
//                {
//                    if (item.Username == _user.Username)
//                        return item.Id;
//                }
//                throw new DalXml.DO.EXItemNotFoundException();
//            }
//         public IEnumerable<DalXml.DO.User> GetUsers()
//            {
//                return listUser;
//            }

//            public void Initialize()   
//        {
//            Random r = new Random();
//            //coordinates for area of jerusalem (Long: 35-37, Lat: 31-33)
//            const int LONGBEGIN = 35;
//            const int LONGEND = 37;
//            const int LATBEGIN = 31;
//            const int LATEND = 33;



//            //INITIALIZE DRONE

//            string[] droneModels = { "Merkava", "Namer" };

//            for (int i = 0; i < 5; i++)
//            {
//                DalXml.DO.Drone exampleD = new DalXml.DO.Drone();
                
//                exampleD.Id = i + 1;
//                exampleD.MaxWeight = (DalXml.DO.WeightCategories) r.Next(1, 4);
//                exampleD.Model = droneModels[r.Next(0, 2)];
//                exampleD.Exists = true;
//                listDrone.Add(exampleD);
//            }

//            //INITIALIZE STATION
//            for (int i = 0; i < 2; i++)
//            {
//                DalXml.DO.Station exampleS = new DalXml.DO.Station();


//                exampleS.Id = i + 1;
//                exampleS.Name = r.Next(20, 100);
//                //coordinates for Jerusalem area..
//                exampleS.Longitude = r.Next(LONGBEGIN, LONGEND) + r.NextDouble();
//                exampleS.Latitude = r.Next(LATBEGIN, LATEND) + r.NextDouble();
//                exampleS.ChargeSlots = r.Next(7, 13);
//                exampleS.Exists = true;
//                listStation.Add(exampleS);
//                //thisConfig.indexAvailStation++;
//            }


            
//            //INITIALIZE CUSTOMER
//            string[] customerNames = new string[12] { "Reuven", "Shimon", "Levi",
//                "Yehuda", "Yissachar", "Zevulun", "Asher", "Gad", "Dan", "Naftali",
//                "Yosef", "Binyamin" };
//            string[] customerPhones = new string[10] { "0552255518", "0525553455",
//                "0552355577", "0557155580", "0557155548", "0559555755",
//                "0556555137", "0545558684", "0556555731", "0552255513" };

//            for (int i = 0; i < 10; i++)
//            {
//                DalXml.DO.Customer exampleC = new DalXml.DO.Customer();
//                exampleC.Id = i + 1;

//                exampleC.Longitude = r.Next(LONGBEGIN, LONGEND) + r.NextDouble();
//                exampleC.Latitude = r.Next(LATBEGIN, LATEND) + r.NextDouble();

//                exampleC.Name = customerNames[i];
//                exampleC.Phone = customerPhones[i];
//                exampleC.Exists = true;

//                listCustomer.Add(exampleC);
//            }


//                //INITIALIZE PARCELS
//                for (int i = 0; i < 10; i++)
//                {
//                    DalXml.DO.Parcel exampleP = new DalXml.DO.Parcel();
//                    exampleP.Id = thisConfig.parcelSerialNumber++;
//                    exampleP.SenderId = listCustomer[r.Next(0, 10)].Id;
//                    do
//                    {
//                        exampleP.ReceiverId = listCustomer[r.Next(0, 10)].Id;
//                    } while (exampleP.ReceiverId == exampleP.SenderId);

//                    exampleP.Weight = (DalXml.DO.WeightCategories)r.Next(0, 3);
//                    exampleP.Priority = (DalXml.DO.Priorities)r.Next(0, 3);
//                    exampleP.TimeCreated = DateTime.Now;
//                    exampleP.Exists = true;


//                    //no Parcel is collectd/delivered  in Initialization

//                    listParcel.Add(exampleP);
//                }

//                //INITIALIZE USERS
//                DalXml.DO.User userEmployee = new DalXml.DO.User();
//                userEmployee.Id = -1; //employee
//                userEmployee.Username = "boss";
//                userEmployee.Password = "boss";
//                listUser.Add(userEmployee);
//                DalXml.DO.User userReuven = new DalXml.DO.User();
//                userReuven.Id = 1; //employee
//                userReuven.Username = "ruv";
//                userReuven.Password = "ruv";
//                listUser.Add(userReuven);
//                //END OF FUNCTION
//            }

           
//            public void EraseDrone(int droneId)
//            {
//                foreach (var item in listDrone)
//                {
//                    if (item.Id == droneId)
//                    {
//                        DalXml.DO.Drone copy = item;
//                        listDrone.Remove(item);
//                        copy.Exists = false;
//                        listDrone.Add(copy);
//                        return;
//                    }
                    
//                }
//            }
//            public void EraseCustomer(int id)
//            {
//                foreach (var item in listCustomer)
//                {
//                    if (item.Id == id)
//                    {
//                        DalXml.DO.Customer copy = item;
//                        listCustomer.Remove(item);
//                        copy.Exists = false;
//                        listCustomer.Add(copy);
//                        return;
//                    }

//                }
//            }
//            public void EraseStation(int id)
//            {
//                foreach (var item in listStation)
//                {
//                    if (item.Id == id)
//                    {
//                        DalXml.DO.Station copy = item;
//                        listStation.Remove(item);
//                        copy.Exists = false;
//                        listStation.Add(copy);
//                        return;
//                    }

//                }
//            }
//            public void EraseParcel(int id)
//            {
//                foreach (var item in listParcel)
//                {
//                    if (item.Id == id)
//                    {
//                        DalXml.DO.Parcel copy = item;
//                        listParcel.Remove(item);
//                        copy.Exists = false;
//                        listParcel.Add(copy);
//                        return;
//                    }

//                }
//            }
//            public void EraseDroneCharge(DalXml.DO.DroneCharge thisDroneCharge)
//            {

//                //if item not found, no exception is thrown..
//            foreach (var item in listDroneCharge)
//            {
//                if (item.DroneId == thisDroneCharge.DroneId
//                        && item.StationId == thisDroneCharge.StationId)
//                {
//                    DalXml.DO.DroneCharge copy = new DalXml.DO.DroneCharge();
//                    copy = item;
//                    listDroneCharge.Remove(thisDroneCharge);
//                    //copy.Exists = false;
//                    //listDroneCharge.Add(copy);
//                    break;
//                }
//            }

//        }






//        public void ModifyDrone(int _id, string _model) //changes drone model
//        {
//            foreach (var item in listDrone)
//            {
//                if (item.Id == _id && item.Exists)
//                {
//                    DalXml.DO.Drone copy = item;
//                    listDrone.Remove(item);
//                    copy.Model = _model;
//                    listDrone.Add(copy);
//                    return;
//                }
//            }
//            //if not found --> exception
//            throw new DalXml.DO.EXItemNotFoundException();
//        }
//        public void ModifyCust(int _id, string _name = "", string _phone = "")
//        {
//            foreach (var item in listCustomer )
//            {
//                if (item.Id == _id && item.Exists)
//                {
//                    DalXml.DO.Customer copy = item;
//                    listCustomer.Remove(item);
//                    if (_name != "")
//                        copy.Name = _name;
//                    if (_phone != "")
//                        copy.Phone = _phone;
//                    listCustomer.Add(copy);
//                    return;
//                }
//            }
//            //if not found --> exception
//            throw new DalXml.DO.EXItemNotFoundException();

//        }
//        public void ModifyStation(int _id, int _name = 0, int _totalChargeSlots = 0)
//        {
//            foreach (var item in listStation)
//            {
//                if (item.Id == _id && item.Exists)
//                {
//                    DalXml.DO.Station copy = item;
//                    listStation.Remove(item);
//                    if (_name != 0)
//                        copy.Name = _name;
//                    if (_totalChargeSlots != 0)
//                        copy.ChargeSlots = _totalChargeSlots;
//                    listStation.Add(copy);
//                    return;
//                }
//            }
//            //if not found --> exception
//            throw new DalXml.DO.EXItemNotFoundException();

//        }
//        public void ModifyParcel(int _id, DalXml.DO.Priorities? _priority)
//            {
//                foreach (var item in listParcel)
//                {
//                    if (item.Id == _id && item.Exists)
//                    {
//                        DalXml.DO.Parcel copy = item;
//                        listParcel.Remove(item);
//                        copy.Priority = _priority;
//                        listParcel.Add(copy);
//                        return;
//                    }
//                }
//            }


//            public void AssignDroneToParcel(int droneId, int parcelId)
//        {
//            foreach (var item in listParcel)
//            {
//                if (item.Id == parcelId && item.Exists)
//                {
//                    DalXml.DO.Parcel copy = item;
//                    listParcel.Remove(copy);
//                    copy.DroneId = droneId;
//                    copy.TimeAssigned = DateTime.Now;
//                    listParcel.Add(copy);
//                    return;
//                }
//            }
//            //if not found --> exception
//        }
//        public void PickupParcel(int parcelId)
//        {
//            foreach (var item in listParcel)
//            {
//                if (item.Id == parcelId && item.Exists)
//                {
//                    DalXml.DO.Parcel copy = item;
//                    listParcel.Remove(copy);
//                    copy.TimePickedUp = DateTime.Now;
//                    listParcel.Add(copy);
//                    return;
//                }
//            }
//            //if not found --> exception
//        }
//        public void DeliverParcel(int parcelId)
//        {
//            foreach (var item in listParcel )
//            {
//                if (item.Id == parcelId && item.Exists)
//                {
//                    DalXml.DO.Parcel copy = item;
//                    listParcel.Remove(copy);
//                    copy.TimeDelivered = DateTime.Now;
//                    listParcel.Add(copy);
//                    return;
//                }
//            }
//            //if not found --> exception
//        }


//        //User functions
        


//        //public IEnumerable<DalXml.DO.Drone> getSpecificDroneList(Predicate<DalXml.DO.Drone> property)
//        //{
//        //        List<DalXml.DO.Drone> lst = new List<DalXml.DO.Drone>();
//        //        foreach (var item in listDrone)


//        //        {
//        //            lst.Add
//        //        }
//        //    return listDrone.FindAll(property);
//        //}


//    }

//    }

    

//}


