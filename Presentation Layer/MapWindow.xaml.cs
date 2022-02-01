using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;
using System.ComponentModel;


namespace Presentation_Layer
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        BL.BLApi.Ibl busiAccess;
        enum ObjectType { Drone, Station, Customer}
        //file paths for images:
        readonly String ImgDroneWithParcel = AppDomain.CurrentDomain.BaseDirectory.Split("Presentation Layer")[0] + "Presentation Layer\\Pictures\\drone_with_parcel.jpg";
        readonly String ImgDroneWithoutParcel = AppDomain.CurrentDomain.BaseDirectory.Split("Presentation Layer")[0] +"Presentation Layer\\Pictures\\drone_without_parcel2.PNG";
        readonly String ImgStation = AppDomain.CurrentDomain.BaseDirectory.Split("Presentation Layer")[0] + "Presentation Layer\\Pictures\\station.jpg";
        readonly String ImgHouse = AppDomain.CurrentDomain.BaseDirectory.Split("Presentation Layer")[0] + "Presentation Layer\\Pictures\\house.png";

        List<TextBlock> listTextBlocks = new List<TextBlock>();
        List<Image> listImages = new List<Image>();

        readonly Color customerColor = Colors.Blue;
        readonly Color stationColor = Colors.Green;
        readonly Color droneColor = Colors.Red;
        readonly Color textColor = Colors.Black;
        readonly int IMAGESIZEforDrones = 1; //gridspan and rowspan of image
        readonly int IMAGESIZEforCustAndStations = 2; //gridspan and rowspan of image
        readonly string emptyTextForInfoWindow = "Hover the mouse over a square or image";
        //for background map - created with drawMap()
        public static double LongitudeBegin; 
        public static double LatitudeBegin;
        public const int MARGIN_FOR_MAP = 1; //for top and left margin of map
        public readonly Color MapColor = Colors.LightGray;
        public const int MAPGRIDSPAN = 20; // Map only works if square -- functionality must be added for different shapes, or sizes
  
        //for simulator:
        readonly BackgroundWorker worker = new BackgroundWorker();
        bool simulatorOn = false;
        readonly string textForSimulatorBtnStart = "Turn On Simulator";
        readonly int DELAY_BTW_REFRESH = 500; // __ miliseconds
        //InfoBlock Class:
        /// <summaryOfInfoBlock>
        /// Each block is synchronized with an id number, tagged with a type of object,
        /// and given appropriate Column and Row places
        /// </summary>
        class InfoBlock
        {
            /// <summaryOfNumGridSpots>
            /// number of squares available on map,  only set once...
            /// MUST BE A MULTIPLE OF 10...grid must be square..
            /// </summary>
            public readonly int numGridSpots = 20; // per line
           //CTORS: (3 in total)
            public InfoBlock(BL.BO.BOCustomer cust, int _numParcelsAtCustomer)
            {
                Id = cust.Id;
                ThisObjectType = ObjectType.Customer;
                ColumnPlace = getColumnPlace(cust.Location);
                RowPlace = getRowPlace(cust.Location);
                numParcelsOrDronesCharging = _numParcelsAtCustomer;
                name = cust.Name;
            }
            public InfoBlock(BL.BO.BOStation st, int _numDronesCharging)
            {
                Id = st.Id;
                ThisObjectType = ObjectType.Station;
                ColumnPlace = getColumnPlace(st.Location);
                RowPlace = getRowPlace(st.Location);
                numParcelsOrDronesCharging = _numDronesCharging;
                name = "Station: " + st.Id.ToString();
            }
            public InfoBlock(BL.BO.BODrone drone)
            {
                Id = drone.Id;
                ThisObjectType = ObjectType.Drone;
                ColumnPlace = getColumnPlace(drone.Location);
                RowPlace = getRowPlace(drone.Location);
                numParcelsOrDronesCharging = (drone.ParcelInTransfer.Id == 0 || drone.ParcelInTransfer.Id == -1
                    || drone.ParcelInTransfer.Collected == false) ? //if drone has not yet picked up parcel...
                    /*set to Zero*/ 0 : /*else set to Parcel's Id*/ drone.ParcelInTransfer.Id;
                name = "Drone " + drone.Id.ToString();
            }
           //FIELDS:
            public int Id { get; set; }
            public ObjectType ThisObjectType { get; set; }
            public int RowPlace { get; set; }
            public int ColumnPlace { get; set; }
            public int? numParcelsOrDronesCharging { get; set; } //used differently for drone, customer, and station...
            public string name { get; set; }
            //METHODS:
            private int getColumnPlace(BL.BO.BOLocation loc)
            {
                return (int)((Math.Round(loc.Longitude, numGridSpots/10) - MapWindow.LongitudeBegin) * 10) + MARGIN_FOR_MAP;
            }
            private int getRowPlace(BL.BO.BOLocation loc)
            {
                return (int)((Math.Round(loc.Latitude, numGridSpots / 10) - MapWindow.LatitudeBegin) * 10) + MARGIN_FOR_MAP;
            }
        }

        //CTOR of MapWindow: 
        public MapWindow(BL.BLApi.Ibl _busiAccess) 
        {
            InitializeComponent();
            busiAccess = _busiAccess;
            LongitudeBegin = busiAccess.GetLongitudeBegin();
            LatitudeBegin = busiAccess.GetLatitudeBegin();
            double longitudeRange = busiAccess.GetLongitudeEnd() - LongitudeBegin;
            double latitudeRange = busiAccess.GetLatitudeEnd() - LatitudeBegin;
            if ( longitudeRange != 2
                || latitudeRange != 2)
            {
                HelpfulFunctions.ErrorMsg("Map only works if the Range for Longitude and Latitude are exactly 2");
                Close();
            }
            tBoxInfoWindow.Text = emptyTextForInfoWindow;
            drawMapBackground();
            refreshMap();
            WindowState = WindowState.Maximized;
            chkboxTextMode.IsChecked = true;        //default - with text boxes...
            //for simulator: 
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerSupportsCancellation = true;
        }
        
        //BUTTONS AND OTHER USER INTERFACE:
        private void btnReturnToMainMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            refreshMap();
        }
        private void chkboxTextMode_Checked(object sender, RoutedEventArgs e)
        {
            refreshMap();
        }
        private void chkboxTextMode_Unchecked(object sender, RoutedEventArgs e)
        {
            refreshMap();
        }
        
        //HELPING FUNCTIONS
        private void drawMapBackground()
        {
            TextBlock mapBackgroundTBlock = new TextBlock();
            Grid.SetColumn(mapBackgroundTBlock, MARGIN_FOR_MAP);
            Grid.SetRow(mapBackgroundTBlock, MARGIN_FOR_MAP);
            mapBackgroundTBlock.Background = new SolidColorBrush(MapColor);
            Grid.SetColumnSpan(mapBackgroundTBlock, MAPGRIDSPAN + 1); //for image's which have bigger grid span
            Grid.SetRowSpan(mapBackgroundTBlock, MAPGRIDSPAN + 1);
            gridMap.Children.Add(mapBackgroundTBlock);
        }
        private void fillMapWithTextBlocks() //drones are behind stations, in front of customers
        {
            foreach (var item in busiAccess.GetAllBOCustomers())
            {
                if (item.Exists) 
                    listTextBlocks.Add(createTextBlock(new InfoBlock(item,
                    busiAccess.GetNumParcelsWaitingAtCustomer(item))));
            }
            foreach (var item in busiAccess.GetBODroneList())
            {
                if (item.Exists)
                    listTextBlocks.Add(createTextBlock(new InfoBlock(item)));
            }
            foreach (var item in busiAccess.GetStations())
            {
                if (item.Exists) 
                    listTextBlocks.Add(createTextBlock(new InfoBlock(item,
                    busiAccess.GetOneStationToList(item.Id).ChargeSlotsTaken)));
            }
        }
        private void fillMapWithImages() //drones are in front of other items
        {
            foreach (var item in busiAccess.GetAllBOCustomers())
            {
                if (item.Exists) 
                    listImages.Add(createImage(new InfoBlock(item,
                    busiAccess.GetNumParcelsWaitingAtCustomer(item))));
            }
            foreach (var item in busiAccess.GetStations())
            {
                if (item.Exists) 
                    listImages.Add(createImage(new InfoBlock(item,
                    busiAccess.GetOneStationToList(item.Id).ChargeSlotsTaken)));
            }
            foreach (var item in busiAccess.GetBODroneList())
            {
                if (item.Exists) 
                    listImages.Add(createImage(new InfoBlock(item)));
            }
        }
        private TextBlock createTextBlock(InfoBlock _infoBlock) //Creates and sets TextBlock on Grid
        {
            TextBlock newTextBlock = new TextBlock();
            newTextBlock.Name = "tBlockR" + _infoBlock.RowPlace.ToString() + "C" + _infoBlock.ColumnPlace.ToString();
            Grid.SetColumn(newTextBlock, _infoBlock.ColumnPlace);
            Grid.SetRow(newTextBlock, _infoBlock.RowPlace);
            newTextBlock.FontSize = 11;
            newTextBlock.FontWeight = FontWeights.Bold;
            newTextBlock.Foreground = new SolidColorBrush(textColor);
            newTextBlock.FontWeight = FontWeight.FromOpenTypeWeight(700);
            
            newTextBlock.MouseLeftButtonDown += new MouseButtonEventHandler(
                new EventHandler((sender, e) => openWindowOfInfoBlock(sender, e, _infoBlock)));
            newTextBlock.MouseLeave += new MouseEventHandler(this.hideTextInInfoWindow);
            switch (_infoBlock.ThisObjectType)
            {
                case ObjectType.Station:
                    {
                        newTextBlock.Background = new SolidColorBrush(stationColor);
                        newTextBlock.MouseEnter += new MouseEventHandler(
                            new EventHandler((sender, e) => displayStationInInfoWindow(sender, e, _infoBlock)));
                        newTextBlock.Text = _infoBlock.numParcelsOrDronesCharging.ToString();
                    }
                    break;
                case ObjectType.Customer:
                    {
                        newTextBlock.Background = new SolidColorBrush(customerColor);
                        newTextBlock.MouseEnter += new MouseEventHandler(
                            new EventHandler((sender, e) => displayCustomerInInfoWindow(sender, e, _infoBlock)));
                        newTextBlock.Text = _infoBlock.numParcelsOrDronesCharging.ToString();
                    }
                    break;
                case ObjectType.Drone:
                    {
                        newTextBlock.Background = new SolidColorBrush(droneColor);
                        newTextBlock.MouseEnter += new MouseEventHandler(
                            new EventHandler((sender, e) => displayDroneInInfoWindow(sender, e, _infoBlock)));
                        newTextBlock.Text = (_infoBlock.numParcelsOrDronesCharging == 0)?
                            "Empty" : "Parcel " + _infoBlock.numParcelsOrDronesCharging.ToString();
                        newTextBlock.Margin = new Thickness(5, 0, 0, 0);
                    }
                    break;
                default:
                    break;
            }
            newTextBlock.Text += "\n" + _infoBlock.name;
            gridMap.Children.Add(newTextBlock);
            return newTextBlock;
        }
        private Image createImage(InfoBlock _infoBlock)
        {
            Image newImage = new Image();
            Grid.SetColumn(newImage, _infoBlock.ColumnPlace);
            Grid.SetRow(newImage, _infoBlock.RowPlace);
            ImageSource _imageSource;
            switch(_infoBlock.ThisObjectType)
            {
                case ObjectType.Drone:
                    {
                        _imageSource = (_infoBlock.numParcelsOrDronesCharging == 0) ?
                                        new BitmapImage(new Uri(ImgDroneWithoutParcel))
                                        : new BitmapImage(new Uri(ImgDroneWithParcel));
                        newImage.MouseEnter += new MouseEventHandler(
                            new EventHandler((sender, e) => displayDroneInInfoWindow(sender, e, _infoBlock)));
                        Grid.SetColumnSpan(newImage, IMAGESIZEforDrones);
                        Grid.SetRowSpan(newImage, IMAGESIZEforDrones);
                    }
                    break;
                case ObjectType.Station:
                    {
                        _imageSource = new BitmapImage(new Uri(ImgStation));
                        newImage.MouseEnter += new MouseEventHandler(
                            new EventHandler((sender, e) => displayStationInInfoWindow(sender, e, _infoBlock)));
                        Grid.SetColumnSpan(newImage, IMAGESIZEforCustAndStations);
                        Grid.SetRowSpan(newImage, IMAGESIZEforCustAndStations);
                    }
                    break;
                case ObjectType.Customer:
                    {
                        _imageSource = new BitmapImage(new Uri(ImgHouse));
                        newImage.MouseEnter += new MouseEventHandler(
                            new EventHandler((sender, e) => displayCustomerInInfoWindow(sender, e, _infoBlock)));
                        Grid.SetColumnSpan(newImage, IMAGESIZEforCustAndStations);
                        Grid.SetRowSpan(newImage, IMAGESIZEforCustAndStations);
                    }
                    break;
                default:
                    _imageSource = null;
                    break;
            }
            newImage.Source = _imageSource;
            newImage.MouseLeftButtonDown += new MouseButtonEventHandler(
                new EventHandler((sender, e) => openWindowOfInfoBlock(sender, e, _infoBlock)));
            newImage.MouseLeave += new MouseEventHandler(this.hideTextInInfoWindow);
            gridMap.Children.Add(newImage);
            return newImage;
        }
        private void displayCustomerInInfoWindow(object sender, System.EventArgs e, MapWindow.InfoBlock _InfoBlock)
        {
            var item = busiAccess.GetBOCustomer(_InfoBlock.Id);

            tBoxInfoWindow.Text = busiAccess.GetOneCustToList(_InfoBlock.Id).ToString()
                + "\n" + "Long: " + Math.Round(item.Location.Longitude, 3).ToString()
                + "\n" + "Lat: " + Math.Round(item.Location.Latitude, 3).ToString();
        }
        private void displayStationInInfoWindow(object sender, System.EventArgs e, MapWindow.InfoBlock _InfoBlock)
        {
            var item = busiAccess.GetBOStation(_InfoBlock.Id);

            tBoxInfoWindow.Text = item.ToString()
                + "\n" + "Long: " + Math.Round(item.Location.Longitude, 3).ToString()
                + "\n" + "Lat: " + Math.Round(item.Location.Latitude, 3).ToString(); 
        }
        private void displayDroneInInfoWindow(object sender, System.EventArgs e, MapWindow.InfoBlock _InfoBlock)
        {
            var item = busiAccess.GetBODrone(_InfoBlock.Id);

            tBoxInfoWindow.Text = item.ToString();
        }
        private void openWindowOfInfoBlock(object sender, EventArgs e, InfoBlock infoBlock)
        {
            switch (infoBlock.ThisObjectType)
            {
                case ObjectType.Drone:
                    new DroneWindow(busiAccess, 
                        busiAccess.GetBODrone(infoBlock.Id)).ShowDialog();
                    break;
                case ObjectType.Station:
                    new StationWindow(busiAccess,
                        (infoBlock.Id)).ShowDialog();
                    break;
                case ObjectType.Customer:
                    new CustomerWindow(busiAccess, 
                busiAccess.GetBOCustomer(infoBlock.Id)).ShowDialog();
                    break;
                default:
                    break;
            }
            refreshMap();
        }
        private void hideTextInInfoWindow(object sender, System.EventArgs e)
        {
            tBoxInfoWindow.Text = emptyTextForInfoWindow;
        }
        private void refreshMap()
        {
            Dispatcher.Invoke(() =>
           {
               clearMap();
               if ((bool)chkboxTextMode.IsChecked)
                   fillMapWithTextBlocks();
               else
                   fillMapWithImages();
           });
        }
        private void clearMap()
        {
            foreach (var item in listTextBlocks)
            {
                gridMap.Children.Remove(item);
            }
            foreach (var item in listImages)
            {
                gridMap.Children.Remove(item);
            }
        }

        //FOR SIMULATOR:
        private void worker_DoWork(object sender, DoWorkEventArgs e) //displays Map, Sleeps
        {
            while(simulatorOn)
            {
                refreshMap();
                Thread.Sleep(DELAY_BTW_REFRESH);
            }
        }
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //HelpfulMethods.SuccessMsg("Simulator Stopped Successfully");
        }
        private void beginSimulator()
        {
            foreach (var item in busiAccess.GetBODroneList()) //begin all Simulators in BL
            {
                if(item.Exists)
                {
                    busiAccess.BeginSimulatorForDrone(item.Id);
                }
            }
            worker.RunWorkerAsync(); //begin simular in PL
        } 
        private void btnSimulator_Click(object sender, RoutedEventArgs e)
        {
            if (!simulatorOn) //turn on simulator...
            {
                simulatorOn = true;
               Thread newThread = new Thread(beginSimulator);
               newThread.Start();
               btnSimulator.Content = "Turn off Simulator";
            }
            else    //turn off simulator
            {
                stopSimulator();
            }
        }
        private void stopSimulator()
        {
            simulatorOn = false;
            worker.CancelAsync();
            btnSimulator.Content = textForSimulatorBtnStart;
            foreach (var item in busiAccess.GetBODroneList()) //stop simulators in BL
            {
                if (item.Exists)
                {
                    busiAccess.StopSimulatorForDrone(item.Id);
                }
            }
            HelpfulFunctions.SuccessMsg("All Drone Simulators canceled");
        }
        //WINDOW:
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(simulatorOn)
                stopSimulator();
        }




        //END OF MAP WINDOW
    }
}
