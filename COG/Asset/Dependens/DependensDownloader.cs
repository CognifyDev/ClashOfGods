using System;
using System.Collections;
using System.Threading.Tasks;
using COG.Utils;

namespace COG.Asset.Dependens
{
    public class DependensDownloader
    {
        public static IEnumerator DownloadCommonDependens()
        {
            yield return AdvancedExampleCoroutine("https://xtreme.net.cn/upload/Acornima.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
            yield return AdvancedExampleCoroutine("https://xtreme.net.cn/upload/System.Windows.Forms.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
        }
        public static IEnumerator DownloadYaml()
        {
            yield return AdvancedExampleCoroutine("https://xtreme.net.cn/upload/YamlDotNet.dll", @$"{PathUtils.GetAmongUsPath()}\BepInEx\core");
        }
        static IEnumerator AdvancedExampleCoroutine(string targetFile, string targetPath)
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

        static async Task AdvancedExample(string targetFile, string targetPath)
        {
            var downloader = new AdvancedFileDownloader();

            downloader.ProgressChanged += (sender, progress) =>
            {
                Main.Logger.LogInfo($"下载进度: {progress:F1}%  ");
            };

            var fileUrl = targetFile;
            var targetDir = targetPath;

            var success = await downloader.DownloadAndMoveAsync(fileUrl, targetDir);

            Main.Logger.LogInfo($"\n下载结果: {(success ? "成功" : "失败")}");
        }
    }
}