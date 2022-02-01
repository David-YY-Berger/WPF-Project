using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;


namespace BL
{
    namespace BLApi
    {
        public class stringCodes
        {
            public const string DRONE = "drone";
            public const string CUSTOMER = "customer";
            public const string PARCEL = "parcel";
            public const string STATION = "station";
            public const string PRCL_TO_ASSIGN = "ParcelsNotYetAssigned";
            public const string CHARGING_STATIONS = "availChargingStations";
        }

        public static class FactoryBL
        {
            public static global::BL.BLApi.Ibl GetBL()
            {
                return BLApi.BusinessLayer.Instance;
            }
        }

        public partial class BusinessLayer : global::BL.BLApi.Ibl
        {
            class NestedClass //for Lazy Initialization...
            {
                static NestedClass() { }
                internal static readonly BusinessLayer instance = new BusinessLayer(); //constructed only once
            }

            public static BusinessLayer Instance { get { return NestedClass.instance; } }

            //DalXml.IDal dataAccess = DalXml.FactoryDL.GetDL(DalXml.LibTypes.CodeVersion);
            internal DalXml.IDal dataAccess = DalXml.FactoryDL.GetDL(DalXml.LibTypes.XMLVersion);
            Random r = new Random();

            internal double empty;
            internal double light;
            internal double medium;
            internal double heavy;
            internal double chargeRate;// per second

            List<BO.BODrone> listDrone = new List<BO.BODrone>();
            List<SimulatorBL> listSimulators = new List<SimulatorBL>(); //holds open Simulators

            //Lazy Initialization...
            private BusinessLayer() //Private CTOR - implementing Singleton Design Pattern
            {
                IEnumerable<double> elecInfo = dataAccess.RequestElec();
                empty = elecInfo.First();
                light = elecInfo.ElementAt(1);
                medium = elecInfo.ElementAt(2);
                heavy = elecInfo.ElementAt(3);
                chargeRate = elecInfo.ElementAt(4);

                receiveDronesFromData();
               
                //holds temporary list of locations of customers 
                List<BO.BOLocation> tempListCust = new List<BO.BOLocation>();
                //(for now, tempListCust holds every customer, not just those who have had a parcel delivered to them

                foreach (BO.BODrone drone in listDrone)
                {
                    //assumption that Drone does not have a parcel
                    do //randomly set droneStatus =  "charging" and "available"
                    {
                        drone.DroneStatus = (BO.Enum.DroneStatus)r.Next(0, 3);
                    } while (drone.DroneStatus == BO.Enum.DroneStatus.InDelivery);

                    if (drone.DroneStatus == BO.Enum.DroneStatus.Charging)
                    {
                        //(1) SET LOCATION - to Random Station
                        List<DalXml.DataObject.Station> listStation = dataAccess.GetStations().ToList();
                        DalXml.DataObject.Station st = listStation[r.Next(0, listStation.Count)];
                        drone.Location = new BO.BOLocation(st.Longitude, st.Latitude);

                        //(2) SET BATTERY - btw 50 to 100%
                        drone.Battery = r.Next(50, 100);
                        drone.Battery += r.NextDouble();
                        drone.Battery = Math.Round(drone.Battery,2);
                        AddDroneCharge(drone.Id, st.Id);
                    }
                    else
                    {
                        IEnumerable<DalXml.DataObject.DroneCharge> listDroneCharge = dataAccess.GetDroneCharges();

                        foreach (var item in listDroneCharge)
                        {
                            if (item.DroneId == drone.Id /*&& item.Exists*/)
                                dataAccess.EraseDroneCharge(item);
                        }
                    }
                    if (drone.DroneStatus == BO.Enum.DroneStatus.Available)
                    {
                        //(1) SET LOCATION - to Random Customer's location
                        if (tempListCust.Count == 0) //if not yet full, fill customer list
                            foreach (var item in dataAccess.GetCustomers())
                            {
                                //For now, tempListCust includes every customer,
                                //not just those who have had a parcel already delivered to them
                                BO.BOLocation loc = new BO.BOLocation(item.Longitude, item.Latitude);
                                tempListCust.Add(loc);
                            }

                        drone.Location = tempListCust[r.Next(0, tempListCust.Count())];

                        //(2) SET BATTERY - battNeeded to 100%
                        double minBatteryNeeded = battNededForDist(drone.Location, getClosestStationLoc(drone.Location));
                        double battery = r.Next((int)minBatteryNeeded + 1, 100);
                        battery += r.NextDouble();
                        drone.Battery = Math.Round(battery,2);

                    }
                    try
                    {
                         AssignParcel(drone.Id);
                    }
                    catch (EXNoAppropriateParcel)
                    {
                        continue;
                    }
                    catch (EXDroneUnavailableException)
                    {
                        continue;
                    }
                }
                //end of foreach
                //end of Ctor
            }

