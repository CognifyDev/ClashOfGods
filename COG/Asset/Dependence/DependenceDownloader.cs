using System.Collections;
using System.Threading.Tasks;
using COG.Utils;

namespace COG.Asset.Dependence;

public static class DependenceDownloader
{
	public static IEnumerator DownloadCommonDependence()
	{
		yield return AdvancedExampleCoroutine("https://cog.amongusclub.cn/Dependence/Acornima.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
		yield return AdvancedExampleCoroutine("https://cog.amongusclub.cn/Dependence/System.Windows.Forms.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
	}
	public static IEnumerator DownloadPluginSystemDependence()
	{
		yield return AdvancedExampleCoroutine("https://cog.amongusclub.cn/Dependence/IronPython.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
		yield return AdvancedExampleCoroutine("https://cog.amongusclub.cn/Dependence/IronPython.Modules.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
		yield return AdvancedExampleCoroutine("https://cog.amongusclub.cn/Dependence/IronPython.SQLite.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
		yield return AdvancedExampleCoroutine("https://cog.amongusclub.cn/Dependence/IronPython.Wpf.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
		yield return AdvancedExampleCoroutine("https://cog.amongusclub.cn/Dependence/Microsoft.Dynamic.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
		yield return AdvancedExampleCoroutine("https://cog.amongusclub.cn/Dependence/Microsoft.Scripting.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
		yield return AdvancedExampleCoroutine("https://cog.amongusclub.cn/Dependence/System.CodeDom.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
	}
	public static IEnumerator DownloadYaml()
    {
        yield return AdvancedExampleCoroutine("https://cog.amongusclub.cn/Dependence/YamlDotNet.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
    }

    private static IEnumerator AdvancedExampleCoroutine(string targetFile, string targetPath)
    {
        var task = AdvancedExample(targetFile, targetPath);
        yield return WaitForTaskCompletion(task);
    }
    private static IEnumerator WaitForTaskCompletion(Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }
    }

    private static async Task AdvancedExample(string targetFile, string targetPath)
    {
        var downloader = new AdvancedFileDownloader();

        downloader.ProgressChanged += (_, progress) =>
        {
            Main.Logger.LogInfo($"Downloading Progress: {progress:F1}%");
        };

        var success = await downloader.DownloadAndMoveAsync(targetFile, targetPath);

        Main.Logger.LogInfo($"\nDownload {(success ? "Succeeded" : "Failed")}");
    }
}