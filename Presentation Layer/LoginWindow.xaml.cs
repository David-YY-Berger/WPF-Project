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
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {

        BL.BLApi.Ibl busiAccess = BL.BLApi.FactoryBL.GetBL();
        public LoginWindow()
        {
            InitializeComponent();
            tBlockUserInfo.Text = "For Employee: \n" +
                "user: boss \n" +
                "password: bPassword\n" +
                "\n" +
                "For Customer:\n" +
                "user: Reuven613\n" +
                "password: rPassword";
            btnOpenMain.Content = "Default Login\n" +
                "(View as Employee\n" +
                " without sign-in)";
        }
        public LoginWindow(BL.BLApi.Ibl _busiAccess) : this()
        {
            busiAccess = _busiAccess;
        }

        private void btnOpenMain_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow(busiAccess).Show();
            Close();
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            HelpfulFunctions.ChangeTextColor(Colors.Black, tBlockUsername, tBlockPassword);

            string _username = tBoxUsernameInput.Text;
            string _password = tBoxPasswordInput.Text;

            //(2) Check that Data is Valid
            bool validData = true;
            if (tBoxUsernameInput.Text == null)
            {
                tBlockUsername.Foreground = new SolidColorBrush(Colors.Red);
                validData = false;
            }
            if (tBoxPasswordInput.Text == null)
            {
                tBlockPassword.Foreground = new SolidColorBrush(Colors.Red);
                validData = false;
            }
            if (!validData)
                return;

            try
            {
               int _id = busiAccess.GetIdOfUser(_username, _password);
               if (_id == -1)
                {
                    new MainWindow(busiAccess).Show(); //User is Employee
                    Close();
                }
                else
                {
                    new CustomerWindow(busiAccess, _id).Show(); //User is a Customer
                    Close();
                }
                Close();
            }
            catch (BL.BLApi.EXUsernameNotFound ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }
            catch (BL.BLApi.EXUserPasswordIncorrect ex)
            {
                HelpfulFunctions.ErrorMsg(ex.ToString());
            }

        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            //for a new customer
            new CustomerWindow(busiAccess, true).Show();
            Close();
        }

        private void btnCreateOnlineAcct_Click(object sender, RoutedEventArgs e)
        {
            //for an existing customer, without a Username and Password
            new AddUserWindow(busiAccess).Show();
            Close();
        }
    }
}
