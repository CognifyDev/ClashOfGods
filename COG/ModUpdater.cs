using COG.Utils;
using COG.Utils.Version;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using static COG.Main;

namespace COG
{
    public static class ModUpdater
    {
        public static VersionInfo VersionInfo { get; private set; } = null!;
        public static VersionInfo LatestVersion { get; private set; } = null!;
        public static string LatestDescription { get; set; } = "";
        public static bool BetaVersion { get; private set; }

        public static void FetchUpdate()
        {
            string latestVersionString = null!;
            string description = "";

            try
            {
                var jsonObject =
                    JObject.Parse(WebUtils.GetWeb("https://api.github.com/repos/CognifyDev/ClashOfGods/releases/latest"));
                var tagNameToken = jsonObject["tag_name"];
                var tagBodyToken = jsonObject["body"];

                if (tagNameToken is { Type: JTokenType.String }) latestVersionString = tagNameToken.ToString();
                if (tagBodyToken is { Type: JTokenType.String }) description = tagBodyToken.ToString();
            }
            catch
            {
                // ignored
            }

            LatestVersion = latestVersionString == null ? VersionInfo.Empty : VersionInfo.NewVersionInfoInstanceByString(latestVersionString);

            BetaVersion = PluginVersion.ToLower().Contains("beta") || PluginVersion.ToLower().Contains("dev");

            LatestDescription = description;
        }

#pragma warning disable SYSLIB0014
        public static void DownloadUpdate()
        {
            using var client = new WebClient();
            client.DownloadFile(
                $"https://download.yzuu.cf/CognifyDev/ClashOfGods/releases/download/{Main.LatestVersion}/ClashOfGods.dll",
                "BepInEx/plugins/ClashOfGods.dll.new"
            );

            File.WriteAllText("BepInEx/plugins/do.vbs", "WScript.Sleep 1000\n\nstrFileToDelete = \"ClashOfGods.dll\"\nstrFileToRename = \"ClashOfGods.dll.new\"\nstrScriptToDelete = WScript.ScriptFullName\n\nSet fs = CreateObject(\"Scripting.FileSystemObject\")\n\nIf fs.FileExists(strFileToDelete) Then\n    fs.DeleteFile strFileToDelete\nEnd If\n\nIf fs.FileExists(strFileToRename) Then\n    fs.MoveFile strFileToRename, strFileToDelete\nEnd If\n\nIf fs.FileExists(strScriptToDelete) Then\n    fs.DeleteFile strScriptToDelete\nEnd If");

            Process.Start("BepInEx/plugins/do.vbs");
        }
    }
}
