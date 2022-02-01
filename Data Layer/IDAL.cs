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

        DalXml.DO.Drone GetDrone(int _id);
        DalXml.DO.Customer GetCustomer(int _id);
        DalXml.DO.Parcel GetParcel(int _id);
        DalXml.DO.Station GetStation(int _id);
        DalXml.DO.DroneCharge GetDroneCharge(int _droneId);

        IEnumerable<double> RequestElec();
        IEnumerable<DalXml.DO.Drone> GetDrones();
        IEnumerable<DalXml.DO.Parcel> GetParcels();
        IEnumerable<DalXml.DO.Station> GetStations();

        IEnumerable<DalXml.DO.Customer> GetCustomers();
        IEnumerable<DalXml.DO.DroneCharge> GetDroneCharges();
        int GetIdFromUser(DalXml.DO.User _user);
        IEnumerable<DalXml.DO.User> GetUsers();
        double GetLongitudeBegin();
        double GetLongitudeEnd();
        double GetLatitudeBegin();
        double GetLatitudeEnd();

        //Add:
        void AddCustomer(DalXml.DO.Customer custom);
        void AddDrone(DalXml.DO.Drone drone);
        void AddParcel(DalXml.DO.Parcel parcel);
        void AddStation(DalXml.DO.Station st);
        void AddDroneCharge(DalXml.DO.DroneCharge droneCharge);
        void AddUser(DalXml.DO.User _user);


        //Modify:
        public void ModifyDrone(int _id, string _model);
        public void ModifyCust(int _id, string _name, string _phone);
        public void ModifyStation(int _id, int _name, int _totalChargeSlots);
        public void ModifyParcel(int _id, DalXml.DO.Priorities? _priority);

        //Erase:
        void EraseDroneCharge(DalXml.DO.DroneCharge thisDroneCharge);
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
                throw new DalXml.DO.EXItemNotFoundException();
        }
    }
}


