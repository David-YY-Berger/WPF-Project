using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BO
    {
        public class BOParcel
        {
            public BOParcel()
            {
                Exists = true;
            }
            public int Id { get; set; }
            public BOCustomerInParcel Sender { get; set; }
            public BOCustomerInParcel Receiver { get; set; }
            public BL.BO.Enum.WeightCategories WeightCategory { get; set; }
            public BL.BO.Enum.Priorities Priority { get; set; }
            public DateTime? TimeOfCreation { get; set; } 
            public DateTime? TimeOfAssignment { get; set; }
            public DateTime? TimeOfCollection { get; set; }
            public DateTime? TimeOfDelivery { get; set; }
            public bool Exists { get; set; }


            public override string ToString()
            {
                string res = "Parcel " + Id + "From " + Sender + " to " + Receiver + "\n";
                res += (BL.BO.Enum.WeightCategories)WeightCategory + " Priority: " + Priority + "\n";
                res += "created: " + TimeOfCreation.ToString() + "\n";
                res += "assigned: " + TimeOfAssignment.ToString() + "\n";
                res += "collected: " + TimeOfCollection.ToString() + "\n";
                res += "deliverd: " + TimeOfDelivery.ToString() + "\n"; 

                res += "\n";
                return res;
            }

        }
    }
    
}
