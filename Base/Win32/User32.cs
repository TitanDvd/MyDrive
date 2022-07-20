using MyDrive.Base.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetDrive.Base.Win32
{
    public class User32
    {
        [DllImport("User32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);


        [DllImport("User32.dll")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);


        [DllImport("User32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, int flags);


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
           public Kernel32.POINT ptReserved;
           public Kernel32.POINT ptMaxSize;
           public Kernel32.POINT ptMaxPosition;
           public Kernel32.POINT ptMinTrackSize;
           public Kernel32.POINT ptMaxTrackSize;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }


        public enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }


        public enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }
    }
}