            void receiveDronesFromData()
            { 
                IEnumerable<DalXml.DataObject.Drone> origList = dataAccess.GetDrones();
                //receives drones from Data Layer, adds them in listDrone
                foreach (DalXml.DataObject.Drone drone in origList)
                {
                    addDroneToBusiLayer(drone);
                }
            }
            void addDroneToBusiLayer(DalXml.DataObject.Drone drone) //receives IDAL.DO.Drone,
                                                          //creates a corresponding BODrone, saves in BL's list
            {
                BO.BODrone boDrone = new BO.BODrone();
                boDrone.Exists = drone.Exists;
                boDrone.Id = drone.Id;
                switch (drone.MaxWeight)
                {
                    case DalXml.DataObject.WeightCategories.light:
                        boDrone.MaxWeight = BO.Enum.WeightCategories.Light;
                        break;
                    case DalXml.DataObject.WeightCategories.medium:
                        boDrone.MaxWeight = BO.Enum.WeightCategories.Medium;
                        break;
                    case DalXml.DataObject.WeightCategories.heavy:
                        boDrone.MaxWeight = BO.Enum.WeightCategories.Heavy;
                        break;
                    default:
                        break;
                }
                boDrone.Model = drone.Model;
                try
                {
                    boDrone.ParcelInTransfer = createParcInTrans(boDrone.Id);
                    if (boDrone.ParcelInTransfer != null)
                        boDrone.DroneStatus = BO.Enum.DroneStatus.InDelivery;                   
                }
                catch (EXParcInTransNotFoundException exception)
                {
                    boDrone.ParcelInTransfer = exception.creatEmptyParcInTrans();
                }
                listDrone.Add(boDrone);
            }

            int getParcIdFromDroneID(int origDroneId)
            {
                lock (dataAccess)
                {
                    //receives ID of its drone. Fetches correct parcel from Data Layer.
                    IEnumerable<DalXml.DataObject.Parcel> origParcList = dataAccess.GetParcels();

                    //(1)FETCH PARCEL FROM DATA LAYER
                    DalXml.DataObject.Parcel origParcel = new DalXml.DataObject.Parcel();
                    origParcel.Id = -1;
                    foreach (var item in origParcList)
                    {
                        if (origDroneId == item.DroneId && item.Exists)
                        {
                            origParcel = item; break;
                        }
                    }
                    //(2) THROW EXCEPTION IF NOT FOUND
                    if (origParcel.Id == -1) throw new EXParcInTransNotFoundException();

                    return origParcel.Id;
                }
            }

