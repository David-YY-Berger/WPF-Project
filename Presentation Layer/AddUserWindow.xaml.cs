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
    /// Interaction logic for AddUserWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {
        BL.BLApi.Ibl busiAccess;

        public AddUserWindow(BL.BLApi.Ibl _busiAccess)
        {
            busiAccess = _busiAccess;
            InitializeComponent();
            changeVisibilty(System.Windows.Visibility.Hidden);

            btnCreateNewUser.IsEnabled = false;
        }

        public AddUserWindow(BL.BLApi.Ibl _busiAccess, int _id) : this(_busiAccess)
        {
            changeVisibilty(System.Windows.Visibility.Visible);
            tBoxIdInput.Text = _id.ToString();
            tBoxIdInput.IsEnabled = false;
            btnCreateNewUser.IsEnabled = true;

        }

        private void btnGetMyInfo_Click(object sender, RoutedEventArgs e)
        {
            int _id;
            bool idSuccess = Int32.TryParse(tBoxIdInput.Text, out _id);
            if (tBoxIdInput.Text == null || !idSuccess || _id <= 0)
            {
                HelpfulFunctions.ErrorMsg("Invalid Id number"); return;
            }

            tBlockNameInput.Text = busiAccess.GetBOCustomer(_id).Name;
            changeVisibilty(System.Windows.Visibility.Visible);
            tBoxIdInput.IsEnabled = false;
            btnCreateNewUser.IsEnabled = true;

        }
        private void changeVisibilty(System.Windows.Visibility vis)
        {
            tBlockName.Visibility = vis;
            tBlockNameInput.Visibility = vis;
            tBlockPassword.Visibility = vis;
            //tBlockUserId.Visibility = vis;
            tBlockUserName.Visibility = vis;

            tBoxPasswordInput.Visibility = vis;
            tBoxUsernameInput.Visibility = vis;
            btnCreateNewUser.Visibility = vis;
        }

        private void btnCreateNewUser_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                busiAccess.AddUser(tBoxUsernameInput.Text, tBoxPasswordInput.Text,
                    Int32.Parse(tBoxIdInput.Text));
                MessageBox.Show("User Added Successfully", 
                    "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                new LoginWindow(busiAccess).Show();
                Close();
            }
            catch (BL.BLApi.EXUserAlreadyExists ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow(busiAccess).Show();
            Close();
        }
    }
}
