using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DalXml
{
    namespace DataObject
    {

        public struct Station : INotifyPropertyChanged
        {

            public Station(int _id,int name,double _longitude, double _latitude, 
                int _chargeSlots) : this()
            {
                Id = _id;
                _name = name;
                Longitude = _longitude;
                Latitude = _latitude;
                ChargeSlots = _chargeSlots; //total charge slots 
                Exists = true;
            }
            public int Id { get; set; }
            private int _name { get; set; }
            public double Longitude { get; set; }
            public double Latitude { get; set; }
            public int ChargeSlots { get; set; }
            public bool Exists { get; set; }

            public override string ToString()
            {
                string res = "";
                res += "Station " + Name + " id: " + Id + "\n" +
                    "(" + Longitude + "," + Latitude + ")" + "\n" +
                    "ChargeSlots: " + ChargeSlots + "\n";
                return res;
            }
            public event PropertyChangedEventHandler PropertyChanged;
            public int Name
            {
                get { return _name; }
                set
                {
                    _name = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this,
                            new PropertyChangedEventArgs("Name"));
                    }

                }
            }

        }


    }

}
