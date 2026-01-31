using System;
using UnityEngine;
using System.Runtime.InteropServices;

namespace COG.Utils
{
    public static class TextUtils
    {
        public static string HeadLower(this string text) => char.ToLower(text[0]) + text[1..];
        public static string HeadUpper(this string text) => char.ToUpper(text[0]) + text[1..];
        public static string GetClipboardString()
        {
            uint type = 0;
            if (ClipboardHelper.IsClipboardFormatAvailable(1U)) { type = 1U; Debug.Log("ASCII"); }
            if (ClipboardHelper.IsClipboardFormatAvailable(13U)) { type = 13U; Debug.Log("UNICODE"); }
            if (type == 0) return "";

            string result;
            try
            {
                if (!ClipboardHelper.OpenClipboard(IntPtr.Zero))
                {
                    result = "";
                }
                else
                {

                    IntPtr clipboardData = ClipboardHelper.GetClipboardData(type);
                    if (clipboardData == IntPtr.Zero)
                        result = "";
                    else
                    {
                        IntPtr intPtr = IntPtr.Zero;
                        try
                        {
                            intPtr = ClipboardHelper.GlobalLock(clipboardData);
                            int len = ClipboardHelper.GlobalSize(clipboardData);

                            if (type == 1U)
                                result = Marshal.PtrToStringAnsi(clipboardData, len);
                            else
                            {
                                result = Marshal.PtrToStringUni(clipboardData) ?? "";
                            }
                        }
                        finally
                        {
                            if (intPtr != IntPtr.Zero) ClipboardHelper.GlobalUnlock(intPtr);
                        }
                    }
                }
            }
            finally
            {
                ClipboardHelper.CloseClipboard();
            }
            return result;
        }
    }
}
