using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BL
{
    namespace BLApi
    {
        public class EXCannotDelete : Exception
        {
            public string ItemName { get; set; }
            public string ReasonCannotDelete { get; set; }

            public EXCannotDelete(string _item, string _reason)
            {
                ItemName = _item;
                ReasonCannotDelete = _reason;
            }
            public override string ToString()
            {
                return "Cannot delete this " + ItemName + ", because " + ReasonCannotDelete;
            }

        }

        public class EXCantDltDroneWParc : EXCannotDelete
        {
            //cannot delete Drone With a Parcel
            public EXCantDltDroneWParc() : base("Drone", "it is carrying a parcel") { }
        }

        public class EXCantDltParNotYetDelivered : EXCannotDelete
        {
            public EXCantDltParNotYetDelivered() : 
                base("Parcel", "it is assigned, but not yet delivered") { }
        }
        public class EXCantDltParAlrdyAssgndToDrone : EXCannotDelete
        {
            public EXCantDltParAlrdyAssgndToDrone(int droneId) :
                base("Parcel", "it is already assigned to a Drone " + droneId.ToString())
            { }
        }
        public class EXCantDltStationWDroneCharging : EXCannotDelete
        {
            public EXCantDltStationWDroneCharging() : 
                base("Station", "there are drones charging at this station") { }
        }
        public class EXCantDltCustWParcInDelivery : EXCannotDelete
        {
            public EXCantDltCustWParcInDelivery() :
                base("Customer", "this customer has a parcel assigned or in delivery") { }
        }









    }
}