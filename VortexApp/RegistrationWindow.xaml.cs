using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;
using VortexApp.UI.MVVM.Model;
using VortexApp.UI.MVVM.ViewModel;
using VortexApp.UI.Helpers;
using Microsoft.Win32;
using System.IO;
using System.Threading;

namespace VortexApp
{
    /// <summary>
    /// Interaction logic for CallWindow.xaml
    /// </summary>
    /// 
    public partial class RegistrationWindow : Window
    {

        public DispatcherTimer timer;
        private TimeSpan elapsedTime;
        private DateTime startTime;
        public RegistrationWindow()
        {
            InitializeComponent();

            MaxHeight = 1080 - TaskbarHelper.GetTaskbarHeight();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            Close();
            Application.Current.Shutdown();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            MaximizeButton.Content = FindResource(MaximizeButton.Content == FindResource("Windowed") ? "Fullscreen" : "Windowed");

            if (WindowState == WindowState.Maximized)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                ResizeMode = ResizeMode.CanResize;
                return;
            }

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Minimized;
            WindowStyle = WindowStyle.None;
        }

        private void SignInTextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!SignInTextBlock.IsSelected)
            {
                SignInContent.Visibility = Visibility.Visible;
                SignUpContent.Visibility = Visibility.Hidden;
                SignInTextBlock.IsSelected = true;
                SignUpTextBlock.IsSelected = false;
            }
        }

        private void SignUpTextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!SignUpTextBlock.IsSelected)
            {
                SignUpContent.Visibility = Visibility.Visible;
                SignInContent.Visibility = Visibility.Hidden;
                SignUpTextBlock.IsSelected = true;
                SignInTextBlock.IsSelected = false;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EnterProgram();
            }
        }

        private void EnterProgram()
        {
            if (SignUpTextBlock.IsSelected)
            {

            }

            Application.Current.MainWindow.Show();
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EnterProgram();
        }
    }
}