            BO.BOParcelInTransfer createEmptyParcInTrans()
            {
                BO.BOParcelInTransfer thisParc = new BO.BOParcelInTransfer();
                thisParc.Id = -1;
                thisParc.Collected = false;
                thisParc.Priority = (BO.Enum.Priorities)0;
                thisParc.ParcelWeight = (BO.Enum.WeightCategories)0;
                try
                {
                    thisParc.Sender = createCustInParcel(0);
                }
                catch (EXCustInParcNotFoundException exception)
                {
                    thisParc.Sender = exception.creatEmptyCustInParc();
                }
                try
                {
                    thisParc.Recipient = createCustInParcel(0);
                }
                catch (EXCustInParcNotFoundException exception)
                {
                    thisParc.Recipient = exception.creatEmptyCustInParc();
                }
                thisParc.PickupPoint = new BO.BOLocation(0, 0);
                thisParc.DeliveryPoint = new BO.BOLocation(0, 0);
                thisParc.TransportDistance = 0;
                return thisParc;
            }
            BO.BOParcelInTransfer createParcInTrans(int origDroneId, int origParcId = -1) //used in Initialization
            {
                lock (dataAccess)
                {
                    //receives ID of its drone. Fetches correct parcel from Data Layer.
                    //Builds the object based on that parcel
                    BO.BOParcelInTransfer thisParc = new BO.BOParcelInTransfer();

                    //(1)FETCH SPECIFIC PARCEL FROM DATA LAYER
                    IEnumerable<DalXml.DataObject.Parcel> origParcList = dataAccess.GetParcels();

                    DalXml.DataObject.Parcel origParcel = new DalXml.DataObject.Parcel();
                    //assume origParcel it's proper ID
                    if (origParcId == -1) //if caller of this function did not sent Parcel's Id as a parameter
                        origParcel.Id = getParcIdFromDroneID(origDroneId);
                    else
                        origParcel.Id = origParcId;

                    origParcel.SenderId = -1;

                    foreach (var item in origParcList) //sets "origParcel" acc to Parcel saved in data Layer
                    {
                        if (item.Id == origParcel.Id)
                        {
                            origParcel = item; break;
                        }
                    }

                    //(2) THROW EXCEPTION IF NOT FOUND
                    if (origParcel.Id == -1) throw new EXParcInTransNotFoundException();

                    if (origParcel.SenderId == -1) throw new EXParcInTransNotFoundException();

                    //(3) CREATE THIS OBJECT
                    thisParc.Id = origParcel.Id;
                    //thisParc.Collected = (origParcel.TimePickedUp == null) ? false : true;
                    thisParc.Collected = false;
                    thisParc.Priority = (BO.Enum.Priorities)origParcel.Priority;
                    thisParc.ParcelWeight = (BO.Enum.WeightCategories)origParcel.Weight;
                    try
                    {
                        thisParc.Sender = createCustInParcel(origParcel.SenderId);
                    }
                    catch (EXCustInParcNotFoundException exception)
                    {
                        thisParc.Sender = exception.creatEmptyCustInParc();
                    }
                    try
                    {
                        thisParc.Recipient = createCustInParcel(origParcel.ReceiverId);
                    }
                    catch (EXCustInParcNotFoundException exception)
                    {
                        thisParc.Recipient = exception.creatEmptyCustInParc();
                    }

                    thisParc.PickupPoint = getLocationOfCustomer(origParcel.SenderId);
                    thisParc.DeliveryPoint = getLocationOfCustomer(origParcel.ReceiverId);
                    thisParc.TransportDistance = HelpfulFunctionsBL.GetDistance(thisParc.PickupPoint, thisParc.DeliveryPoint);
                    return thisParc;
                }
            }
            BO.BOCustomerInParcel createCustInParcel(int origCustId)
            {
                lock (dataAccess)
                {
                    //function not affected by "Exists" feature
                    IEnumerable<DalXml.DataObject.Customer> origCustomers = dataAccess.GetCustomers();
                    foreach (var item in origCustomers)
                    {
                        if (origCustId == item.Id)
                        {
                            BO.BOCustomerInParcel ans = new BO.BOCustomerInParcel(item.Id, item.Name);
                            return ans;
                        }
                    }
                    //throw exception! not found!
                    throw new EXCustInParcNotFoundException();
                }
            }
            BO.BOParcelAtCustomer createParcAtCust(DalXml.DataObject.Parcel origParc, bool Sender)
            {
                //see later comments for details
                BO.BOParcelAtCustomer newParcAtCust = new BO.BOParcelAtCustomer();
                newParcAtCust.Id = origParc.Id;
                newParcAtCust.MaxWeight = (BO.Enum.WeightCategories)origParc.Weight;
                if (Sender == true) //if the Parcel is being held by a Sender, this field holds the Receiver
                {
                    newParcAtCust.OtherSide = createCustInParcel(origParc.ReceiverId);
                }
                else //if the Parcel is being held by a Receiver, this field holds the Sender
                {
                    newParcAtCust.OtherSide = createCustInParcel(origParc.SenderId);
                } 
                newParcAtCust.Priority = (BO.Enum.Priorities)origParc.Priority;
                //set parcel status:
                if (origParc.TimeDelivered != null) //if delivered..
                    newParcAtCust.ParcelStatus = BO.Enum.ParcelStatus.delivered;
                else if (origParc.TimePickedUp != null)
                    newParcAtCust.ParcelStatus = BO.Enum.ParcelStatus.collected;
                else if (origParc.TimeAssigned != null)
                    newParcAtCust.ParcelStatus = BO.Enum.ParcelStatus.assigned;
                else
                    newParcAtCust.ParcelStatus = BO.Enum.ParcelStatus.created;
                
                return newParcAtCust;
            }

