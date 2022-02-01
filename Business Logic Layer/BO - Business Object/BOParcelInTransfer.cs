using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL 
{
    namespace BO
    {

        public class BOParcelInTransfer
        {

            public int Id { get; set; } // if id == -1, then parcel is empty..
            public bool Collected { get; set; } //true = collected, in transit
                                                //false = not yet collected
            public BL.BO.Enum.Priorities Priority { get; set; }
            public BL.BO.Enum.WeightCategories ParcelWeight { get; set; }
            public BL.BO.BOCustomerInParcel Sender { get; set; }
            public BL.BO.BOCustomerInParcel Recipient { get; set; }
            public BL.BO.BOLocation PickupPoint { get; set; }
            public BL.BO.BOLocation DeliveryPoint { get; set; }
            public double TransportDistance { get; set; }


            public override string ToString()
            {
                string res = "Parcel " + Id + ": from " + Sender.Name + " to " + Recipient + "\n";

                return res;
            }


        }
    }
}