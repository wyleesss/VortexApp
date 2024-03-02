using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VortexApp.UI.MVVM.ViewModel;

namespace VortexApp.UI.Themes
{
    public partial class TextBoxHighlight
    {
        public void TextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var folderBrowserDialog = new VistaFolderBrowserDialog();
            var result = folderBrowserDialog.ShowDialog();

            if (result != true)
            {
                return;
            }

            ((MainViewModel)Application.Current.MainWindow.DataContext).DownloadFile(folderBrowserDialog);
        }
    }
}
