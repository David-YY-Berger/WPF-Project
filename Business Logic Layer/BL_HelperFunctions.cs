using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
namespace BL
{
    namespace BLApi
    {
        public partial class BusinessLayer : global::BL.BLApi.Ibl
        {

            [MethodImpl(MethodImplOptions.Synchronized)]
            public double GetChargeRate()
            {
                return chargeRate;
            }

            BO.BOLocation getClosestStationLoc(BO.BOLocation sourceLocation, bool needChargeSlot = false)
            {
                lock (dataAccess)
                {
                    //"needChargeSlots" --> if we need the station to have a free spot,
                    //then we send a parameter = true.
                    //otherwise, we can ignore this parameter

                    BO.BOLocation result = new BO.BOLocation(0, 0); //set to "null" to begin with..

                    foreach (DalXml.DataObject.Station st in dataAccess.GetStations())
                    {
                        //skip Stations with no available charging slots...
                        if (needChargeSlot == true) //if we need the station to have a free slot
                        {
                            if (freeSpots(st) <= 0) //if there are no free spots in this station, we continue our loop
                                continue;
                        }

                        BO.BOLocation checkThisStationLocation = new BO.BOLocation(st.Longitude, st.Latitude);
                        if (HelpfulFunctionsBL.GetDistance(sourceLocation, checkThisStationLocation) < HelpfulFunctionsBL.GetDistance(sourceLocation, result))
                        {
                            result.Latitude = checkThisStationLocation.Latitude;
                            result.Longitude = checkThisStationLocation.Longitude;
                        }
                    }
                    if (result.Latitude == 0 || result.Longitude == 0)
                        if (needChargeSlot == true)
                            throw new EXNoStationWithAvailChargingSlots();
                        else
                            throw new EXMiscException("Unknown exception - could not locate station");
                    return result;
                }
            }

            DalXml.DataObject.Station getStationFromLoc(BO.BOLocation loc)
            {
                lock (dataAccess)
                {
                    IEnumerable<DalXml.DataObject.Station> stations = dataAccess.GetStations();
                    foreach (var item in stations)
                    {
                        if (item.Longitude == loc.Longitude && item.Latitude == loc.Latitude)
                            return item;
                    }
                    //throw exception! //not found;
                    throw new EXNotFoundPrintException("Station");
                }
            }
            BO.BOLocation getLocationOfCustomer(int customerId)
            {
                BO.BOLocation loc =
                            new BO.BOLocation(dataAccess.GetCustomer(customerId).Longitude,
                            dataAccess.GetCustomer(customerId).Latitude);
                return loc;
            }

