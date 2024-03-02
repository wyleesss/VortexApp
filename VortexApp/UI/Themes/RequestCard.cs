using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VortexApp.UI.MVVM.Model;
using VortexApp.UI.MVVM.ViewModel;

namespace VortexApp.UI.Themes
{
    public partial class RequestCard
    {
        public void ApplyRequest(object sender, RoutedEventArgs e)
        {
            ListViewItem listViewItem = FindAncestor<ListViewItem>((Button)sender);

            RequestModel item = (RequestModel)listViewItem.DataContext;

            string userID = item.UserID;

            ((MainViewModel)Application.Current.MainWindow.DataContext).ApplyRequest(userID);
        }

        public void DeclineRequest(object sender, RoutedEventArgs e)
        {
            ListViewItem listViewItem = FindAncestor<ListViewItem>((Button)sender);

            RequestModel item = (RequestModel)listViewItem.DataContext;

            string userID = item.UserID;

            ((MainViewModel)Application.Current.MainWindow.DataContext).DeclineRequest(userID);
        }

        public static T FindAncestor<T>(DependencyObject dependencyObject)
            where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);

            if (parent == null) return null;

            var parentT = parent as T;
            return parentT ?? FindAncestor<T>(parent);
        }
    }
}
