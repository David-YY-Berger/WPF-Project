using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BO
    {
        public class BODroneToList
        {
            public BODroneToList()
            {
                Exists = true;
            }
            public int Id { get; set; }
            public string Model { get; set; }
            public BL.BO.Enum.WeightCategories MaxWeight { get; set; }
            public double Battery { get; set; }
            public BOLocation Location { get; set; }
            public int IdOfParcelCarrying { get; set; }
            public bool Exists { get; set; }

            public override string ToString()
            {
                string res = "";
                if (!Exists) res += "DELETED --";
                res += "Drone " + Id + " Model: " + Model + " \n";
                res += "Battery: " + Math.Round(Battery, 2) + " Location " + Location + "\n";
                if (IdOfParcelCarrying != 0 && IdOfParcelCarrying != -1)
                    res += "carrying parcel " + IdOfParcelCarrying + "\n";
                
                return res;
            }
        }
    }
    
}
