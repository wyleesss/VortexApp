using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using VortexApp.UI.Helpers;
using VortexApp.UI.MVVM.Model;
using VortexApp.UI.MVVM.ViewModel;

namespace VortexApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public RegistrationWindow registrationWindow;
        public MainWindow()
        {
            InitializeComponent();

            MaxHeight = 1080 - TaskbarHelper.GetTaskbarHeight();

            CollectionView contactView = (CollectionView)CollectionViewSource.GetDefaultView(((MainViewModel)DataContext).Contacts);
            CollectionView requestView = (CollectionView)CollectionViewSource.GetDefaultView(((MainViewModel)DataContext).Requests);

            contactView.Filter = ContactsViewFilter;
            requestView.Filter = RequestsViewFilter;

            Visibility = Visibility.Hidden;
            registrationWindow = new();
            registrationWindow.Show();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = ((MainViewModel)Application.Current.MainWindow.DataContext);
                if (context.client != null)
                {
                    context.client.Disconect();
                }
            }));
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

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddContactInfo.Visibility = Visibility.Visible;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsInfo.Visibility = Visibility.Visible;
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedUserInfo.Visibility = Visibility.Visible;
        }

        private void TextBoxSelectAll(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            ((TextBox)sender).SelectAll();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Placeholder.Visibility = Visibility.Collapsed;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                Placeholder.Visibility = Visibility.Visible;
            }
        }

        private void ContactsTextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!ContactsTextBlock.IsSelected)
            {
                ContactsTextBlock.IsSelected = true;
                RequestsTextBlock.IsSelected = false;

                MainListView.ItemsSource = ((MainViewModel)DataContext).Contacts;
                MainListView.ItemContainerStyle = (Style)FindResource("ContactCard");
            }
        }

        private void RequestsTextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!RequestsTextBlock.IsSelected)
            {
                RequestsTextBlock.IsSelected = true;
                ContactsTextBlock.IsSelected = false;

                MainListView.ItemsSource = ((MainViewModel)DataContext).Requests;
                MainListView.ItemContainerStyle = (Style)FindResource("RequestCard");
            }
        }

        private void CancelTextBlock_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            UserSettingsInfo.Visibility = Visibility.Hidden;
            SettingsUsername.Text = ((MainViewModel)DataContext).User.Username;
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            Placeholder.Visibility = Visibility.Visible;
        }

        private bool ContactsViewFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                return true;
            }

            return ((ContactModel)item).Username.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool RequestsViewFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                return true;
            }

            return ((RequestModel)item).Username.IndexOf(SearchBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(MainListView.ItemsSource).Refresh();
        }

        private void MainListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0 && e.RemovedItems.Count > 0)
            {
                MainListView.SelectedItem = e.RemovedItems[0];
                return;
            }

            if (e.AddedItems.Count > 0)
            {
                var scrollViewer = GetScrollViewer(MessageListView);

                SelectedContactInteraction.Visibility = Visibility.Visible;
                MessagesInteraction.Visibility = Visibility.Visible;
                scrollViewer?.ScrollToEnd();
                return;
            }

            SelectedContactInteraction.Visibility = Visibility.Hidden;
            MessagesInteraction.Visibility = Visibility.Hidden;
        }

        private ScrollViewer? GetScrollViewer(DependencyObject depObj)
        {
            if (depObj is ScrollViewer scrollViewer)
                return scrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetScrollViewer(child);
                if (result != null)
                    return result;
            }

            return null;
        }

        private void MessageBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                {
                    MessageBox.Text += Environment.NewLine;
                    return;
                }

                var scrollViewer = GetScrollViewer(MessageListView);
                scrollViewer?.ScrollToEnd();
            }
        }

        private void MoreCloseButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedUserInfo.Visibility = Visibility.Hidden;
        }

        private void SettingsCloseButton_Click(object sender, RoutedEventArgs e)
        {
            UserSettingsInfo.Visibility = Visibility.Hidden;
        }

        private void AddContactCloseButton_Click(object sender, RoutedEventArgs e)
        {
            AddContactInfo.Visibility = Visibility.Hidden;
        }

        private void AddContactButton_Click(object sender, RoutedEventArgs e)
        {
            Guid friendReq;
            if (!Guid.TryParse(NewContactID.Text, out friendReq))
            {
                NewContactIDError.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    var context = ((MainViewModel)Application.Current.MainWindow.DataContext);
                    if (context.client != null)
                    {
                        context.client.SendFriendRequest(friendReq);
                    }
                }));
            }

        }

        private void SendFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            ((MainViewModel)DataContext).SendFile(openFileDialog);

            var scrollViewer = GetScrollViewer(MessageListView);
            scrollViewer?.ScrollToEnd();
        }

        private void AcceptCallRequest(object sender, RoutedEventArgs e)
        {
            CallRequest.Visibility = Visibility.Hidden;
            CallWindow callWindow = new();
            callWindow.Title = ((MainViewModel)DataContext).CallingContact.Username + " - Call";
            callWindow.Show();
        }

        private void DeclineCallRequest(object sender, RoutedEventArgs e)
        {
            CallRequest.Visibility = Visibility.Hidden;
            ((MainViewModel)DataContext).CallingContact = null;
        }

        private void CancelCallRequest(object sender, RoutedEventArgs e)
        {
            CallResponseWaiting.Visibility = Visibility.Hidden;
            ((MainViewModel)DataContext).CallingContact = null;
        }

        private void CallButton_Click(object sender, RoutedEventArgs e)
        {
            ((MainViewModel)DataContext).CallingContact = new()
            {
                UserID = ((MainViewModel)DataContext).SelectedContact.UserID,
                ImageSource = ((MainViewModel)DataContext).SelectedContact.ImageSource,
                Username = ((MainViewModel)DataContext).SelectedContact.Username
            };

            CallResponseWaiting.Visibility = Visibility.Visible;
        }
    }
}