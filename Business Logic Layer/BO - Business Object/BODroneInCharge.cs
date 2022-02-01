using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BO
    {
        public class BODroneInCharge
        {
            public int Id { get; set; }
            public double Battery { get; set; }
            public override string ToString()
            {
                string res = "";
                res += "Drone " + Id.ToString() + "\n"
                 + "Battery: " + Battery.ToString() + "\n";
                return res;
            }
        }
       
    }
}
