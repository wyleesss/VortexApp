using Client.Services.Network.Utilits;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VortexApp.UI.Helpers;
using VortexApp.UI.MVVM.ViewModel;

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
                ((MainViewModel)Application.Current.MainWindow.DataContext).client = new(ChatHelper.SERVER_PORT, ChatHelper.SERVER_IP, NetworkInterfaceUtility.GetRadminVPNIPAddress().ToString(),
                    SignUpUsername.Text, SignUpPassword.Password, SignUpUsername.Text, SignUpGmailListBox.Text, DateTime.Now);
                ((MainViewModel)Application.Current.MainWindow.DataContext).client.Init();
                return;
            }

            //((MainViewModel)Application.Current.MainWindow.DataContext).client = new(10000, "26.144.152.222", SignInGmailListBox.Text, SignInPassword.Password, 
            //    NetworkInterfaceUtility.GetRadminVPNIPAddress().ToString());
            ((MainViewModel)Application.Current.MainWindow.DataContext).client = new(ChatHelper.SERVER_PORT, ChatHelper.SERVER_IP, SignInGmailListBox.Text,
                SignInPassword.Password, NetworkInterfaceUtility.GetRadminVPNIPAddress().ToString());
            ((MainViewModel)Application.Current.MainWindow.DataContext).client.Init();
            //Application.Current.MainWindow.Show();
            //this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EnterProgram();
        }
    }
}