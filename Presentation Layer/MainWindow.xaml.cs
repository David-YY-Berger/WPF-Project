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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Presentation_Layer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {

        BL.BLApi.Ibl busiAccess;          // <-- allows us to access business logic layer..
        MapWindow ptrMapWindow;
        DroneListWindow ptrDroneListWindow;  // <-- used to prevent user from opening other windows while
                                          // the droneList window is open
        public bool IsClosing = false;         //<-- used to communicate to DroneListwWindow that this window is closing...

        public MainWindow(BL.BLApi.Ibl _busiAccess) //CTOR
        {
            busiAccess = _busiAccess;
            InitializeComponent();
        }
        //BUTTONS:
        private void btnOpenDroneList_Click(object sender, RoutedEventArgs e)
        {
            
            if (HelpfulFunctions.IsWindowOpen(ptrDroneListWindow)) //if droneListWindow is already open...
            {
                ptrDroneListWindow.Show();
                ptrDroneListWindow.Focus();
                if (ptrDroneListWindow.WindowState == WindowState.Minimized)
                    ptrDroneListWindow.WindowState = WindowState.Normal;
            }
            else
            {
                ptrDroneListWindow = new DroneListWindow(busiAccess, this);
                ptrDroneListWindow.Show();
            }
        }
        private void btnCustomerLists_Click(object sender, RoutedEventArgs e)
        {
            if (HelpfulFunctions.IsWindowOpen(ptrDroneListWindow))
                HelpfulFunctions.ErrorMsg("Cannot open this window while the drone list winow is open");
            else if (HelpfulFunctions.IsWindowOpen(ptrMapWindow))
                HelpfulFunctions.ErrorMsg("Cannot open this window while the map window is open");
            else
                new CustomerListWindow(busiAccess).ShowDialog();
        }
        private void btnParcelLists_Click(object sender, RoutedEventArgs e)
        {
            if (HelpfulFunctions.IsWindowOpen(ptrDroneListWindow))
                HelpfulFunctions.ErrorMsg("Cannot open this window while the drone list window is open");
            else if (HelpfulFunctions.IsWindowOpen(ptrMapWindow))
                HelpfulFunctions.ErrorMsg("Cannot open this window while the map window is open");
            else
                new ParcelListWindow(busiAccess).ShowDialog();
        }
        private void btnStationLists_Click(object sender, RoutedEventArgs e)
        {
            if (HelpfulFunctions.IsWindowOpen(ptrDroneListWindow))
                HelpfulFunctions.ErrorMsg("Cannot open this window while the drone list window is open");
            else if (HelpfulFunctions.IsWindowOpen(ptrMapWindow))
                HelpfulFunctions.ErrorMsg("Cannot open this window while the map window is open");
            else
                new StationListWindow(busiAccess).ShowDialog();
        }
        private void btnOpenMap_Click(object sender, RoutedEventArgs e)
        {
            if (HelpfulFunctions.IsWindowOpen(ptrMapWindow)) //if mapWindow is already open...
            {
                ptrMapWindow.Show();
                ptrMapWindow.Focus();
                if (ptrMapWindow.WindowState == WindowState.Minimized)
                    ptrMapWindow.WindowState = WindowState.Normal;
            }
            else
            {
                ptrMapWindow = new MapWindow(busiAccess);
                ptrMapWindow.Show();
            }
        }
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow(busiAccess).Show();
            Close();
        }
        //HELPING FUNCTIONS:
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //ensure that window is not left open..
            if (ptrDroneListWindow != null)
            {
                IsClosing = true;
                ptrDroneListWindow.Close();
                ptrDroneListWindow = null;
            } 
            if(HelpfulFunctions.IsWindowOpen(ptrMapWindow))
            {
                ptrMapWindow.Close();
                ptrMapWindow = null;
            }    
        }
        //END OF WINDOW
    }
}
