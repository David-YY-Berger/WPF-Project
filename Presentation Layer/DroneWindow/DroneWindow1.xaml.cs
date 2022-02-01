using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;


namespace Presentation_Layer
{
    /// <summary>
    /// Interaction logic for DroneWindow.xaml
    /// </summary>
    public partial class DroneWindow : Window
    {
        BL.BLApi.Ibl busiAccess;
        public int ThisDroneId; //field to allow window's function to retrieve bodrone from BL -- 
                                //this field is public because it is used by DroneListWindow
        //SIMULATOR FIELDS:
        private readonly BackgroundWorker workerForPLSimulator = new BackgroundWorker();
        bool simulatorOn = false;
        readonly int DELAY_BTW_STEPS = 500; //wait __ miliseconds between steps of 
       //FIELDS TO ALLOW OPENING MULTIPLE DRONEWINDOWS SIMOULTANEOUSLY
        bool openedByDroneList = false; 
        DroneListWindow parent; //the window which called this window
        
        //3 TYPES OF CONSTUCTORS:
        public DroneWindow(BL.BLApi.Ibl _busiAccess)//default CTOR - to Add a drone - OPENED FROM DRONElIST
        {
            InitializeComponent();
            busiAccess = _busiAccess;
            cmbWeightChoice.ItemsSource = Enum.GetValues(typeof(BL.BO.Enum.WeightCategories));

            //(1) Disable and Hide irrelevant buttons
            HelpfulFunctions.ChangeVisibilty(Visibility.Hidden, btnModifyDroneModel, btnAssignDroneToParcel, btnFreeDroneFromCharge,
                btnPickupPkg, btnSendToCharge, btnDeliverPkg, btnEraseDrone, btnSimulator);
            
            //(2) Hide irrelevnat TextBlocks
            tBlockStatus.Visibility = Visibility.Hidden;
            tBlockStatusInfo.Visibility = Visibility.Hidden;
            tBlockDelivery.Visibility = Visibility.Hidden;
            tBlockDeliveryInfo.Visibility = Visibility.Hidden;
            tBlockCurrentLocation.Visibility = Visibility.Hidden;
            tBlockCurrentLocationInfo.Visibility = Visibility.Hidden;
            tBlockLongitude.Visibility = Visibility.Hidden;
            tBlockLongInfo.Visibility = Visibility.Hidden;
            tBlockLatitude.Visibility = Visibility.Hidden;
            tBlockLatinfo.Visibility = Visibility.Hidden;
            tBlockBattery.Visibility = Visibility.Hidden;
            tBlockBatteryInfo.Visibility = Visibility.Hidden;

        }
        public DroneWindow(BL.BLApi.Ibl _busiAccess, 
            BL.BO.BODrone _bodrone) //CTOR called by ParcelWindow, StationWindow - to VIEW drone
        {
            InitializeComponent();
            busiAccess = _busiAccess;
            
            //updates DroneViewModel to display details
            ThisDroneId = _bodrone.Id; 
            displayBODrone(ThisDroneId);
             
            //edit buttons and text boxes for Update Window:
            tBoxIdInput.IsReadOnly = true;
            tBoxIdInput.BorderBrush = Brushes.Transparent;
            tBoxStationIdInput.IsReadOnly = true;
            tBoxStationIdInput.BorderBrush = Brushes.Transparent;
            cmbWeightChoice.ItemsSource = Enum.GetValues(typeof(BL.BO.Enum.WeightCategories));
            cmbWeightChoice.IsReadOnly = true;
            cmbWeightChoice.IsEnabled = false;

            HelpfulFunctions.ChangeVisibilty(Visibility.Hidden, 
                btnModifyDroneModel, btnAssignDroneToParcel, btnFreeDroneFromCharge, btnAddDrone,
               btnPickupPkg, btnSendToCharge, btnDeliverPkg, btnEraseDrone, btnSimulator);
        }

        public DroneWindow(BL.BLApi.Ibl _busiAccess, BL.BO.BODrone _bodrone,  
            DroneListWindow _parent) //CTOR called by DroneListWindow - to update a drone
             : this(_busiAccess, _bodrone) //calls constructor for viewing..
        {
            parent = _parent;
            openedByDroneList = true; //boolean value - used to keep window open, to prevent user 
                                     // from opening many windows of same drone... 
                                     //see comment at line __ of DroneListWindow.xaml.cs
            //ALLOW UPDATES:
            HelpfulFunctions.ChangeVisibilty(Visibility.Visible,
               btnModifyDroneModel, btnAssignDroneToParcel, btnFreeDroneFromCharge,
              btnPickupPkg, btnSendToCharge, btnDeliverPkg, btnEraseDrone, btnSimulator);
            //INITIALIZE BACKGROUNDWORKER FOR SIMULATOR
            workerForPLSimulator.DoWork += worker_DoWork;
            workerForPLSimulator.RunWorkerCompleted += worker_RunWorkerCompleted;
            workerForPLSimulator.ProgressChanged += worker_ProgressChanged;
            workerForPLSimulator.WorkerReportsProgress = true;
            workerForPLSimulator.WorkerSupportsCancellation = true;
        }
        
