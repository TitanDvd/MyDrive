using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static MyDrive.Base.Win32.Kernel32;

namespace NetDrive.Base
{
    public class MouseHook
    {
        private const int WH_MOUSE_LL = 14;
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelMouseProc callback;
        private IntPtr _hookId = IntPtr.Zero;

        public delegate void MouseMove(int x, int y);
        public event MouseMove OnMouseMove;


        public delegate void MouseClick(int x, int y);
        public event MouseClick OnMouseClick;


        public MouseHook()
        {
            callback = HookCallback;
        }


        public void Hook()
        {
            _hookId = SetHook(callback);
        }


        public void UnHook()
        {
            UnhookWindowsHookEx(_hookId);
        }


        private IntPtr SetHook(LowLevelMouseProc callback)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
                return SetWindowsHookEx(WH_MOUSE_LL, callback, GetModuleHandle(curModule.ModuleName), 0);
        }


        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                OnMouseMove?.Invoke(hookStruct.pt.x, hookStruct.pt.y);
            }

            if (nCode >= 0 && MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam)
            {
                MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                OnMouseClick?.Invoke(hookStruct.pt.x, hookStruct.pt.y);
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
}
