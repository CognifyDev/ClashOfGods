#pragma warning disable SYSLIB0014
using COG.Utils;
using COG.Utils.Version;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace COG;

/// <summary>
/// 模组更新类
/// </summary>
public static class ModUpdater
{
    public static VersionInfo? LatestVersion { get; private set; }
    public static string LatestDescription { get; private set; } = "";

    public static void FetchUpdate()
    {
        string? latestVersionString = null;
        var description = "";

        try
        {
            var webText = WebUtils.GetWebByAPIMethod(
                "https://api.github.com/repos/CognifyDev/ClashOfGods/releases/latest",
                "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");
            var jsonObject =
                JObject.Parse(webText);
            var tagNameToken = jsonObject["tag_name"];
            var tagBodyToken = jsonObject["body"];

            if (tagNameToken is { Type: JTokenType.String }) latestVersionString = tagNameToken.ToString();
            if (tagBodyToken is { Type: JTokenType.String }) description = tagBodyToken.ToString();
        }
        catch
        {
            // ignored
        }

        LatestVersion = latestVersionString == null
            ? VersionInfo.Empty
            : VersionInfo.NewVersionInfoInstanceByString(latestVersionString);

        LatestDescription = description;
    }

    public static void DoUpdate()
    {
        using var client = new WebClient();
        client.DownloadFile(
            $"https://download.yzuu.cf/CognifyDev/ClashOfGods/releases/download/{LatestVersion}/ClashOfGods.dll",
            "BepInEx/plugins/ClashOfGods.dll.new"
        );

        File.WriteAllText("BepInEx/plugins/do.vbs",
            "WScript.Sleep 1000\n\nstrFileToDelete = \"ClashOfGods.dll\"\nstrFileToRename = \"ClashOfGods.dll.new\"\nstrScriptToDelete = WScript.ScriptFullName\n\nSet fs = CreateObject(\"Scripting.FileSystemObject\")\n\nIf fs.FileExists(strFileToDelete) Then\n    fs.DeleteFile strFileToDelete\nEnd If\n\nIf fs.FileExists(strFileToRename) Then\n    fs.MoveFile strFileToRename, strFileToDelete\nEnd If\n\nIf fs.FileExists(strScriptToDelete) Then\n    fs.DeleteFile strScriptToDelete\nEnd If");

        Process.Start("BepInEx/plugins/do.vbs");
    }
}