using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BLApi
    {
        public class EXAlreadyExistsPrintException:Exception
        {
            public string ItemName { get; }
            public EXAlreadyExistsPrintException(string name)
            {
                ItemName = name;
            }

            public string printException()
            {
                return ItemName + " already exists!";
            }
        }
        public class EXDroneAlreadyExists : EXAlreadyExistsPrintException
        {
            public EXDroneAlreadyExists() : base("Drone") { }
        }
        public class EXCustomerAlreadyExists : EXAlreadyExistsPrintException
        {
            public EXCustomerAlreadyExists() : base("Customer") { }
        }
        public class EXParcelAlreadyExists : EXAlreadyExistsPrintException
        {
            public EXParcelAlreadyExists() : base("Parcel") { }
        }
        public class EXStationAlreadyExists : EXAlreadyExistsPrintException
        {
            public EXStationAlreadyExists() : base("Station") { }
        }
        public class EXUserAlreadyExists : EXAlreadyExistsPrintException
        {
            public EXUserAlreadyExists() : base("User") { }
        }
    }
}
