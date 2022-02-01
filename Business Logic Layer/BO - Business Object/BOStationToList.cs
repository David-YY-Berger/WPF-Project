using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BL
{
    namespace BO
    {
        public class BOStationToList : INotifyPropertyChanged
        {
            public BOStationToList()
            {
                Exists = true;
            }
            public int Id { get; set; }
            private int _nameStation { get; set; }
            public int ChargeSlotsAvailable { get; set; }
            public int ChargeSlotsTaken { get; set; }
            public bool Exists { get; set; }
            public override string ToString()
            {
                string res = "";
                if (!Exists) res += "DELETED --\n";
                res += "Station " + Id + " " + "Name:" + _nameStation.ToString()
                    + "\nCharging Slots available: " + ChargeSlotsAvailable.ToString() + "\n"
                    + ChargeSlotsTaken.ToString() + " drones charging at this station";
               
                res += "\n";
                return res;
            }


            public event PropertyChangedEventHandler PropertyChanged;
            public int NameStation
            {
                get { return _nameStation; }
                set
                {
                    _nameStation = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this,
                            new PropertyChangedEventArgs("NameStation"));
                    }

                }
            }



        }
    }
   
}
