using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{

    namespace BLApi
    {

        public interface Ibl
        {
            //ADD:
            void AddDrone(int _id, string _model, DalXml.DataObject.WeightCategories _maxWeight, int _stationId = 0);
            void AddCustomer(int _id, string _name, string _phone, double _longitude,
                    double _latitude);
            void AddDroneCharge(int _droneId, int _stationId);
            void AddParcel(int _senderId, int _targetId, DalXml.DataObject.WeightCategories? _weight,
                              DalXml.DataObject.Priorities? _priority);// DateTime _requested,
                                                            //DateTime _scheduled);
            void AddStation(int _id, int _name, double _longitude, double _latitude, int _chargeSlots);
            void AddUser(string _username, string _password, int _id = -1);

            //GETTERS:
            double GetChargeRate();
            int GetNumParcelsWaitingAtCustomer(BO.BOCustomer cust);
            BO.BODrone GetBODrone(int id);
            BO.BOCustomer GetBOCustomer(int id);
            BO.BOParcel GetBOParcel(int id);
            BO.BOStation GetBOStation(int stationId);
            public int GetStationIdOfBODrone(int droneId);
            IEnumerable<BO.BODrone> GetBODroneList(bool getDeleted = false);
            IEnumerable<BO.BOParcelAtCustomer> GetBOParcelAtCustomerList(BO.BOCustomer customer);
            IEnumerable<BO.BOStation> GetStations();
            string GetDroneLocationString(int droneId);//returns string describing drone's location
            int GetDroneIdOfParcel(int parcelId);
            int GetIdOfUser(string _username, string _password);
            double GetElectricityRate(BO.BODrone bodrone);
            IEnumerable<BO.BOCustomer> GetAllBOCustomers();
            IEnumerable<BO.BOCustomerToList> GetCustToList();
            BO.BOCustomerToList GetOneCustToList(int _id);
            IEnumerable<BO.BOParcelToList> GetParcelToList();
            IEnumerable<BO.BOStationToList> GetStationToList();
            BO.BOStationToList GetOneStationToList(int stationId);
            IEnumerable<BO.BODrone> GetSpecificDroneListStatus(int num);
            IEnumerable<BO.BODrone> GetSpecificDroneListWeight(int num);
            double GetLongitudeBegin();
            double GetLongitudeEnd();
            double GetLatitudeBegin();
            double GetLatitudeEnd();

            //Modify:
            public void ModifyDrone(int _id, string _model);
            public void ModifyCust(int _id, string _name, string _phone);
            public void ModifyStation(int _id, int _name, int _totalChargeSlots);
            public void ModifyParcel(int _id, BO.Enum.Priorities? _priority);
            
            //UPDATES:
            void ChargeDrone(int droneId, bool dontUpdateBatteryLocation = false); //sends drone to available station
            void FreeDrone(int droneId, DateTime timeLeftStation,
                 bool dontUpdateBatteryOrLocation = false); //frees drone from station..                                                                                  
            public void AssignParcel(int droneId); 
            //drone determines its parcel based on algorithm
            void PickupParcel(int droneId, bool dontUpdateBatteryOrLocation = false); 
            //drone collects its pre-determined parcel
            void DeliverParcel(int droneId, bool dontUpdateBatteryOrLocation = false); //drone delivers its pre-determined parcel

            //Erase
            void EraseDrone(int droneId);
            void EraseCustomer(int id);
            void EraseStation(int id);
            void EraseParcel(int id);

            //SIMULATOR
            void BeginSimulatorForDrone(int droneId/*, Action<int> updatesToDo, Func<bool> stopSimulator*/);
            void StopSimulatorForDrone(int _droneId);

            //end of interface
        }
    }
}
