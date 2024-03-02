using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VortexApp.UI.MVVM.Model
{
    class MessageModel
    {
        public string Username { get; set; }
        public string ImageSource { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public string Color { get; set; } = "White";
        public string Cursor { get; set; } = "IBeam";
        public bool IsFocusable { get; set; } = true;
        public bool IsClickable { get; set; } = false;
        public string FontWeight { get; set; } = "SemiBold";
        public bool IsNativeOrigin { get; set; }
        public bool? FirstMessage { get; set; }


    }
}
