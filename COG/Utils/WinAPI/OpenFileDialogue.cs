using System;
using System.Runtime.InteropServices;

namespace COG.Utils.WinAPI;

public static class OpenFileDialogue
{
    public enum OpenFileMode
    {
        Open,
        Save
    }

    [DllImport("Comdlg32.dll", CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true, SetLastError = true)]
    private static extern bool GetOpenFileName([In] [Out] OPENFILENAME ofn);

    [DllImport("Comdlg32.dll", CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true, SetLastError = true)]
    private static extern bool GetSaveFileName([In] [Out] OPENFILENAME ofn);

    /// <summary>
    ///     更加灵活的打开对话框方法，但运用困难
    /// </summary>
    public static void Open(OPENFILENAME ofn)
    {
        GetOpenFileName(ofn);
    }

    /// <summary>
    ///     更加灵活的保存对话框方法，但运用困难
    /// </summary>
    public static void Save(OPENFILENAME ofn)
    {
        GetSaveFileName(ofn);
    }

    /// <summary>
    ///     更加易用的打开/保存对话框方法，但有一定限制
    /// </summary>
    /// <param name="mode">对话框模式</param>
    /// <param name="filter">文件筛选器</param>
    /// <param name="title">对话框标题</param>
    /// <param name="defaultDir">打开对话框后默认所在目录</param>
    /// <param name="defaultFilterIdx">默认文件筛选编号</param>
    /// <returns>文件名称与路径</returns>
    public static OpenedFileInfo Open(OpenFileMode mode, string filter = "", string title = "", string defaultDir = "",
        int? defaultFilterIdx = null)
    {
        var ofn = new OPENFILENAME();
        ofn.lStructSize = Marshal.SizeOf(ofn);
        ofn.stringFilter = filter;
        ofn.stringTitle = title;
        ofn.stringInitialDir = defaultDir;
        if (defaultFilterIdx.HasValue) ofn.nFilterIndex = defaultFilterIdx.Value;

        if (mode == OpenFileMode.Open)
            Open(ofn);
        else
            Save(ofn);
        Main.Logger.LogInfo(
            $"Opened file: {(ofn.stringFile.Equals("") || ofn.stringFile == null ? "None" : ofn.stringFile)}");
        return new OpenedFileInfo(ofn.stringFile, ofn.stringFileTitle);
    }

#nullable disable
    // Class(Struct) from commdlg.h
    // 这玩意最好一点都别动，包括成员顺序！！！！！！名字随便改但顺序不能改！！！！！！
    // Windows 底层的东西可不能乱来！
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class OPENFILENAME // 这玩意最好一点都别动，包括成员顺序！！！！！！名字随便改但顺序不能改！！！！！！
    {
        public int dwReserved;
        public int Flags = 1 << 3;
        public int FlagsEx;
        public IntPtr hInstance;
        public IntPtr IntPtrOwner;
        public int lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public int lStructSize;
        public short nFileExtension;
        public short nFileOffset;
        public int nFilterIndex;
        public int nMaxCustFilter;
        public int nMaxFile = 256;
        public int nMaxFileTitle = 64;
        public IntPtr pvReserved;
        public string stringCustomFilter;
        public string stringDefExt;
        public string stringFile = new(new char[256]);
        public string stringFileTitle = new(new char[64]);
        public string stringFilter;
        public string stringInitialDir;
        public string stringTitle;
    }
#nullable enable

    public class OpenedFileInfo
    {
        public OpenedFileInfo(string? filePath, string? fileName)
        {
            FilePath = filePath;
            FileName = fileName;
        }

        public string? FilePath { get; }
        public string? FileName { get; }
    }
}