            BO.BOLocation getLocationOfStation(int StationId)
            {
                BO.BOLocation loc;
                try
                {
                    loc = new BO.BOLocation(dataAccess.GetStation(StationId).Longitude,
                            dataAccess.GetStation(StationId).Latitude);
                }
                catch (DalXml.DataObject.EXItemNotFoundException)
                {
                    throw new EXNotFoundPrintException("Station " + StationId.ToString());
                }

                return loc;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public int GetStationIdOfBODrone(int droneId)
            {
                //check if charging
                BO.BODrone drone = GetBODrone(droneId);
                foreach (DalXml.DataObject.DroneCharge drCharge in GetDroneCharges())
                {
                    if (drCharge.DroneId == droneId)
                        return drCharge.StationId;
                }
                //check if assigned at Station
                IEnumerable<DalXml.DataObject.Station> stationList = dataAccess.GetStations();
                foreach (DalXml.DataObject.Station st in stationList)//dataAccess.GetStations())
                {
                    BO.BOLocation stLoc = new BO.BOLocation(st.Longitude, st.Latitude);
                    if (stLoc == drone.Location)
                        return st.Id;
                }
                //if drone is not charging at station
                return -1;
            }

            double battNededForDist(BO.BOLocation start, BO.BOLocation finish, BO.Enum.WeightCategories? weight = null)
            {

                double dist = HelpfulFunctionsBL.GetDistance(start, finish);

                if (weight != null)
                {
                    if (weight == BO.Enum.WeightCategories.Light)
                        return dist * light;
                    if (weight == BO.Enum.WeightCategories.Medium)
                        return dist * medium;
                    if (weight == BO.Enum.WeightCategories.Heavy)
                        return dist * heavy;
                }
                return dist * empty;
            }
            double battNeededForJourey(BO.BODrone drone, BO.BOLocation Sender,
                BO.BOLocation Receiver, BO.Enum.WeightCategories weight)
            {
                double totalBattery = 0;
                totalBattery += battNededForDist(drone.Location, Sender, weight);                            //drone -> Sender
                totalBattery += battNededForDist(Sender, Receiver, weight);                  //Sender -> Receiver
                totalBattery += battNededForDist(Receiver, getClosestStationLoc(Receiver));//Receiver -> Station

                return totalBattery;
            }

            int freeSpots(DalXml.DataObject.Station st)
            {//returns 0 (or less) if not spots are free...
                lock (dataAccess)
                {
                    int numSpots = st.ChargeSlots;
                    foreach (DalXml.DataObject.DroneCharge drCharge in GetDroneCharges())
                    {
                        if (st.Id == drCharge.StationId)
                            numSpots--;
                    }
                    return numSpots;
                }
            }

            int findClosestParcel(BO.BODrone droneCopy)
            {
                lock (dataAccess)
                {
                    //Explanation:
                    //(1) Only take the relevant Parcels (acc to Drone's max weight)
                    //(2) organize into 3 groups (by Priority), each group with 3 sub groups (by weight)
                    //(3) Traverse the parcels, beginning from best choice. if we can make the journey, take the parcel

                    //Initialize our 2D array:
                    //first dimension - organized by Parcel Priority
                    //second dimesion - organized by weight category - index 0: light, index 1: medium, index 2: heavy
                    List<DalXml.DataObject.Parcel>[,] parcels = new List<DalXml.DataObject.Parcel>[3, 3];
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            parcels[i, j] = new List<DalXml.DataObject.Parcel>();
                        }
                    }
                    const int REGULAR = 0, FAST = 1, URGENT = 2;

                    foreach (var origParcel in dataAccess.GetParcels())
                    {
                        //(1) Take Relevant Parcels
                        if ((origParcel.DroneId == 0 || origParcel.DroneId == null)
                            && (int)origParcel.Weight <= (int)droneCopy.MaxWeight) //if drone can hold parcel
                        {
                            //(2) Fill our 3 Arrays...each with 3 sub groups
                            switch ((DalXml.DataObject.Priorities)origParcel.Priority)
                            {
                                case DalXml.DataObject.Priorities.regular:
                                    if (origParcel.Weight == DalXml.DataObject.WeightCategories.light)
                                        parcels[REGULAR, 0].Add(origParcel);
                                    if (origParcel.Weight == DalXml.DataObject.WeightCategories.medium)
                                        parcels[REGULAR, 1].Add(origParcel);
                                    if (origParcel.Weight == DalXml.DataObject.WeightCategories.heavy)
                                        parcels[REGULAR, 2].Add(origParcel);
                                    break;
                                case DalXml.DataObject.Priorities.fast:
                                    if (origParcel.Weight == DalXml.DataObject.WeightCategories.light)
                                        parcels[FAST, 0].Add(origParcel);
                                    if (origParcel.Weight == DalXml.DataObject.WeightCategories.medium)
                                        parcels[FAST, 1].Add(origParcel);
                                    if (origParcel.Weight == DalXml.DataObject.WeightCategories.heavy)
                                        parcels[FAST, 2].Add(origParcel);
                                    break;
                                case DalXml.DataObject.Priorities.urgent:
                                    if (origParcel.Weight == DalXml.DataObject.WeightCategories.light)
                                        parcels[URGENT, 0].Add(origParcel);
                                    if (origParcel.Weight == DalXml.DataObject.WeightCategories.medium)
                                        parcels[URGENT, 1].Add(origParcel);
                                    if (origParcel.Weight == DalXml.DataObject.WeightCategories.heavy)
                                        parcels[URGENT, 2].Add(origParcel);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    //(3) traverse parcels, choose closest parcel
                    int closestParcelId = -1;
                    BO.BOLocation closestLoc = new BO.BOLocation(0, 0); //distance will be big..

                    for (int i = 2; i >= 0; i--) //i iterates thru parcel priority
                    {
                        for (int j = 2; j >= 0; j--) //j iterates thru weight category
                        {
                            foreach (DalXml.DataObject.Parcel parcel in parcels[i, j])
                            {
                                if (battNeededForJourey(droneCopy, getLocationOfCustomer(parcel.SenderId),
                                    getLocationOfCustomer(parcel.ReceiverId), (BO.Enum.WeightCategories)parcel.Weight) <= droneCopy.Battery)
                                { //if drone can make the journey,

                                    //if this parcel is closer, replace the "closest Parcel":
                                    BO.BOLocation thisParcLoc = new BO.BOLocation(getLocationOfCustomer(parcel.SenderId).Longitude,
                                        getLocationOfCustomer(parcel.SenderId).Latitude);

                                    if (HelpfulFunctionsBL.GetDistance(droneCopy.Location, thisParcLoc) < HelpfulFunctionsBL.GetDistance(droneCopy.Location, closestLoc))
                                    {
                                        closestParcelId = parcel.Id;
                                        closestLoc = thisParcLoc;
                                    }
                                }
                            }
                            if (closestParcelId != -1) //if we found a parcel that fits our criteria
                                return closestParcelId;
                            //else, j-- ; next weight category
                        }
                        //i-- ; next parcel priority
                    }

                    return closestParcelId; //will return -1
                }
            }


            [MethodImpl(MethodImplOptions.Synchronized)]
            public BO.BODrone GetBODrone(int _id) //return reference... 
                                                  //only returns if drone Exists
            {

                foreach (var item in listDrone)
                {
                    if (_id == item.Id && item.Exists)
                        return item;
                }
                //throw exception!!!
                throw new EXDroneNotFound();
                //return null;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public BO.BOCustomer GetBOCustomer(int _id)
            {
                IEnumerable<DalXml.DataObject.Customer> origList = dataAccess.GetCustomers();
                foreach (var item in origList)
                {
                    if (_id == item.Id && item.Exists)
                    {
                        return CreateBOCustomer(_id);
                    }
                }
                //throw exception!!!
                throw new EXCustomerNotFound();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public IEnumerable<BO.BOCustomer> GetAllBOCustomers()
            {
                List<BO.BOCustomer> res = new List<BO.BOCustomer>();
                foreach (var item in dataAccess.GetCustomers())
                {
                    res.Add(CreateBOCustomer(item.Id));
                }
                return res;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public BO.BOCustomerToList GetOneCustToList(int _id)
            {
                return createBOCustToList(_id);
            }


            [MethodImpl(MethodImplOptions.Synchronized)]
            public BO.BOParcel GetBOParcel(int _id)
            {
                IEnumerable<DalXml.DataObject.Parcel> origList = dataAccess.GetParcels();
                foreach (var item in origList)
                {
                    if (_id == item.Id)
                    {
                        return CreateBOParcel(_id);
                    }
                }
                //throw exception!!!
                throw new EXParcelNotFound(); ;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public BO.BOStation GetBOStation(int _stationId)
            {
                foreach (var item in GetStations())
                {
                    if (_stationId == item.Id)
                        return item;
                }
                throw new EXNotFoundPrintException("Station");
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public int GetDroneIdOfParcel(int parcelId)
            {
                DalXml.DataObject.Parcel parc = dataAccess.GetParcel(parcelId);
                if (!parc.Exists)
                    throw new EXParcelNotFound();
                if (parc.DroneId != null)
                    return (int)parc.DroneId;
                else
                    throw new BLApi.EXDroneNotFound();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public IEnumerable<BO.BODrone> GetBODroneList(bool getDeleted = false)
            {
                if (getDeleted)
                    return listDrone; // without filtering out the deleted drones

                ObservableCollection<BO.BODrone> res = new ObservableCollection<BO.BODrone>();
                foreach (var item in listDrone)
                {
                    if (item.Exists)
                        res.Add(item);
                }
                return res;
            }
            private IEnumerable<DalXml.DataObject.Parcel> getParcelListFromData(bool getDeleted = false)
            {
                lock (dataAccess)
                {
                    if (getDeleted)
                        return dataAccess.GetParcels();
                    ObservableCollection<DalXml.DataObject.Parcel> res = new ObservableCollection<DalXml.DataObject.Parcel>();
                    foreach (var item in dataAccess.GetParcels())
                    {
                        if (item.Exists)
                            res.Add(item);
                    }
                    return res;
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public IEnumerable<BO.BODrone> GetSpecificDroneListStatus(int num)
            {
                switch (num)
                {
                    case 0:
                        {
                            return new ObservableCollection<BO.BODrone>(GetBODroneList()
                                .Where(x => x.DroneStatus == BO.Enum.DroneStatus.Available));
                        }
                    case 1:
                        {
                            return new ObservableCollection<BO.BODrone>(GetBODroneList()
                                .Where(x => x.DroneStatus == BO.Enum.DroneStatus.Charging));
                        }
                    case 2:
                        {
                            return new ObservableCollection<BO.BODrone>(GetBODroneList()
                                .Where(x => x.DroneStatus == BO.Enum.DroneStatus.InDelivery));
                        }
                    default:
                        return listDrone;
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public IEnumerable<BO.BODrone> GetSpecificDroneListWeight(int num)
            {
                switch (num)
                {
                    case 0:
                        {
                            return new ObservableCollection<BO.BODrone>(GetBODroneList()
                                .Where(x => x.MaxWeight == BO.Enum.WeightCategories.Light));
                        }
                    case 1:
                        {
                            return new ObservableCollection<BO.BODrone>(GetBODroneList()
                                .Where(x => x.MaxWeight == BO.Enum.WeightCategories.Medium));
                        }
                    case 2:
                        {
                            return new ObservableCollection<BO.BODrone>(GetBODroneList()
                                .Where(x => x.MaxWeight == BO.Enum.WeightCategories.Heavy));
                        }
                    default:
                        return listDrone;
                }
            }

            private static bool availableDrone(BO.BODrone _drone)
            {
                if (_drone.DroneStatus == BO.Enum.DroneStatus.Available)
                    return true;
                else
                    return false;
            }

            private static bool maintenanceDrone(BO.BODrone _drone)
            {
                if (_drone.DroneStatus == BO.Enum.DroneStatus.Charging)
                    return true;
                else
                    return false;
            }
            private static bool inDeliveryDrone(BO.BODrone _drone)
            {
                if (_drone.DroneStatus == BO.Enum.DroneStatus.InDelivery)
                    return true;
                else
                    return false;
            }

            private static bool heavyDrone(BO.BODrone _drone)
            {
                if (_drone.MaxWeight == BO.Enum.WeightCategories.Heavy)
                    return true;
                else
                    return false;
            }

            private static bool mediumDrone(BO.BODrone _drone)
            {
                if (_drone.MaxWeight == BO.Enum.WeightCategories.Medium)
                    return true;
                else
                    return false;
            }

            private static bool lightDrone(BO.BODrone _drone)
            {
                if (_drone.MaxWeight == BO.Enum.WeightCategories.Light)
                    return true;
                else
                    return false;
            }

            //for printing these lists:
            [MethodImpl(MethodImplOptions.Synchronized)]
            public IEnumerable<BO.BOCustomerToList> GetCustToList()
            {
                List<BO.BOCustomerToList> res =
                    new List<BO.BOCustomerToList>();
                foreach (var item in dataAccess.GetCustomers())
                {
                    res.Add(createBOCustToList(item.Id));
                }
                return res;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public IEnumerable<BO.BOParcelToList> GetParcelToList()
            {
                List<BO.BOParcelToList> res = new List<BO.BOParcelToList>();
                foreach (var item in dataAccess.GetParcels())
                {
                    //if(item.Exists)
                    res.Add(createBOParcToList(item.Id));
                }
                return res;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public IEnumerable<BO.BOStationToList> GetStationToList()
            {
                List<BO.BOStationToList> res = new List<BO.BOStationToList>();
                foreach (var item in dataAccess.GetStations())
                {
                    res.Add(createBOStationToList(item.Id));
                }
                return res;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            public BO.BOStationToList GetOneStationToList(int stationId)
            {
                foreach (var item in dataAccess.GetStations())
                {
                    if (item.Id == stationId)
                        return createBOStationToList(item.Id);
                }
                throw new EXNotFoundPrintException("Station");
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public IEnumerable<BO.BOStation> GetStations()
            {
                List<BO.BOStation> res = new List<BO.BOStation>();
                foreach (var item in dataAccess.GetStations())
                {
                    res.Add(CreateBOStation(item.Id));
                }
                return res;
            }
            private IEnumerable<DalXml.DataObject.DroneCharge> GetDroneCharges()
            {
                return dataAccess.GetDroneCharges();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public IEnumerable<BO.BOParcelAtCustomer> GetBOParcelAtCustomerList(BO.BOCustomer customer)
            {
                ObservableCollection<BO.BOParcelAtCustomer> lst = new ObservableCollection<BO.BOParcelAtCustomer>();
                foreach (BO.BOParcelAtCustomer item in customer.ListOfParcReceived)
                    lst.Add(item);
                foreach (BO.BOParcelAtCustomer item in customer.ListOfParcSent)
                    lst.Add(item);
                return lst;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public bool droneIdExists(int id)
            {

                foreach (BO.BODrone item in listDrone)
                {
                    if (id == item.Id && item.Exists)
                        return true;
                }
                return false;
            }


            [MethodImpl(MethodImplOptions.Synchronized)]
            public string GetDroneLocationString(int id) //returns string describing location
                                                         //helpful for debugging, & user convenience
            {
                BO.BODrone bodrone = GetBODrone(id);
                if (bodrone.DroneStatus == BO.Enum.DroneStatus.Charging)
                {
                    foreach (var item in GetDroneCharges())
                    {
                        if (item.DroneId == bodrone.Id)
                            return "At Station " + item.StationId.ToString();
                    }
                }
                else if (bodrone.DroneStatus == BO.Enum.DroneStatus.InDelivery)
                {
                    //if already pickuped up pkg
                    if (bodrone.Location == bodrone.ParcelInTransfer.PickupPoint)
                        return "At Customer " + bodrone.ParcelInTransfer.Sender.Name;
                    else if (bodrone.Location == bodrone.ParcelInTransfer.DeliveryPoint)
                        return "At Customer " + bodrone.ParcelInTransfer.Recipient.Name;
                    else
                        return findAllPossibleLoc(bodrone);
                }
                else if (bodrone.DroneStatus == BO.Enum.DroneStatus.Available)
                {
                    return findAllPossibleLoc(bodrone);
                }

                return "Could not locate..";
            }

            private string findAllPossibleLoc(BO.BODrone bodrone)
            {
                lock (dataAccess)
                {
                    //if at station - after charging
                    foreach (var station in dataAccess.GetStations())
                    {
                        if (bodrone.Location.Longitude == station.Longitude
                            && bodrone.Location.Latitude == station.Latitude)
                            return "At Station " + station.Id.ToString();
                    }
                    //if at customer
                    foreach (var cust in dataAccess.GetCustomers())
                    {
                        if (bodrone.Location.Longitude == cust.Longitude
                          && bodrone.Location.Latitude == cust.Latitude)
                            return "At Customer " + cust.Name;
                    }
                    return "Could not locate..";
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public int GetIdOfUser(string _username, string _password)
            {
                foreach (var item in dataAccess.GetUsers())
                {
                    if (item.Username == _username)
                        if (item.Password != _password)
                            throw new EXUserPasswordIncorrect();
                        else
                            return item.Id;
                }
                //if did not find username at all
                throw new EXUsernameNotFound();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public double GetElectricityRate(BO.BODrone bodrone)
            {
                //IF DRONE CARRYING PARCEL
                if (bodrone.ParcelInTransfer != null
                    && bodrone.ParcelInTransfer.Id != -1)
                {
                    switch (bodrone.MaxWeight)
                    {
                        case BO.Enum.WeightCategories.Light: return light;
                        case BO.Enum.WeightCategories.Medium: return medium;
                        case BO.Enum.WeightCategories.Heavy: return heavy;
                        default: return 0;
                    }
                }
                else // if drone not carrying parcel
                    return empty;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            public int GetNumParcelsWaitingAtCustomer(BO.BOCustomer cust)
            {
                int res = 0;
                foreach (var item in cust.ListOfParcSent)
                {
                    if (item.ParcelStatus == BO.Enum.ParcelStatus.created
                        || item.ParcelStatus == BO.Enum.ParcelStatus.assigned) //but not yet collected...
                        ++res;
                }
                return res;
            }
            public double GetLongitudeBegin()
            {
                return dataAccess.GetLongitudeBegin();
            }
            public double GetLongitudeEnd()
            {
                return dataAccess.GetLongitudeEnd();
            }
            public double GetLatitudeBegin()
            {
                return dataAccess.GetLatitudeBegin();
            }
            public double GetLatitudeEnd()
            {
                return dataAccess.GetLatitudeEnd();
                //end of class definition...
            }
        }
    }
}
 