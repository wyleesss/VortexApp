using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VortexApp.UI.Helpers
{
    public class TaskbarHelper
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern nint SHAppBarMessage(int msg, ref APPBARDATA data);

        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            public int cbSize;
            public nint hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rc;
            public nint lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left, top, right, bottom;
        }

        public static int GetTaskbarHeight()
        {
            APPBARDATA data = new APPBARDATA();
            data.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
            nint result = SHAppBarMessage(5, ref data);
            if (result != nint.Zero)
            {
                return data.rc.bottom - data.rc.top;
            }
            return 0;
        }
    }
}
