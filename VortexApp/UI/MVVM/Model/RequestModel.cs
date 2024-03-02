using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VortexApp.UI.MVVM.Model
{
    internal class RequestModel
    {
        public string Username { get; set; }
        public string UserID { get; set; }
        public string ImageSource { get; set; }

        public ContactModel Contact { get; set; }
    }
}
