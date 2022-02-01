using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Presentation_Layer
{
    /// <summary>
    /// Interaction logic for StationWindow.xaml
    /// </summary>
    public partial class StationWindow : Window
    {
        BL.BLApi.Ibl busiAccess;
        public StationWindow(BL.BLApi.Ibl _busiAccess) //CTOR - Add Station
        {
            InitializeComponent();
            busiAccess = _busiAccess;

            lstViewDronesCharging.IsEnabled = false;
            lstViewDronesCharging.Visibility = Visibility.Hidden;
            btnEraseStation.IsEnabled = false;
            btnEraseStation.Visibility = Visibility.Hidden;
            tBlockLatitude.Text += busiAccess.GetLongitudeBegin().ToString() + " - " +
                busiAccess.GetLongitudeEnd().ToString();
            tBlockLatitude.Text += busiAccess.GetLatitudeBegin().ToString() + " - " +
                busiAccess.GetLatitudeEnd().ToString();
            HelpfulFunctions.ChangeVisibilty(Visibility.Hidden, btnModifyName, btnModifyChargeSlots);
            HelpfulFunctions.ChangeVisibilty(Visibility.Hidden, tBlockDronesCharging);
            

        }
        public StationWindow(BL.BLApi.Ibl _busiAccess, int stationId) // CTOR - Update Station
        {
            InitializeComponent();
            busiAccess = _busiAccess;
            displayStation(stationId);
            btnAddStation.IsEnabled = false;
            btnAddStation.Visibility = Visibility.Hidden;

        }
        private void btnAddStation_Click(object sender, RoutedEventArgs e)
        {
            //reset text color
            HelpfulFunctions.ChangeTextColor(Colors.Black, tBlockId, tBlockChargeSlots, 
                tBlockLong, tBlockLatitude, tBlockName);

            //(1) Receive Data
            int _id;
            bool idSuccess = Int32.TryParse(tBoxIdInput.Text, out _id);

            int _name;
            bool nameSuccess = Int32.TryParse(tBoxNameInput.Text, out _name);
            //NAME TZARICH IYUN!


            int numChargeSlots;
            bool chargeSlotsSuccess = Int32.TryParse(tBoxChargeSlotsInput.Text, out numChargeSlots);

            double _longitude;
            double _latitude;
            bool longSuccess = double.TryParse(tBoxLongInput.Text, out _longitude);
            bool latSuccess = double.TryParse(tBoxLatInput.Text, out _latitude);


            //(2) Check that Data is Valid
            bool validData = true;
            //check Id
            if (tBoxIdInput.Text == null || !idSuccess || _id <= 0)
            {
                tBlockId.Foreground = new SolidColorBrush(Colors.Red);
                validData = false;
            }

            //check Id
            if (tBoxNameInput.Text == null || !nameSuccess)
            {
                tBlockName.Foreground = new SolidColorBrush(Colors.Red);
                validData = false;
            }
            //check charge slots
            if (tBoxChargeSlotsInput.Text == null || !chargeSlotsSuccess || numChargeSlots <= 0)
            {
                tBlockChargeSlots.Foreground = new SolidColorBrush(Colors.Red);
                validData = false;
            }
            //check longitude
            if (tBoxLongInput.Text == null || !longSuccess || _longitude < busiAccess.GetLongitudeBegin() 
                || _longitude > busiAccess.GetLongitudeEnd())
            {
                tBlockLong.Foreground = new SolidColorBrush(Colors.Red);
                validData = false;
            }
            //check latitude
            if (tBoxLatInput.Text == null || !latSuccess || _latitude< busiAccess.GetLongitudeBegin()
                || _latitude > busiAccess.GetLongitudeEnd())
            {
                tBlockLatitude.Foreground = new SolidColorBrush(Colors.Red);
                validData = false;
            }

            //(3) Add Station...
            if (validData)
            {
                try
                {
                    busiAccess.AddStation(_id, _name, _longitude, _latitude, numChargeSlots);
                    MessageBox.Show("Station Added Successfully", "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                    Close();
                }
                catch (BL.BLApi.EXStationAlreadyExists exception)
                {
                    HelpfulFunctions.ErrorMsg(exception.ToString());
                }
               
            }
            else
                return;

        }
        private void displayStation(int _stationId)
        {
            BL.BO.BOStation st = busiAccess.GetBOStation(_stationId);

            tBoxIdInput.Text = st.Id.ToString();
            tBoxNameInput.Text = st.Name.ToString();
            tBoxChargeSlotsInput.Text = st.ChargeSlots.ToString();
            tBoxLongInput.Text = st.Location.Longitude.ToString();
            tBoxLatInput.Text = st.Location.Latitude.ToString();

            tBoxIdInput.IsEnabled = false;
            //tBoxNameInput.IsEnabled = false;
            //tBoxChargeSlotsInput.IsEnabled = false;
            tBoxLongInput.IsEnabled = false;
            tBoxLatInput.IsEnabled = false;
            lstViewDronesCharging.ItemsSource = st.ListDroneCharge;
        }

        private void btnModifyName_Click(object sender, RoutedEventArgs e)
        {
            int _name;
            bool nameSuccess = Int32.TryParse(tBoxNameInput.Text, out _name);
            //NAME TZARICH IYUN!
            //check Id
            if (tBoxNameInput.Text == null || !nameSuccess)
            {
                tBlockName.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            busiAccess.ModifyStation(Int32.Parse(tBoxIdInput.Text), _name,
               busiAccess.GetBOStation(Int32.Parse(tBoxIdInput.Text)).ChargeSlots);
            MessageBox.Show("Name Modified", "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            displayStation(Int32.Parse(tBoxIdInput.Text));
        }

        private void btnModifyChargeSlots_Click(object sender, RoutedEventArgs e)
        {
            int numChargeSlots;
            bool chargeSlotsSuccess = Int32.TryParse(tBoxChargeSlotsInput.Text, out numChargeSlots);

            if (tBoxChargeSlotsInput.Text == null || !chargeSlotsSuccess || numChargeSlots <= 0)
            {
                tBlockChargeSlots.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            busiAccess.ModifyStation(Int32.Parse(tBoxIdInput.Text), 
                busiAccess.GetBOStation(Int32.Parse(tBoxIdInput.Text)).Name, 
                numChargeSlots);
            MessageBox.Show("Number of Charge Slots Modified", "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            displayStation(Int32.Parse(tBoxIdInput.Text));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void lstViewDronesCharging_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BL.BO.BODrone bodrone;
            try
            {
                bodrone = busiAccess.
                GetBODrone((lstViewDronesCharging.SelectedItem as BL.BO.BODroneInCharge).Id);
            }
            catch (BL.BLApi.EXDroneNotFound)
            {
                return;
            }
            if (bodrone.Exists)
                new DroneWindow(busiAccess, bodrone).ShowDialog();
            else
                HelpfulFunctions.ErrorMsg("Drone does not exist");
            displayStation(Int32.Parse(tBoxIdInput.Text));
        }

        private void btnEraseStation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                busiAccess.EraseStation(Int32.Parse(tBoxIdInput.Text));
                HelpfulFunctions.SuccessMsg("Station Erased");
                Close();
            }
            catch (BL.BLApi.EXCantDltStationWDroneCharging ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }
        }













        //END OF WINDOW
    }
}
