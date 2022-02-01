using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BL
{
    namespace BO
    {
        public class BOStation
        {
            public BOStation()
            {
                Exists = true;
            }
            public int Id { get; set; }
            public int Name { get; set; }
            public BOLocation Location { get; set; }
            public int ChargeSlots { get; set; }
            public List<BODroneInCharge> ListDroneCharge { get; set; }
            public bool Exists { get; set; }

            public override string ToString()
            {
                string res = "";
                if (!Exists) res += "DELETED --\n";
                res += "Station " + Id + " Location: " + Location
                    + "\nCharging Slots: " + ChargeSlots
                    + "\nDrones charging at this station: ";
                foreach (var item in ListDroneCharge)
                {
                    res += " Drone " + item.Id;
                }
                res += "\n";
                return res;
            }
        }
        
    }
}
