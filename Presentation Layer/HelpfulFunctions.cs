using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Media;


namespace Presentation_Layer
{
    static class HelpfulFunctions
    {
        /// <summary>
        /// this class holds different WPF methods  which are very common
        /// all functionality is simple
        /// </summary>
        /// <param name="msg"></param>

        public static void ErrorMsg(string msg)
        {
            MessageBox.Show(msg, "Error",
                   MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }
        public static void SuccessMsg(string msg)
        {
            MessageBox.Show(msg, "SUCCESS", 
                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);

        }
        public static void ChangeTextColor(Color color, params TextBlock[] listTBlock)
        {
            foreach (var item in listTBlock)
            {
                item.Foreground = new SolidColorBrush(color);
            }
        }
        public static void ChangeVisibilty(System.Windows.Visibility vis, params TextBlock[] listTBlock)
        {
            foreach (var item in listTBlock)
            {
                item.Visibility = vis;
            }
        }
        public static void ChangeVisibilty(System.Windows.Visibility vis, params Button[] listButton)
        {
            //function hides or shows buttons, AND updates whether or not they are enabled

            if (vis == Visibility.Hidden || vis == Visibility.Collapsed)
                foreach (var item in listButton)
                {
                    item.IsEnabled = false;
                    item.Visibility = vis;
                }
            else if (vis == Visibility.Visible)
                foreach (var item in listButton)
                {
                    item.IsEnabled = true;
                    item.Visibility = vis;
                }
        }
        public static bool IsWindowOpen(this Window window)
        {
            return Application.Current.Windows.Cast<Window>().Any(x => x == window);
        }



        //end of helpful methods file
    }
}
