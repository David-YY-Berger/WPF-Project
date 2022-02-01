using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    namespace BO
    {
        public class BODrone : INotifyPropertyChanged
        {
            public BODrone()
            {
                Exists = true;
            }
            public int Id { get; set; }
            
            private string _model;
            public BL.BO.Enum.WeightCategories MaxWeight { get; set; }
            public double Battery { get; set; }
            public BL.BO.Enum.DroneStatus DroneStatus { get; set; }
            public BL.BO.BOParcelInTransfer ParcelInTransfer { get; set; }
            public BL.BO.BOLocation Location { get; set; }
            //public bool Exists { get; set; }

            private bool _exists;



            public event PropertyChangedEventHandler PropertyChanged;
            public bool Exists
            {
                get { return _exists; }
                set
                {
                    _exists = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Exists"));
                    }
                    
                }
            }

            public string Model
            {
                get { return _model; }
                set
                {
                    _model = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Model"));
                    }
                }
            }
            public override string ToString()
            {
                string res = "";
                if (!Exists) 
                    res += "DELETED --\n";
                res += "Drone " + Id + " Model: " + Model + " \n";
                res += "Battery: " + Math.Round(Battery, 2) + " \nStatus: " + DroneStatus + "\n";
                res += "MaxWeight: " + MaxWeight + "\n";
                res += "Longitude: " + Math.Round(Location.Longitude, 5)
                    + " Latitude: " + Math.Round(Location.Latitude, 5) + "\n";
                if ((ParcelInTransfer.Id != -1) && ParcelInTransfer.Id != 0)
                    res += ParcelInTransfer.ToString();
                else
                    res += "Not carrying a Parcel";
                return res;
            }



        }
    }


}