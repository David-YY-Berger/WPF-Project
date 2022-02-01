using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Presentation_Layer
{
    /// <summary>
    /// Interaction logic for ParcelListWindow.xaml
    /// </summary>
    public partial class ParcelListWindow : Window
    {
        BL.BLApi.Ibl busiAccess;
        public ICollectionView parcCollectionView { get; set; }
        public ParcelListWindow(BL.BLApi.Ibl busiAccess1)
        {
            InitializeComponent();
            busiAccess = busiAccess1;
            refreshList();
        }

        private void refreshList()
        {
            DataContext = busiAccess.GetParcelToList(); // for grouping...
            parcCollectionView = (CollectionView)CollectionViewSource.
                GetDefaultView(DataContext);
            ParcelListView.ItemsSource = parcCollectionView;

            if ((bool)chkboxSortSender.IsChecked)
                parcCollectionView.GroupDescriptions.
                                   Add(new PropertyGroupDescription(nameof(BL.BO.BOParcelToList.NameSender)));
            if ((bool)chkboxSortPriority.IsChecked)
                parcCollectionView.GroupDescriptions.
                    Add(new PropertyGroupDescription(nameof(BL.BO.BOParcelToList.Priority)));
            
            if ((bool)!chkboxShowErased.IsChecked)
                parcCollectionView.Filter = filterOutErased;

            parcCollectionView.SortDescriptions.Add(new SortDescription
                (nameof(BL.BO.BOParcelToList.Id), ListSortDirection.Ascending));
        }

        private void btnAddParcel_Click(object sender, RoutedEventArgs e)
        {
            new ParcelWindow(busiAccess).ShowDialog();
            refreshList();
        }

        private void ParcelListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            BL.BO.BOParcelToList parcel = ParcelListView.SelectedItem as BL.BO.BOParcelToList;
            if (parcel == null)
                return;
            int id = parcel.Id;
            try
            {
                BL.BO.BOParcel parc = busiAccess.GetBOParcel(id);
                new ParcelWindow(busiAccess, parc).ShowDialog();
                refreshList();
            }
            catch (BL.BLApi.EXParcelNotFound ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }
            
            
        }

        private void btnCloseList_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void chkboxShowErased_Checked(object sender, RoutedEventArgs e)
        {
            refreshList();
        }
        private void chkboxShowErased_Unchecked(object sender, RoutedEventArgs e)
        {
            refreshList();
        }
        private bool filterOutErased(object obj)
        {
            if (obj is BL.BO.BOParcelToList item)
            {
                return item.Exists;
            }
            else return false;
        }

       
        private void clearGrouping() //removes groupDescriptions
        {
            var groupDescrip = parcCollectionView.GroupDescriptions.OfType<PropertyGroupDescription>()
                .FirstOrDefault(x => x.PropertyName == nameof(BL.BO.BOParcelToList.Priority));
            if (groupDescrip != null)
                parcCollectionView.GroupDescriptions.Remove(groupDescrip);
            
            groupDescrip = parcCollectionView.GroupDescriptions.OfType<PropertyGroupDescription>()
                .FirstOrDefault(x => x.PropertyName == nameof(BL.BO.BOParcelToList.NameSender));
            if (groupDescrip != null)
                parcCollectionView.GroupDescriptions.Remove(groupDescrip);
        }

        private void chkboxSortSender_Checked(object sender, RoutedEventArgs e)
        {
            //if (chkboxSortPriority.IsChecked == true)
            //{
            //    clearGrouping(); chkboxSortPriority.IsChecked = false;
            //}
            refreshList();    
        }
        private void chkboxSortSender_Unchecked(object sender, RoutedEventArgs e)
        {
            refreshList();
        }
       
        private void chkboxByPriority_Checked(object sender, RoutedEventArgs e)
        {
            refreshList();
            //if (chkboxSortSender.IsChecked == true)
            //{
            //    clearGrouping(); chkboxSortSender.IsChecked = false;
            //}

            //clearGrouping();
            //if ((bool)chkboxSortPriority.IsChecked)
            //    parcCollectionView.GroupDescriptions.
            //        Add(new PropertyGroupDescription(nameof(BL.BO.BOParcelToList.Priority)));

           // chkboxSortSender.IsChecked = false;
        }
        private void chkboxByPriority_Unchecked(object sender, RoutedEventArgs e)
        {
            refreshList();
        }

    }
}
