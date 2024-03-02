using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VortexApp.UI.MVVM.Model
{
    class ContactModel
    {
        public string Username { get; set; }
        public string UserID { get; set; }
        public string ImageSource { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; }
        public string LastMessage => Messages.Count != 0 ? Messages.Last().Message : string.Empty;
    }
}
