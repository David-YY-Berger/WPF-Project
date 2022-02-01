using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BO
    {
        public class BOLocation
        {
            public double Longitude { get; set; }
            public double Latitude { get; set; }
            public BOLocation(double longi, double lati)
            {
                Longitude = longi;
                Latitude = lati;
            }

            public override string ToString()
            {
                string res = "";
                res += "Long: " + Longitude +"Lat: " + Latitude + " "; 
                return res;
            }

        }

    }
}