            private BO.BOStation CreateBOStation(int id)
            {
                lock (dataAccess)
                {
                    DalXml.DataObject.Station origSt;
                    try
                    {
                        origSt = dataAccess.GetStation(id);
                    }
                    catch (DalXml.DataObject.EXItemNotFoundException)
                    {
                        throw new EXNotFoundPrintException("Station");
                    }

                    BO.BOStation newSt = new BO.BOStation();
                    newSt.Exists = origSt.Exists;
                    newSt.Id = origSt.Id;
                    newSt.Name = origSt.Name;
                    newSt.Location = new BO.BOLocation(origSt.Longitude, origSt.Latitude);
                    newSt.ChargeSlots = origSt.ChargeSlots;
                    newSt.ListDroneCharge = new List<BO.BODroneInCharge>();

                    foreach (var item in GetDroneCharges()) //create BODroneInCharge and add to list
                    {
                        if (item.StationId == newSt.Id)
                        {
                            try
                            {
                                BO.BODrone copy = GetBODrone(item.DroneId);
                            }
                            catch (BLApi.EXDroneNotFound)
                            {
                                break;
                            }

                            BO.BODroneInCharge d = new BO.BODroneInCharge();
                            d.Id = item.DroneId;
                            d.Battery = GetBODrone(d.Id).Battery;
                            newSt.ListDroneCharge.Add(d);
                        }
                    }
                    return newSt;
                }
            }
            private BO.BOCustomer CreateBOCustomer(int id)
            {
                lock (dataAccess)
                {
                    BO.BOCustomer newCust = new BO.BOCustomer();
                    DalXml.DataObject.Customer origCust = new DalXml.DataObject.Customer();
                    try
                    {
                        origCust = dataAccess.GetCustomer(id);
                    }
                    catch (DalXml.DataObject.EXItemNotFoundException)
                    {
                        throw new EXNotFoundPrintException("Customer");
                    }
                    //throw exception if not found..
                    newCust.Id = origCust.Id;
                    newCust.Exists = origCust.Exists;
                    newCust.Location = new BO.BOLocation(origCust.Longitude, origCust.Latitude);
                    newCust.Name = origCust.Name;
                    newCust.Phone = origCust.Phone;

                    newCust.ListOfParcSent = new List<BO.BOParcelAtCustomer>();
                    newCust.ListOfParcReceived = new List<BO.BOParcelAtCustomer>();
                    foreach (var item in dataAccess.GetParcels())
                    {
                        if (!item.Exists)
                            continue;
                        if (item.SenderId == newCust.Id)
                            newCust.ListOfParcSent.Add(createParcAtCust(item, true));
                        if (item.ReceiverId == newCust.Id)
                            newCust.ListOfParcReceived.Add(createParcAtCust(item, false));
                    }
                    return newCust;
                }
            }

            private BO.BOParcel CreateBOParcel(int id)
            {
                lock (dataAccess)
                {
                    BO.BOParcel newParc = new BO.BOParcel();
                    DalXml.DataObject.Parcel origParc = new DalXml.DataObject.Parcel();
                    try
                    {
                        origParc = dataAccess.GetParcel(id);
                    }
                    //throw exception if not found..
                    catch (DalXml.DataObject.EXItemNotFoundException)
                    {
                        throw new EXNotFoundPrintException("Parcel");
                    }
                    newParc.Id = origParc.Id;
                    newParc.Priority = (BO.Enum.Priorities)origParc.Priority;
                    newParc.Sender = new BO.BOCustomerInParcel(
                        origParc.SenderId, dataAccess.GetCustomer(origParc.SenderId).Name);
                    newParc.Receiver = new BO.BOCustomerInParcel(
                        origParc.ReceiverId, dataAccess.GetCustomer(origParc.ReceiverId).Name);

                    newParc.WeightCategory = (BO.Enum.WeightCategories)origParc.Weight;
                    newParc.TimeOfCreation = origParc.TimeCreated;
                    newParc.TimeOfAssignment = origParc.TimeAssigned;
                    newParc.TimeOfDelivery = origParc.TimeDelivered;
                    newParc.TimeOfCollection = origParc.TimePickedUp;
                    return newParc;
                }
            }

