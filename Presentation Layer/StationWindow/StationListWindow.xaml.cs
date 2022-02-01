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
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Presentation_Layer
{
    /// <summary>
    /// Interaction logic for StationListWindow.xaml
    /// </summary>
    
    public partial class StationListWindow : Window
    {
        BL.BLApi.Ibl busiAccess;
        public ICollectionView StationCollectionView { get; set; }

        public StationListWindow(BL.BLApi.Ibl _busiAccess)
        {
            InitializeComponent();
            busiAccess = _busiAccess;
            refreshList();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LstViewStation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BL.BO.BOStationToList st = LstViewStation.SelectedItem as BL.BO.BOStationToList;
            new StationWindow(busiAccess, (LstViewStation.SelectedItem as BL.BO.BOStationToList).Id).ShowDialog();
            refreshList();
        }

        private void btnAddStation_Click(object sender, RoutedEventArgs e)
        {
            new StationWindow(busiAccess).ShowDialog();
            refreshList();
        }

        private void chkBoxGetErased_Checked(object sender, RoutedEventArgs e)
        {
            refreshList();
        }

        private void chkBoxGetErased_UnChecked(object sender, RoutedEventArgs e)
        {
            refreshList();
        }
        private void refreshList()
        {
            DataContext = busiAccess.GetStationToList(); // for grouping...
            StationCollectionView = (CollectionView)CollectionViewSource.
                GetDefaultView(DataContext);
            LstViewStation.ItemsSource = StationCollectionView;

            if ((bool)!chkBoxGetErased.IsChecked)
                StationCollectionView.Filter = filterOutErased;

            StationCollectionView.SortDescriptions.Add(new SortDescription
                (nameof(BL.BO.BOStationToList.Id), ListSortDirection.Ascending));
        }
        private bool filterOutErased(object obj)
        {
            if (obj is BL.BO.BOStationToList item)
                return item.Exists;
            else return false;
        }




        //END OF STATION LIST WINDOW
    }
}
