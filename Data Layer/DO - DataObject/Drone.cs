using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace DalXml
{
    namespace DataObject
    {
        public struct Drone : INotifyPropertyChanged
        {
            
            public Drone(int _id, string model, DalXml.DataObject.WeightCategories _maxWeight)
                : this()
            {
                Id = _id;
                _model = model;
                MaxWeight = _maxWeight;
                _exists = true;
            }

            public int Id { get; set; }

            private string _model; 
            public DalXml.DataObject.WeightCategories MaxWeight { get; set; }
            private bool _exists { get; set; }

           
            public override string ToString()
            {
                string res = "";
                res += "Drone " + Id + " " + Model + "\n" +
                   "MaxWeight: " + MaxWeight + "\n" ;
               
                return res;
            }
            public event PropertyChangedEventHandler PropertyChanged;
            public bool Exists
            {
                get { return _exists; }
                set
                {
                    _exists = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, 
                            new PropertyChangedEventArgs("Exists"));
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



        }

    }
    
}
