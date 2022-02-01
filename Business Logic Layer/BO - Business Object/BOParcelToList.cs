using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BO
    {
        public class BOParcelToList
        { 
        //    BOParcelToList()
        //    {
        //        Exists = true;
        //    }

            public int Id { get; set;  }
            public string NameSender { get; set;  }
            public string NameReceiver { get; set;  }
            public BL.BO.Enum.WeightCategories Weight { get; set; }
            public BL.BO.Enum.Priorities Priority { get; set;}
            public BL.BO.Enum.ParcelStatus ParcelStatus { get; set; }
            public bool Exists { get; set; }
            public override string ToString()
            {
                string res = "";
                if (!Exists) res += "DELETED --\n";
                res += "Parcel " + Id + " From " + NameSender + " to " + NameReceiver + "\n";
                res += Weight.ToString() + " Priority: " + Priority.ToString() + "\n";
                res += "Status: " + ParcelStatus;
                res += "\n";
                return res;
            }
        }
    }
    
}
