using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MyDrive.Base.Win32.Kernel32;

namespace MyDrive.Base
{
    public class KeyBoardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelMouseProc _callback;
        private IntPtr _hookId = IntPtr.Zero;

        public delegate void KeyPressed(Keys key);
        public delegate void KeyRealesed(Keys key);
        public event KeyPressed OnKeyPressed;
        public event KeyPressed OnKeyRealesed;


        public KeyBoardHook() => _callback = HookCallback;


        public void Hook() => _hookId = SetHook(_callback);


        public void UnHook() => UnhookWindowsHookEx(_hookId);


        private IntPtr SetHook(LowLevelMouseProc callback)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
                return SetWindowsHookEx(WH_KEYBOARD_LL, callback, GetModuleHandle(curModule.ModuleName), 0);
        }


        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            KBDLLHOOKSTRUCT hookStruct = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
            if(nCode >= 0 && wParam == (IntPtr)0x0100)
                OnKeyPressed?.BeginInvoke((Keys)hookStruct.vkCode, null, null);
            if(nCode >= 0 && wParam == (IntPtr)0x0101)
                OnKeyRealesed?.BeginInvoke((Keys)hookStruct.vkCode, null, null);
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
            
        }
    }
}
