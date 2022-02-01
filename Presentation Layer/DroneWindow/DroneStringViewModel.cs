using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

namespace Presentation_Layer
{
    /// <summary>
    /// this class holds the STRING FORM of each field of BODrone.... 
    /// It cannot be used for calculations, only for presentation
    /// </summary>
    class DroneStringViewModel : INotifyPropertyChanged
    {
        public string Id { get; set; }
        

        private string _model;
        public string MaxWeight { get; set; }
        private string _battery; 
        public string DroneStatus { get; set; }
        public string ParcelInTransfer { get; set; }
        public string StationId { get; set; }
       
        public string LocationString { get; set; }       //string which describes drone's location
                                                         //in relation to nearby Stations or Customers
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        
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
        public string Battery
        {
            get { return _battery;  }
            set
            {
                _battery = value;
                if( PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Presentation_Layer.DroneStringViewModel.Battery)));
                }
            }
        }




















        //end of class
    }
}
