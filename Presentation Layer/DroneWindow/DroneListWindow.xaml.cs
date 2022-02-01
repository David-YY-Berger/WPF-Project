using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Presentation_Layer
{
    /// <summary>
    /// Interaction logic for DroneList.xaml
    /// </summary>
    public partial class DroneListWindow : Window
    {
        BL.BLApi.Ibl busiAccess;
        public ICollectionView droneCollectionView { get; set; }
        /// <summaryOflistDroneWindows>
        /// -Holds the open windows of each drone, to ensure that user does not open 2 windows of the same drone.
        /// -If a drone's Simulator is on, the DroneWindow will not close, but rather will be hidden
        /// -If the DroneListWindow is closed, all simulators are stopped, and the all DroneWindows are closed
        /// </summary>
        List<DroneWindow> listOpenDroneWindows = new List<DroneWindow>();
        bool allDroneSimulatorsOn = false;
        
        /// <summaryOfWatchSimulator>
        ///  Because we did not succeed with proper databinding, we have creating a simulator
        ///  to refresh the DroneListWindow. This Simulator is not connecting to the individual drone's simulators
        /// </summary>
        bool watchSimulatorOn = false;
        readonly BackgroundWorker workerToDisplayDroneList = new BackgroundWorker();
        readonly int DELAY_BTW_STEPS = 2000;  // ___ miliseconds
        //"parent" - allows us to ensure no other windows open simoultaneously with DronelistWindow, 
        //except for MapWindow (to watch Simulator)
        MainWindow parent;



        //CTOR:
        public DroneListWindow(BL.BLApi.Ibl _busiAccess, MainWindow _parent)
        {
            busiAccess = _busiAccess;
            parent = _parent;
            InitializeComponent();
            //listDrone = getDronesAsObservable();
            RefreshList();
            StatusSelector1.ItemsSource = Enum.GetValues(typeof(BL.BO.Enum.DroneStatus));
            StatusSelector2.ItemsSource = Enum.GetValues(typeof(BL.BO.Enum.WeightCategories));

            btnWatchSimulator.IsEnabled = false;

            //INITIALIZE BACKGROUNDWORKER FOR SIMULATOR
            workerToDisplayDroneList.DoWork += workerToDisplayDroneList_DoWork;
            //workerToDisplayDroneList.RunWorkerCompleted += workerToDisplayDroneList_RunWorkerCompleted;
            //workerToDisplayDroneList.ProgressChanged += workerToDisplayDroneList_ProgressChanged;
            //workerToDisplayDroneList.WorkerReportsProgress = true;
            workerToDisplayDroneList.WorkerSupportsCancellation = true;
        }
        //BUTTONS, USER INTERFAFCE:
        private void StatusSelector1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = StatusSelector1.SelectedIndex;
            DronesListView.ItemsSource = busiAccess.GetSpecificDroneListStatus(index);
        }
        private void StatusSelector2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = StatusSelector2.SelectedIndex;
            DronesListView.ItemsSource = busiAccess.GetSpecificDroneListWeight(index);
        }
        private void btnAddDrone1_Click(object sender, RoutedEventArgs e)
        {
            new DroneWindow(busiAccess).ShowDialog();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void DronesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BL.BO.BODrone drone = DronesListView.SelectedItem as BL.BO.BODrone;
            //check if drone is erased:
            if (drone.Exists)
            {
                openDroneWindow(drone);
            }
            else
            {
                HelpfulFunctions.ErrorMsg("Drone is deleted");
            }
        }
        private void chkBoxGetErased_Checked(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }
        private void chkBoxGetErased_UnChecked(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }
        private void btnSimulateAll_Click(object sender, RoutedEventArgs e)
        {
            if(!allDroneSimulatorsOn)
            {
                simulateAllDrones();
                btnSimulateAll.Content = "Stop all Simulation";
                allDroneSimulatorsOn = true;
            }
            else //if all drones are already being simulated
            {
                CloseAndStopSimulatorAllDroneWindows();
                btnSimulateAll.Content = "Simulate All Drones";
                allDroneSimulatorsOn = false;
            }
        }
        
        //HELPING FUNCTIONS
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(parent.IsClosing) //if User is closing the MainWindow, we prevent this MsgBox from popping up...
            {
                CloseAndStopSimulatorAllDroneWindows();
                return; 
            }
            
            if(allDroneSimulatorsOn)
            {
                MessageBoxResult result =
             MessageBox.Show("Closing this window will close all drone windows, and stop all simulators. " +
              "Do you want to close this window?",
              "Notice", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    // If user doesn't want to close, cancel closure
                    e.Cancel = true;
                }
                else //close the windows...
                {
                    CloseAndStopSimulatorAllDroneWindows();
                }
                //DroneListWindow closes automatically, no need to clear list..
            }

        }
        private void CloseAndStopSimulatorAllDroneWindows()
        {
            for (int i = 0; i < listOpenDroneWindows.Count; i++) //closes any open DroneWindows
            {
                //if the simulator is not on, this function won't do anything...
                listOpenDroneWindows[i].StopSimulator();
                listOpenDroneWindows[i].Close();
                i--; // so that the iteration works properly...
            }
        }
        public void RemoveDroneWindow(int _droneId) //called by DroneWindow - to remove itself
        {
            for (int i = 0; i < listOpenDroneWindows.Count; i++)
            {
                if (listOpenDroneWindows[i].ThisDroneId == _droneId) 
                    //bec we are changing the collection, 
                    //we cannot use a "foreach" loop
                {
                    listOpenDroneWindows.Remove(listOpenDroneWindows[i]);
                    return;
                }
            }

        }
        public void RefreshList() //called to refresh...
        {
            Dispatcher.Invoke(() => //Invoke function ensures that
                                    //only one thread access this code block
            {
                droneCollectionView = CollectionViewSource.
                GetDefaultView(busiAccess.GetBODroneList(true));
                DataContext = droneCollectionView;
                //DronesListView.ItemsSource = droneCollectionView;
                if ((bool)!chkBoxGetErased.IsChecked)
                    droneCollectionView.Filter = filterOutErased;
                else
                    droneCollectionView.Filter = null;

                droneCollectionView.SortDescriptions.Add(new SortDescription
                    (nameof(BL.BO.BOParcelToList.Id), ListSortDirection.Ascending));
            });
        }
        private void openDroneWindow(BL.BO.BODrone drone, bool showDroneWindow = true)
        {
            //CHECK IF WINDOW ALREADY OPEN
            foreach (var item in listOpenDroneWindows)
            {
                if (item.ThisDroneId == drone.Id)   
                {
                    if(showDroneWindow) //received as parameter - if caller wants to open, and show the window..
                    {
                        item.Show();
                        item.Focus(); //Window might not have been closed, or simulator is still on..
                        if (item.WindowState == WindowState.Minimized)
                            item.WindowState = WindowState.Normal;
                    }
                    return;
                }
            }
            //IF THE WINDOW IS NOT OPEN
            DroneWindow newDroneWindow = new DroneWindow(busiAccess, drone, this);
            listOpenDroneWindows.Add(newDroneWindow);
            if(showDroneWindow)
                newDroneWindow.Show();
        }
        private void simulateAllDrones()
        {
            foreach (var item in busiAccess.GetBODroneList(true))
            {
                //create drone windows
                if (item.Exists)
                {
                    openDroneWindow(item, false);
                }
            }
            foreach (var item in listOpenDroneWindows)
            {
                //hide window, and begin simulator
                item.Hide();
                item.BeginSimulator(); //if simulator is already on, nothing happens
            }
        }
        private bool filterOutErased(object obj)
        {
            if (obj is BL.BO.BODrone item)
            {
                return item.Exists;
            }
            else return false;
        }

        //"WATCH"_SIMULATOR FUNCTIONS -- TO REFRESH DRONELIST EVERY FEW SECONDS..
        private void btnWatchSimulator_Click(object sender, RoutedEventArgs e)
        {
            if (!watchSimulatorOn)
            {
                new Thread(beginWatchingSimulator).Start();
                btnWatchSimulator.Content = "Stop Watching Drones";
                watchSimulatorOn = true;
            }
            else //if simulatorOn
            {
                watchSimulatorOn = false;
                workerToDisplayDroneList.CancelAsync();
                btnWatchSimulator.Content = "Watch Drone Simulator";
                workerToDisplayDroneList.Dispose();
            }
        }
        private void workerToDisplayDroneList_DoWork(object sender, DoWorkEventArgs e)
        //THREAD SLEEPS HERE
        {
            while(watchSimulatorOn)
            {
               Thread.Sleep(DELAY_BTW_STEPS);
               RefreshList();
            }
        }
        private void beginWatchingSimulator()
        {
            watchSimulatorOn = true;
            workerToDisplayDroneList.RunWorkerAsync();
        }

        //end of window
    }
}
