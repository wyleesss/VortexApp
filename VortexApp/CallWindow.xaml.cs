using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using VortexApp.UI.Helpers;
using VortexApp.UI.MVVM.Model;
using VortexApp.UI.MVVM.ViewModel;

namespace VortexApp
{
    /// <summary>
    /// Interaction logic for CallWindow.xaml
    /// </summary>
    /// 
    public partial class CallWindow : Window
    {

        public DispatcherTimer timer;
        private TimeSpan elapsedTime;
        public DateTime startTime;
        public CallWindow()
        {
            InitializeComponent();

            MaxHeight = 1080 - TaskbarHelper.GetTaskbarHeight();

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);

            var test = ((MainViewModel)DataContext).Contacts;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;
            TimerTextBox.Text = elapsedTime.ToString(@"hh\:mm\:ss");
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

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ScreenShareButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EndCallButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)DataContext).client.EndCall(Guid.Parse(((MainViewModel)DataContext).CallingContact.UserID));
            ((MainViewModel)DataContext).SelectedContact.Messages.Add(new MessageModel
            {
                Username = "Call Status",
                Time = DateTime.Now,
                Message = "Call Ended" + (DateTime.Now - startTime).ToString(@"hh\:mm\:ss"),
            });
            ((MainViewModel)DataContext).CallingContact = null;
            timer.Stop();
            this.Close();
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}