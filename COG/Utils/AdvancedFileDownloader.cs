using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IronPython.Runtime.Exceptions;

namespace COG.Utils;

internal sealed class AdvancedFileDownloader
{
    private readonly HttpClient _httpClient;
    private long _totalBytes;
    private long _downloadedBytes;

    public event EventHandler<double> ProgressChanged = (_, _) => { };

    public AdvancedFileDownloader()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(30);
    }

    /// <summary>
    /// 下载文件并移动到指定目录
    /// </summary>
    public async Task<bool> DownloadAndMoveAsync(
        string fileUrl,
        string targetDirectory,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "FileDownloader");
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(targetDirectory);

            var fileName = ExtractFileNameFromUrl(fileUrl) ??
                           $"downloaded_{DateTime.Now:yyyyMMdd_HHmmss}.tmp";

            var tempFilePath = Path.Combine(tempDir, fileName);
            var targetFilePath = Path.Combine(targetDirectory, fileName);

            if (File.Exists(targetFilePath))
            {
                return true;
            }

            targetFilePath = GetUniqueFilePath(targetFilePath);

            await DownloadFileWithProgressAsync(fileUrl, tempFilePath, progress, cancellationToken);
            
            if (!await ValidateFileSizeAsync(fileUrl, tempFilePath))
            {
            }

            MoveFile(tempFilePath, targetFilePath);

            try
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
            catch
            {
                // ignored
            }

            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    private async Task DownloadFileWithProgressAsync(
        string url,
        string savePath,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        _totalBytes = response.Content.Headers.ContentLength ?? 0;
        _downloadedBytes = 0;

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write);
        var buffer = new byte[8192];
        int bytesRead;
        double lastProgress = 0;

        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

            _downloadedBytes += bytesRead;

            if (_totalBytes <= 0) continue;
            var currentProgress = _downloadedBytes * 100.0 / _totalBytes;

            if (!(currentProgress - lastProgress >= 1.0)) continue;
            progress?.Report(currentProgress);
            OnProgressChanged(currentProgress);
            lastProgress = currentProgress;
        }
    }

    /// <summary>
    /// 获取唯一文件名（避免冲突）
    /// </summary>
    private string GetUniqueFilePath(string filePath)
    {
        if (!File.Exists(filePath))
            return filePath;

        var directory = Path.GetDirectoryName(filePath);
        if (directory == null)
            throw new RuntimeException("directory cannot be null!");
        
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
        var extension = Path.GetExtension(filePath);
        var counter = 1;

        string newFilePath;
        do
        {
            newFilePath = Path.Combine(directory,
                $"{fileNameWithoutExt} ({counter}){extension}");
            counter++;
        } while (File.Exists(newFilePath));

        return newFilePath;
    }

    /// <summary>
    /// 从URL提取文件名
    /// </summary>
    private static string? ExtractFileNameFromUrl(string url)
    {
        var uri = new Uri(url);
        var fileName = Path.GetFileName(uri.LocalPath);

        if (string.IsNullOrEmpty(fileName))
            return null;

        // 移除查询字符串
        if (fileName.Contains('?'))
            fileName = fileName[..fileName.IndexOf('?')];

        return fileName;
    }

    /// <summary>
    /// 验证文件大小
    /// </summary>
    private async Task<bool> ValidateFileSizeAsync(string url, string filePath)
    {
        try
        {
            var headRequest = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await _httpClient.SendAsync(headRequest);

            if (!response.Content.Headers.ContentLength.HasValue) return true;
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length == response.Content.Headers.ContentLength.Value;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// 移动文件
    /// </summary>
    private static void MoveFile(string sourcePath, string destinationPath)
    {
        if (File.Exists(destinationPath))
            File.Delete(destinationPath);

        File.Move(sourcePath, destinationPath);
    }

    private void OnProgressChanged(double progress)
    {
        ProgressChanged.Invoke(this, progress);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}