            private BO.BOCustomerToList createBOCustToList(int _id)
            {
                lock (dataAccess)
                {
                    BO.BOCustomerToList newCustToList = new BO.BOCustomerToList();
                    DalXml.DataObject.Customer origCust = dataAccess.GetCustomer(_id);
                    newCustToList.Exists = origCust.Exists;
                    newCustToList.Id = origCust.Id;
                    newCustToList.CustomerName = origCust.Name;
                    newCustToList.Phone = origCust.Phone;
                    newCustToList.NumParcelsOnWayToCustomer = 0;
                    newCustToList.NumParcelsRecieved = 0;
                    newCustToList.NumParcelsSentDelivered = 0;
                    newCustToList.NumParcelsSentNotDelivered = 0;

                    foreach (var item in dataAccess.GetParcels())
                    {
                        if (!item.Exists)
                            continue;
                        if (item.SenderId == newCustToList.Id) //if sent this parcel
                        {
                            if (item.TimeDelivered == null)//if not delivered
                                newCustToList.NumParcelsSentNotDelivered++;
                            else  //if deliverd
                                newCustToList.NumParcelsSentDelivered++;
                        }
                        else if (item.ReceiverId == newCustToList.Id)
                        {
                            if (item.TimeDelivered == null) //if not delivered
                                newCustToList.NumParcelsOnWayToCustomer++;
                            else  //if delivered
                                newCustToList.NumParcelsRecieved++;
                        }
                    }
                    return newCustToList;
                }
            }

            private BO.BODroneToList createBODroneToList(int _id)
            {
                BO.BODroneToList newDroneToList = new BO.BODroneToList();
                BO.BODrone origBODrone = GetBODrone(_id);
                newDroneToList.Id = origBODrone.Id;
                newDroneToList.Model = origBODrone.Model;
                newDroneToList.MaxWeight = origBODrone.MaxWeight;
                newDroneToList.Battery = origBODrone.Battery;
                newDroneToList.Location = origBODrone.Location;
                newDroneToList.IdOfParcelCarrying = origBODrone.ParcelInTransfer.Id;
                return newDroneToList;
            }

            private BO.BOParcelToList createBOParcToList(int _id)
            {
                BO.BOParcelToList newParcToList = new BO.BOParcelToList();
                DalXml.DataObject.Parcel origParcel = dataAccess.GetParcel(_id);

                newParcToList.Id = origParcel.Id;
                newParcToList.Exists = origParcel.Exists;
                newParcToList.NameReceiver = dataAccess.GetCustomer(origParcel.ReceiverId).Name;
                newParcToList.NameSender = dataAccess.GetCustomer(origParcel.SenderId).Name;
                newParcToList.Weight = (BO.Enum.WeightCategories)origParcel.Weight;
                newParcToList.Priority = (BO.Enum.Priorities)origParcel.Priority;

                if (origParcel.TimeDelivered != null) //if delivered
                    newParcToList.ParcelStatus = BO.Enum.ParcelStatus.delivered;
                else if (origParcel.TimePickedUp != null) // if collected
                    newParcToList.ParcelStatus = BO.Enum.ParcelStatus.collected;
                else if (origParcel.TimeAssigned != null)
                    newParcToList.ParcelStatus = BO.Enum.ParcelStatus.assigned;
                else
                    newParcToList.ParcelStatus = BO.Enum.ParcelStatus.created;
               return newParcToList;
            }

            private BO.BOStationToList createBOStationToList(int _id)
            {
                BO.BOStationToList newStationToList = new BO.BOStationToList();
                DalXml.DataObject.Station origStation = dataAccess.GetStation(_id);

                newStationToList.Id = origStation.Id;
                newStationToList.NameStation = origStation.Name;
                newStationToList.ChargeSlotsAvailable = freeSpots(origStation);
                newStationToList.ChargeSlotsTaken = origStation.ChargeSlots - freeSpots(origStation);
                newStationToList.Exists = origStation.Exists;

                return newStationToList;
            }            
            //end of class
        }
    }
}

