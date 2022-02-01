using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    static public class HelpfulFunctionsBL
    {
        static public double GetDistance(BO.BOLocation l1, BO.BOLocation l2)
        {
            //(1) find diff in radians:
            double diffLat = l1.Latitude - l2.Latitude;
            double diffLong = l1.Longitude - l2.Longitude;
            diffLat *= (Math.PI / 180);
            diffLong *= (Math.PI / 180);

            //(2)convert latitude to radians
            double lat1 = l1.Latitude * (Math.PI / 180);
            double lat2 = l2.Latitude * (Math.PI / 180);

            //(3) use Haversine Formula
            double Hav = Math.Pow(Math.Sin(diffLat / 2), 2) +
               Math.Pow(Math.Sin(diffLong / 2), 2) *
               Math.Cos(lat1) * Math.Cos(lat2);

            //(4) Find distance in KM based on earth's radius
            //d = 2*radius * ArcSin(Square(Hav))
            double radius = 6371; //radius of Earth in km...
            double distance = 2 * radius * Math.Asin(Math.Sqrt(Hav));

            return distance;
        }

    }
}
