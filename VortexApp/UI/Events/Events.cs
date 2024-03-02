using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VortexApp.UI.MVVM.Model;
using VortexApp.UI.MVVM.ViewModel;
using System.Windows;

namespace VortexApp.UI.Events
{
    internal class UIEvents
    {
        public static void UserNotConnectedUI(string friendID)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                var context = (MainViewModel)Application.Current.MainWindow.DataContext;
                var contact = context.Contacts.Where(o => o.UserID == friendID).FirstOrDefault();

                contact.Messages.Add(new MessageModel { Message = "User is not connected. Your message has not been sent" });
            }));

        }
    }
}
