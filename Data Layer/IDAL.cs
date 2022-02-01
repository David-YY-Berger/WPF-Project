using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DalXml
{
    public interface IDal
    {
        void Initialize();

        DalXml.DataObject.Drone GetDrone(int _id);
        DalXml.DataObject.Customer GetCustomer(int _id);
        DalXml.DataObject.Parcel GetParcel(int _id);
        DalXml.DataObject.Station GetStation(int _id);
        DalXml.DataObject.DroneCharge GetDroneCharge(int _droneId);

        IEnumerable<double> RequestElec();
        IEnumerable<DalXml.DataObject.Drone> GetDrones();
        IEnumerable<DalXml.DataObject.Parcel> GetParcels();
        IEnumerable<DalXml.DataObject.Station> GetStations();

        IEnumerable<DalXml.DataObject.Customer> GetCustomers();
        IEnumerable<DalXml.DataObject.DroneCharge> GetDroneCharges();
        int GetIdFromUser(DalXml.DataObject.User _user);
        IEnumerable<DalXml.DataObject.User> GetUsers();
        double GetLongitudeBegin();
        double GetLongitudeEnd();
        double GetLatitudeBegin();
        double GetLatitudeEnd();

        //Add:
        void AddCustomer(DalXml.DataObject.Customer custom);
        void AddDrone(DalXml.DataObject.Drone drone);
        void AddParcel(DalXml.DataObject.Parcel parcel);
        void AddStation(DalXml.DataObject.Station st);
        void AddDroneCharge(DalXml.DataObject.DroneCharge droneCharge);
        void AddUser(DalXml.DataObject.User _user);


        //Modify:
        public void ModifyDrone(int _id, string _model);
        public void ModifyCust(int _id, string _name, string _phone);
        public void ModifyStation(int _id, int _name, int _totalChargeSlots);
        public void ModifyParcel(int _id, DalXml.DataObject.Priorities? _priority);

        //Erase:
        void EraseDroneCharge(DalXml.DataObject.DroneCharge thisDroneCharge);
        void EraseDrone(int droneId);
        void EraseCustomer(int custId);
        void EraseStation(int stationId);
        void EraseParcel(int parcelId);

        //Update:
        void AssignDroneToParcel(int droneId, int parcelId);
        void PickupParcel(int parcelId);
        void DeliverParcel(int parcelId);
    }


}

namespace DalXml
{
    public enum LibTypes {XMLVersion, CodeVersion };

    public static class FactoryDL
    {
        public static DalXml.IDal GetDL(LibTypes libraryType)
        {
            if (libraryType == LibTypes.XMLVersion)
                return DalXml.DataSource.Instance;
            else if (libraryType == LibTypes.CodeVersion)
                return DalXml.DataSource.Instance;
            else
                throw new DalXml.DataObject.EXItemNotFoundException();
        }
    }
}


