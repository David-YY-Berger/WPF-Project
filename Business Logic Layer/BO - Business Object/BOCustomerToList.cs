using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BO
    {
        public class BOCustomerToList
        {

            public BOCustomerToList()
            {
                Exists = true;
            }
            public int Id { get; set; }
            public string CustomerName { get; set; }
            public string Phone { get; set; }
            public int NumParcelsSentDelivered { get; set; }
            public int NumParcelsSentNotDelivered { get; set; }
            public int NumParcelsRecieved { get; set; }
            public int NumParcelsOnWayToCustomer { get; set; }
            public bool Exists { get; set; }

            public override string ToString()
            {
                string res = "";
                if (!Exists) res += "DELETED --\n";
                res += "Customer " + CustomerName + " Id: " + Id + "\n";
                res += "Phone: " + Phone + "\n";
                //res += "Total Parcels: " + (numParcelsSentDelivered + numParcelsSentNotDelivered 
                //    + numParcelsSentDelivered + numParcelsSentNotDelivered).ToString() + "\n";

                res += "Parcels waiting to be delivered: " + NumParcelsSentNotDelivered + "\n";
                res += "Parcels waiting to be received: " + NumParcelsOnWayToCustomer + "\n";
                return res;
            }
        }
    }
    
}
