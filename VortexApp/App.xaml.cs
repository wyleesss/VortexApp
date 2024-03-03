using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using VortexApp.UI.MVVM.ViewModel;

namespace VortexApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal MainViewModel FormDataContext { get; set; }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Current.Dispatcher.Invoke(() =>
            {
                ;
                if (FormDataContext.CallingContact != null)
                {
                    FormDataContext.client.EndCall(Guid.Parse(FormDataContext.CallingContact.UserID));
                }
                else
                {
                    if (FormDataContext.client != null)
                    {
                        FormDataContext.client.EndCallAccept();
                    }
                }
                if (FormDataContext.client != null)
                {
                    FormDataContext.client.Disconect();
                }
            });
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt && Keyboard.IsKeyDown(Key.F4))
            {
                Shutdown();
            }
        }
    }

}
