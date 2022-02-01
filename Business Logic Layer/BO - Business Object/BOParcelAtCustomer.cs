using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BO
    {
        public class BOParcelAtCustomer
        {
            public int Id { get; set; }
            public BL.BO.Enum.WeightCategories MaxWeight { get; set;}
            public BL.BO.Enum.Priorities Priority { get; set; }
            public BL.BO.Enum.ParcelStatus ParcelStatus { get; set; }
            public BOCustomerInParcel OtherSide { get; set; } //for Sender: holds the receiver
                                                              //for Receiver: holds the sender



            public override string ToString()
            {
                string res = "Parcel " + Id;
                res += "Status: " + ParcelStatus.ToString();

                return res;
            }



        }
    }
    
}
