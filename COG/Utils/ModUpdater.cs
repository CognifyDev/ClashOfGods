#if WINDOWS
#pragma warning disable SYSLIB0014
using System.Diagnostics;
using System.IO;
using System.Net;
using COG.Utils.Version;

namespace COG.Utils;

public static class ModUpdater
{
    public static VersionInfo LatestVersion { get; private set; } = VersionInfo.Empty;
    public static string LatestDescription { get; private set; } = "";

    public static void FetchUpdate()
    {
        string? latestVersionString = null;
        var description = "";

        try
        {
            Main.Logger.LogInfo("Checking for updates...");
            // TODO: Implement actual version check logic
        }
        catch (System.Exception e)
        {
            Main.Logger.LogWarning($"Failed to check for updates: {e.Message}");
        }

        LatestVersion = latestVersionString == null
            ? VersionInfo.Empty
            : VersionInfo.Parse(latestVersionString);

        LatestDescription = description;
    }

    public static void DoUpdate()
    {
        using var client = new WebClient();
        client.DownloadFile(
            $"http://download.cognifydev.cn/CognifyDev/ClashOfGods/releases/download/{LatestVersion}/ClashOfGods.dll",
            "BepInEx/plugins/ClashOfGods.dll.new"
        );

        File.WriteAllText("BepInEx/plugins/do.vbs",
            """
            WScript.Sleep 1000

            strFileToDelete = "ClashOfGods.dll"
            strFileToRename = "ClashOfGods.dll.new"
            strScriptToDelete = WScript.ScriptFullName

            Set fs = CreateObject("Scripting.FileSystemObject")

            If fs.FileExists(strFileToDelete) Then
                fs.DeleteFile strFileToDelete
            End If

            If fs.FileExists(strFileToRename) Then
                fs.MoveFile strFileToRename, strFileToDelete
            End If

            If fs.FileExists(strScriptToDelete) Then
                fs.DeleteFile strScriptToDelete
            End If
            """);

        Process.Start("BepInEx/plugins/do.vbs");
    }
}
#endif
