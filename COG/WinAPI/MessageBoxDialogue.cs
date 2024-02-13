using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace COG.WinAPI
{
    public static class MessageBoxDialogue
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

        // From winuser.h
        public enum OpenTypes : uint
        {
            MB_ICONASTERISK = 64,
            MB_ICONEXCLAMATION = 0x30,
            MB_ICONWARNING = 0x30,
            MB_ICONERROR = 16,
            MB_ICONHAND = 16,
            MB_ICONQUESTION = 32,
            MB_OK = 0,
            MB_ABORTRETRYIGNORE = 2,
            MB_APPLMODAL = 0,
            MB_DEFAULT_DESKTOP_ONLY = 0x20000,
            MB_HELP = 0x4000,
            MB_RIGHT = 0x80000,
            MB_RTLREADING = 0x100000,
            MB_TOPMOST = 0x40000,
            MB_DEFBUTTON1 = 0,
            MB_DEFBUTTON2 = 256,
            MB_DEFBUTTON3 = 512,
            MB_DEFBUTTON4 = 0x300,
            MB_ICONINFORMATION = 64,
            MB_ICONSTOP = 16,
            MB_OKCANCEL = 1,
            MB_RETRYCANCEL = 5,

            MB_SETFOREGROUND = 0x10000,
            MB_SYSTEMMODAL = 4096,
            MB_TASKMODAL = 0x2000,
            MB_YESNO = 4,
            MB_YESNOCANCEL = 3,
            MB_ICONMASK = 240,
            MB_DEFMASK = 3840,
            MB_MODEMASK = 0x00003000,
            MB_MISCMASK = 0x0000C000,
            MB_NOFOCUS = 0x00008000,
            MB_TYPEMASK = 15,
        }

        public enum ClickedButton : int
        {
            IDOK = 1,
            IDCANCEL = 2,
            IDABORT = 3,
            IDRETRY = 4,
            IDIGNORE = 5,
            IDYES = 6,
            IDNO = 7
        }

        public static ClickedButton OpenMessageBox(string text, string title, OpenTypes type = OpenTypes.MB_DEFBUTTON1 | OpenTypes.MB_ICONINFORMATION)
        {
            return (ClickedButton)MessageBox(IntPtr.Zero, text, title, (uint)type);
        }
    }
}
