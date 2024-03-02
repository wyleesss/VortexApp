using Ookii.Dialogs.Wpf;
using System.Windows;
using System.Windows.Controls;
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

            string filename = ((TextBox)sender).Text.Substring(6);

            ((MainViewModel)Application.Current.MainWindow.DataContext).DownloadFile(folderBrowserDialog, filename);
        }
    }
}