        //BUTTONS:
        private void btnAddDrone_Click(object sender, RoutedEventArgs e)
        {
            //reset text color
            HelpfulFunctions.ChangeTextColor(Colors.Black, tBlock_chooseDroneId, tBlock_chooseMaxWeight,
                tBlock_chooseModel, tBlock_chooseStation);
            
            //(1) Receive Data
            int _id;
            bool idSuccess = Int32.TryParse(tBoxIdInput.Text, out _id);
            string _model = tBoxModelInput.Text;
            int _stationId;
            bool stationIdSuccess = Int32.TryParse(tBoxStationIdInput.Text, out _stationId);
            DalXml.DataObject.WeightCategories? weight = (DalXml.DataObject.WeightCategories)cmbWeightChoice.SelectedIndex;

            //(2) Check that Data is Valid
            bool validData = true;
            //check Id
            if (tBoxIdInput.Text == null || !idSuccess || _id <= 0)
            {
                tBlock_chooseDroneId.Foreground = new SolidColorBrush(Colors.Red);
                validData = false;
            }

            if(tBoxModelInput.Text == null || tBoxModelInput.Text == "")
            {
                tBlock_chooseModel.Foreground = new SolidColorBrush(Colors.Red);
                validData = false;
            }

            if (tBoxStationIdInput.Text == null || !stationIdSuccess || _stationId <= 0)
            {
                tBlock_chooseStation.Foreground = new SolidColorBrush(Colors.Red);
                validData = false;
            }
            if(weight == null || (int)weight == -1)  //check weight categories
            {
                tBlock_chooseMaxWeight.Foreground = new SolidColorBrush(Colors.Red);
                validData = false;
            }

            //(3) Add Drone..
            if (validData)
            {
                try
                {
                    busiAccess.AddDrone(_id, _model, (DalXml.DataObject.WeightCategories)weight, _stationId);
                    MessageBox.Show("Drone Added Successfully", "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                    Close();
                }
                catch (BL.BLApi.EXDroneAlreadyExists exception)
                {
                    //if Drone's Id already exists
                    MessageBox.Show(exception.printException(), "Error Message", 
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
                catch (BL.BLApi.EXNotFoundPrintException ex)
                {
                    //if Station not found.. (must Add Drone at existing Station...)
                    MessageBox.Show(ex.ToString(), "Error Message", 
                        MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
            }
            else
                return;          
        }

        private void btnModifyDroneModel_Click(object sender, RoutedEventArgs e)
        {
            int id;
            Int32.TryParse(tBoxIdInput.Text, out id);
            busiAccess.ModifyDrone(id, tBoxModelInput.Text);
            MessageBox.Show("Drone Model Changed", "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            Close();
        }

        private void btnSendToCharge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                busiAccess.ChargeDrone(ThisDroneId);
            }
            catch (BL.BLApi.EXDroneUnavailableException ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }
            displayBODrone(ThisDroneId);
        }

        private void btnFreeDroneFromCharge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                busiAccess.FreeDrone(ThisDroneId, DateTime.Now);
            }
            catch (BL.BLApi.EXMiscException ex) //if drone is not charging
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }
             displayBODrone(ThisDroneId);
        }

        private void btnPickupPkg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                busiAccess.PickupParcel(ThisDroneId);
            }
            catch (BL.BLApi.EXDroneNotAssignedParcel ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }
            catch (BL.BLApi.EXParcelAlreadyCollected ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }
            displayBODrone(ThisDroneId);
        }
        private void btnAssignDroneToParcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                busiAccess.AssignParcel(ThisDroneId);
            }
            catch (BL.BLApi.EXNoAppropriateParcel ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }
            catch (BL.BLApi.EXDroneUnavailableException ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }
            displayBODrone(ThisDroneId);
        }

        private void btnDeliverPkg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                busiAccess.DeliverParcel(ThisDroneId);
            }
            catch (BL.BLApi.EXDroneNotAssignedParcel ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }
            catch (BL.BLApi.EXParcelNotCollected ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }
            displayBODrone(ThisDroneId);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnEraseDrone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //no need to check if simulator is on. bec if it was, the btn would be disabled...
                busiAccess.EraseDrone(ThisDroneId);
                MessageBox.Show("Drone " + tBoxIdInput.Text + " Erased", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                Close();
            }
            catch (BL.BLApi.EXCantDltDroneWParc ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
                return;
            }
        }
        
        //SIMULATOR FUNCTIONS
        private void btnSimulator_Click(object sender, RoutedEventArgs e)
        {
            if (simulatorOn == false)
            {
                //TURN ON SIMULATOR
                BeginSimulator();
            }
            else // if(simulatorOn == true)
            {
                StopSimulator();
            }
        }

        public void BeginSimulator()//public function --> can be called by DroneListWindow
        {
            if (simulatorOn) //if similulator is already on..
                return;
            simulatorOn = true;
            Thread newSimulatorThread = new Thread(beginSimulatorWrapperFunc);
            newSimulatorThread.Start();
            btnSimulator.Content = "End Simulator";
            HelpfulFunctions.ChangeVisibilty(Visibility.Hidden, btnFreeDroneFromCharge,
                btnSendToCharge, btnAssignDroneToParcel, btnPickupPkg,
                btnDeliverPkg, btnEraseDrone);
        }
        public void StopSimulator() //public function --> can be called by DroneListWindow
        {
            if (!simulatorOn) //if similulator is already off..
                return; 
            workerForPLSimulator.CancelAsync();
            simulatorOn = false;
            busiAccess.StopSimulatorForDrone(ThisDroneId);
            btnSimulator.Content = "Begin Simulator";
            HelpfulFunctions.ChangeVisibilty(Visibility.Visible, btnFreeDroneFromCharge,
                btnSendToCharge, btnAssignDroneToParcel, btnPickupPkg,
                btnDeliverPkg, btnEraseDrone);
            HelpfulFunctions.ChangeTextColor(Colors.Black, tBlockBatteryInfo); //if charging, this box was green
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
                busiAccess.BeginSimulatorForDrone(ThisDroneId);
                while (simulatorOn == true)
                {
                    Thread.Sleep(DELAY_BTW_STEPS);
                    workerForPLSimulator.ReportProgress(1); //this 1 is insignificant..
                }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Dispatcher.Invoke(() => //Invoke function ensures that
                                         //only one thread access this code block
            {
                displayBODrone(ThisDroneId);
            });
        }

        private void worker_RunWorkerCompleted(object sender,   RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Simulator ended successfully", "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            //workerForPLSimulator.Dispose();
        }
        private void beginSimulatorWrapperFunc()
        {
             simulatorOn = true;
             workerForPLSimulator.RunWorkerAsync();     //<- begin backgroundWorker...
        }
        
        //OTHER FUNCTIONS
        private Presentation_Layer.DroneStringViewModel createDroneViewModel(BL.BO.BODrone origDrone)
        {
            Presentation_Layer.DroneStringViewModel newDrone = new Presentation_Layer.DroneStringViewModel();
            newDrone.Battery = Math.Round(origDrone.Battery, 2).ToString();
            newDrone.DroneStatus = origDrone.DroneStatus.ToString();
            newDrone.Exists = origDrone.Exists;
            newDrone.Id = origDrone.Id.ToString();
            newDrone.Longitude = Math.Round(origDrone.Location.Longitude, 5).ToString();
            newDrone.Latitude = Math.Round(origDrone.Location.Latitude, 5).ToString();
            newDrone.MaxWeight = origDrone.MaxWeight.ToString();
            newDrone.Model = origDrone.Model;
            newDrone.ParcelInTransfer = (origDrone.ParcelInTransfer.Id == -1 || origDrone.ParcelInTransfer == null) ?
                "Not yet carrying Parcel" : origDrone.ParcelInTransfer.ToString();
            newDrone.LocationString = busiAccess.GetDroneLocationString(origDrone.Id);
            int stationId = busiAccess.GetStationIdOfBODrone(origDrone.Id);
            newDrone.StationId = (stationId != -1) ? stationId.ToString() : "Drone is not charging at a Station";
            return newDrone;
        }

        private void displayBODrone(int _droneId) //updates this drone model
        //this function is called after any changes are made
        {
            Dispatcher.Invoke(() =>
            {
                BL.BO.BODrone bodrone = busiAccess.GetBODrone(_droneId);
                DataContext = createDroneViewModel(bodrone); //set this drone for this window...
                if (simulatorOn)
                {
                    if (bodrone.DroneStatus == BL.BO.Enum.DroneStatus.Charging)
                        HelpfulFunctions.ChangeTextColor(Colors.Green, tBlockBatteryInfo);
                    else
                        HelpfulFunctions.ChangeTextColor(Colors.Black, tBlockBatteryInfo);
                }
                if (parent != null)
                    parent.RefreshList();
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (openedByDroneList)
            {
                if(simulatorOn) // keep window open, but hidden..
                {
                    e.Cancel = true;
                    Hide(); //hides window instead of closing....- see comment at line 27 of DroneListWindow
                }
                else //close this window...
                {
                    parent.RemoveDroneWindow(ThisDroneId);
                }
                parent.RefreshList(); //Refreshes list in DroneListWindow
            }
        }
        //END OF DRONE WINDOW CODE.
    }
}
