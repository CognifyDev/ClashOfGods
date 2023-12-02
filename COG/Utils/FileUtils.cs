using System.IO;

namespace COG.Utils;

public static class FileUtils
{
    public static string GetNameWithoutExtension(this FileSystemInfo fileInfo) 
        => string.IsNullOrEmpty(fileInfo.FullName) || fileInfo == null! ? string.Empty : Path.GetFileNameWithoutExtension(fileInfo.FullName);
}