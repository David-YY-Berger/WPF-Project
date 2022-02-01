using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BLApi
    {
        public partial class BusinessLayer : Ibl
        {
            //ADD
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AddDrone(int _id, string _model, DalXml.DataObject.WeightCategories _maxWeight, int _stationId)
            {
                foreach (var item in GetBODroneList(true)) 
                    //do not allow user to add new drone with previous , even if deleted
                {
                    if (item.Id == _id)
                        throw new EXDroneAlreadyExists();
                }

                DalXml.DataObject.Drone newDOdrone = new DalXml.DataObject.Drone(_id, _model, _maxWeight);
                dataAccess.AddDrone(newDOdrone); //adds to DL

                //adds to BL, assuming the drone is charging at station
                BO.BODrone boDrone = new BO.BODrone();
                boDrone.Id = _id;
                boDrone.MaxWeight = (global::BL.BO.Enum.WeightCategories)_maxWeight;
                boDrone.Model = _model;
                boDrone.Battery = r.Next(20, 40) + r.NextDouble();
                boDrone.DroneStatus = global::BL.BO.Enum.DroneStatus.Charging;
                boDrone.Location = getLocationOfStation(_stationId);
                boDrone.ParcelInTransfer = createEmptyParcInTrans(); //No Parcel is Collected
                                                     //nor Delivered in original Initializtion
                listDrone.Add(boDrone);
                AddDroneCharge(_id, _stationId);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AddCustomer(int _id, string _name, string _phone, double _longitude,
                    double _latitude)
            {
                foreach (var item in dataAccess.GetDrones())
                {
                    if (item.Id == _id)
                        throw new EXCustomerAlreadyExists();
                }
                DalXml.DataObject.Customer newCust = new DalXml.DataObject.Customer(_id, _name, _phone, _longitude, _latitude);
                dataAccess.AddCustomer(newCust);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AddDroneCharge(int _droneId, int _stationId)
            {
                DalXml.DataObject.DroneCharge newDroneCharge = new DalXml.DataObject.DroneCharge(_droneId, _stationId);
                dataAccess.AddDroneCharge(newDroneCharge);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AddParcel(int _senderId, int _targetId, DalXml.DataObject.WeightCategories? _weight,
                             DalXml.DataObject.Priorities? _priority)// DateTime _requested, DateTime _scheduled)
            {
                //DO NOT WRITE AN EXCEPTION FOR "ALREADY EXISTS!!"
                try
                {
                    GetBOCustomer(_senderId);
                }
                catch (EXCustomerNotFound)
                {
                    throw new EXSenderNotFound(); //throw forward to PL
                }
                try
                {
                    GetBOCustomer(_targetId);
                }
                catch (EXCustomerNotFound)
                {
                     throw new EXReceiverNotFound();
                }
                DalXml.DataObject.Parcel dummy = new DalXml.DataObject.Parcel(_senderId, _targetId, _weight, _priority);
                dataAccess.AddParcel(dummy);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AddStation(int _id, int _name, double _longitude, double _latitude, int _chargeSlots)
            {
                foreach (var item in dataAccess.GetStations())
                {
                    if (item.Id == _id)
                        throw new EXStationAlreadyExists();
                }
                DalXml.DataObject.Station dummy = new DalXml.DataObject.Station(_id, _name, _longitude, _latitude, _chargeSlots);
                dataAccess.AddStation(dummy);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AddUser(string _username, string _password, int _id = -1)
            {
                foreach (var item in dataAccess.GetUsers())
                {
                    if (item.Username == _username
                        || item.Id == _id)
                        throw new EXUserAlreadyExists();
                }
                DalXml.DataObject.User newUser = new DalXml.DataObject.User();
                newUser.Id = _id; // id = -1 for employees
                newUser.Username = _username;
                newUser.Password = _password;
                dataAccess.AddUser(newUser);
            }

            //Modify
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void ModifyDrone(int _id, string _model)
            {
                try
                {
                    dataAccess.ModifyDrone(_id, _model); //udpates drone in Data Layer
                }
                catch (DalXml.DataObject.EXItemNotFoundException)
                {
                    throw new EXNotFoundPrintException("Drone");
                }
                //update drone in Business layer:
                foreach (var item in listDrone)
                {
                    if (item.Id == _id)
                    {
                        global::BL.BO.BODrone copy = item;
                        listDrone.Remove(copy);
                        copy.Model = _model;
                        listDrone.Add(copy);
                        return;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void ModifyCust(int _id, string _name, string _phone)
            {
                try
                {
                    dataAccess.ModifyCust(_id, _name, _phone);
                }
                catch (DalXml.DataObject.EXItemNotFoundException)
                {
                    throw new EXNotFoundPrintException("Custumer");
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void ModifyStation(int _id, int _name, int _totalChargeSlots)
            {
                try
                {
                    dataAccess.ModifyStation(_id, _name, _totalChargeSlots);
                }
                catch (DalXml.DataObject.EXItemNotFoundException)
                {
                    throw new EXNotFoundPrintException("Station");
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void ModifyParcel(int _id, BO.Enum.Priorities? _priority)
            {
                dataAccess.ModifyParcel(_id, (DalXml.DataObject.Priorities)_priority);
            }

            //UPDATE ACTIONS
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void AssignParcel(int droneId)  //drone determines its parcel based on algorithm
            {
                global::BL.BO.BODrone droneCopy = new global::BL.BO.BODrone();
                //(1) find drone
                try
                {
                    droneCopy = GetBODrone(droneId);
                }
                catch (EXNotFoundPrintException)
                {
                    throw new EXNotFoundPrintException("Drone");
                }

                //(2)check if drone is avail
                if (droneCopy.DroneStatus != global::BL.BO.Enum.DroneStatus.Available)
                    //    throw exception not available
                    throw new EXDroneUnavailableException();

                //(3) find closest parcel
                int closestParcelId = findClosestParcel(droneCopy); //check if enough batt to make journey
                if (closestParcelId == -1)
                    //throw exception no closest parcel --> dont assign drone, continue in menu...
                    throw new EXNoAppropriateParcel();

                //(4) assign parcel to drone
                droneCopy.DroneStatus = global::BL.BO.Enum.DroneStatus.InDelivery;
                droneCopy.ParcelInTransfer = createParcInTrans(droneCopy.Id, closestParcelId);

                dataAccess.AssignDroneToParcel(droneId, closestParcelId);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void PickupParcel(int droneId, bool dontUpdateBatteryOrLocation = false) //drone collects its pre-determined parcel 
            {
                BO.BODrone drone = new BO.BODrone();
                try
                {
                    drone = GetBODrone(droneId);
                }
                catch (EXNotFoundPrintException)
                {
                    throw new EXDroneNotFound();
                }
                if (drone.DroneStatus != global::BL.BO.Enum.DroneStatus.InDelivery)
                    throw new EXDroneNotAssignedParcel();
                if (drone.ParcelInTransfer.Collected)
                    throw new EXParcelAlreadyCollected();

                foreach (var bodrone in listDrone)
                {
                    if (bodrone.Exists && bodrone.Id == droneId)
                    {
                        dataAccess.PickupParcel(bodrone.ParcelInTransfer.Id);
                        bodrone.ParcelInTransfer.Collected = true;
                        if (dontUpdateBatteryOrLocation)
                            return;
                        //otherwise, update battery and location..
                        BO.BOLocation custLoc = getLocationOfCustomer(bodrone.ParcelInTransfer.Sender.Id);
                        bodrone.Battery -= battNededForDist(bodrone.Location, custLoc, 
                            bodrone.ParcelInTransfer.ParcelWeight);
                        bodrone.Location = custLoc;
                        return;
                    }
                }
                //throw Exception //parcel not collected!
                throw new EXMiscException("Parcel not collected!");
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void DeliverParcel(int droneId, 
                bool dontUpdateBatteryOrLocation = false) //drone delivers its pre-determined parcel
            {
                BO.BODrone drone;
                try
                {
                    drone = GetBODrone(droneId);
                }
                catch (EXNotFoundPrintException) 
                {
                    throw new EXDroneNotFound(); 
                }
                if (drone.DroneStatus != global::BL.BO.Enum.DroneStatus.InDelivery)
                     throw new EXDroneNotAssignedParcel();
                if (!drone.ParcelInTransfer.Collected)
                    throw new EXParcelNotCollected();

                //Deliver the Parcel, updating the bodrone's details accordingly
                foreach (var bodrone in listDrone)
                {
                    if (bodrone.Exists && bodrone.Id == droneId)
                    {
                        global::BL.BO.BOLocation custLoc = getLocationOfCustomer(bodrone.ParcelInTransfer.Recipient.Id);
                        
                        bodrone.DroneStatus = BO.Enum.DroneStatus.Available;
                        dataAccess.DeliverParcel(bodrone.ParcelInTransfer.Id);
                        bodrone.ParcelInTransfer = createEmptyParcInTrans(); //sets Id as -1

                        if (dontUpdateBatteryOrLocation)
                            return;
                        bodrone.Battery -= battNededForDist(bodrone.Location, custLoc,
                                             bodrone.ParcelInTransfer.ParcelWeight);
                        bodrone.Location = custLoc;
                        return;
                    }
                }
                //throw Exception //parcel not delivered!
                throw new EXMiscException("parcel not delivered!");
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void ChargeDrone(int droneId, bool dontUpdateBatteryOrLocation = false) //sends drone to available station
            {
                BO.BODrone drone;
                try
                {
                    drone = GetBODrone(droneId);
                }
                catch (EXNotFoundPrintException) { throw new EXNotFoundPrintException("Drone"); }
                if (drone.DroneStatus != global::BL.BO.Enum.DroneStatus.Available)
                    //    throw exception //drone unavailable - return to main menu..
                    throw new EXDroneUnavailableException();

                global::BL.BO.BOLocation closestStationLoc = getClosestStationLoc(drone.Location, true);
                try
                {
                    AddDroneCharge(drone.Id, this.getStationFromLoc(closestStationLoc).Id);
                }
                catch (EXNotFoundPrintException)
                { 
                    throw new EXNotFoundPrintException("Station"); 
                }
                catch (EXNoStationWithAvailChargingSlots)
                {
                    throw;
                }
                //if successful, station's available charging slots update automatically            
                if (dontUpdateBatteryOrLocation == true)
                    return;
                drone.DroneStatus = global::BL.BO.Enum.DroneStatus.Charging;
                //UPDATE BATTERY AND LOCATION:
                if (drone.Battery < battNededForDist(drone.Location, closestStationLoc))
                    throw new EXMiscException("not enough battery to make to closet station!");
    
                drone.Battery -= battNededForDist(drone.Location, closestStationLoc);
                drone.Location = closestStationLoc;               
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void FreeDrone(int droneId, DateTime timeLeftStation, 
                bool dontUpdateBatteryOrLocation = false) //frees drone from station.. 
            {
                global::BL.BO.BODrone drone;
                try
                {
                    drone = GetBODrone(droneId);
                }
                catch (EXNotFoundPrintException) { throw new EXNotFoundPrintException("Drone"); }

                if (drone.DroneStatus != BO.Enum.DroneStatus.Charging)
                    //    throw Exception //drone not charging! return to main menu
                    throw new EXMiscException("drone not charging!");

                DateTime startTime = DateTime.MinValue;
                foreach (var item in GetDroneCharges())
                {
                    if (item.DroneId == droneId)
                    {
                        startTime = item.timeBeganCharging;
                        break;
                    }
                }
                if (startTime == DateTime.MinValue)
                    return; //throw exception

                TimeSpan ts = timeLeftStation - startTime;
                double secondsInCharge = ts.TotalSeconds;
                double batteryGained = chargeRate * secondsInCharge;
                
                //update drone...
                 dataAccess.EraseDroneCharge(dataAccess.GetDroneCharge(droneId));
                
                BO.BODrone bodrone = GetBODrone(droneId);
                bodrone.DroneStatus = BO.Enum.DroneStatus.Available;
                if (dontUpdateBatteryOrLocation)
                    return;
                //otherwise, update battery...
                bodrone.Battery += batteryGained;
                if (bodrone.Battery > 100)
                    bodrone.Battery = 100;                
            }

            //ERASE:
            [MethodImpl(MethodImplOptions.Synchronized)]
            public void EraseDrone(int droneId)
            {
                BO.BODrone copy = GetBODrone(droneId);
                if(copy.DroneStatus == BO.Enum.DroneStatus.InDelivery)
                {
                    //if drone is carrying a Parcel...
                    throw new EXCantDltDroneWParc();
                }

                foreach (var item in listDrone)
                {
                    if (item.Id == droneId)
                    {
                        //UPDATES IN BL
                        item.Exists = false;
                        //UPDATES IN DL
                        dataAccess.EraseDrone(droneId); 
                        if(item.DroneStatus == BO.Enum.DroneStatus.Charging)
                            dataAccess.EraseDroneCharge(dataAccess.GetDroneCharge(droneId));
                       
                        return;
                    }
                }
                throw new EXDroneNotFound();
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void EraseCustomer(int _id) //FUNCTION NOT COMPLETE!!!!
            {
                //CHECK IF CUSTOMER HAS A PARCEL IN DELIVERY....throw exception..
                BO.BOCustomer cust = CreateBOCustomer(_id);
                //create list of his parcels:
                List<BO.BOParcelAtCustomer> allOfCustParcels = new List<BO.BOParcelAtCustomer>();

                foreach (var item in cust.ListOfParcReceived)
                    allOfCustParcels.Add(item);
                foreach (var item in cust.ListOfParcSent)
                    allOfCustParcels.Add(item);

                foreach (var item in allOfCustParcels)
                {
                    if (item.ParcelStatus == BO.Enum.ParcelStatus.assigned
                        || item.ParcelStatus == BO.Enum.ParcelStatus.collected)
                        throw new EXCantDltCustWParcInDelivery();
                }
                
                //IF NO PROBLEMS, DELETE CUSTOMER AND HIS PARCELS 
                foreach (var item in allOfCustParcels)
                {
                    EraseParcel(item.Id);
                }

                //delete Customer from Data Layer
                foreach (var item in dataAccess.GetCustomers())
                {
                    if (item.Id == _id)
                    {
                        dataAccess.EraseCustomer(_id);
                        return;
                    }
                }
                //erase customer from userlist!
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void EraseStation(int id)
            {
                //check if station has drones charging
                BO.BOStation st = CreateBOStation(id);
                if (st.ListDroneCharge.Count != 0)
                    throw new EXCantDltStationWDroneCharging();
                dataAccess.EraseStation(id);
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void EraseParcel(int parcelId)
            {
                //check..
                BO.BOParcel parc = CreateBOParcel(parcelId);
                if (parc.TimeOfAssignment != null  //if parcel was assigned to a drone
                    && parc.TimeOfDelivery == null)    //and not yet delivered
                    throw new EXCantDltParAlrdyAssgndToDrone(GetDroneIdOfParcel(parcelId));
                
              //throw new EXCantDltParNotYetDelivered();

                foreach (var item in dataAccess.GetParcels())
                {
                    if (item.Id == parcelId)
                    {
                        dataAccess.EraseParcel(parcelId);
                        return;
                    }
                }
            }
        }
    }
}