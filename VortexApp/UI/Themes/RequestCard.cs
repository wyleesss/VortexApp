using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VortexApp.UI.MVVM.ViewModel;

namespace VortexApp.UI.Themes
{
    public partial class RequestCard
    {
        public void ApplyRequest(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)Application.Current.MainWindow.DataContext).ApplyRequest();
        }

        public void DeclineRequest(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)Application.Current.MainWindow.DataContext).DeclineRequest();
        }
    }
}
