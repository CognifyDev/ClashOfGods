using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace COG.Utils
{
    class AdvancedFileDownloader
    {
        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cancellationTokenSource;
        private long _totalBytes = 0;
        private long _downloadedBytes = 0;

        public event EventHandler<double> ProgressChanged;
        public event EventHandler<string> StatusChanged;

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
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "FileDownloader");
                Directory.CreateDirectory(tempDir);
                Directory.CreateDirectory(targetDirectory);

                string fileName = ExtractFileNameFromUrl(fileUrl) ??
                    $"downloaded_{DateTime.Now:yyyyMMdd_HHmmss}.tmp";

                string tempFilePath = Path.Combine(tempDir, fileName);
                string targetFilePath = Path.Combine(targetDirectory, fileName);

                if (File.Exists(targetFilePath))
                {
                    OnStatusChanged($"目标文件已存在，跳过下载: {fileName}");
                    return true;
                }

                targetFilePath = GetUniqueFilePath(targetFilePath);

                await DownloadFileWithProgressAsync(fileUrl, tempFilePath, progress, cancellationToken);

                OnStatusChanged($"文件下载完成: {fileName}");

                if (!await ValidateFileSizeAsync(fileUrl, tempFilePath))
                {
                    OnStatusChanged("警告: 下载的文件大小可能不完整");
                }

                MoveFile(tempFilePath, targetFilePath);
                OnStatusChanged($"文件已移动到: {targetFilePath}");

                try
                {
                    if (File.Exists(tempFilePath))
                        File.Delete(tempFilePath);
                }
                catch { }

                return true;
            }
            catch (System.Exception ex)
            {
                OnStatusChanged($"下载失败: {ex.StackTrace}");
                return false;
            }
        }

        private async Task DownloadFileWithProgressAsync(
            string url,
            string savePath,
            IProgress<double> progress,
            CancellationToken cancellationToken)
        {
            using (var response = await _httpClient.GetAsync(
                url,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken))
            {
                response.EnsureSuccessStatusCode();

                _totalBytes = response.Content.Headers.ContentLength ?? 0;
                _downloadedBytes = 0;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                {
                    var buffer = new byte[8192];
                    int bytesRead;
                    double lastProgress = 0;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                        _downloadedBytes += bytesRead;

                        if (_totalBytes > 0)
                        {
                            double currentProgress = (_downloadedBytes * 100.0) / _totalBytes;

                            if (currentProgress - lastProgress >= 1.0)
                            {
                                progress?.Report(currentProgress);
                                OnProgressChanged(currentProgress);
                                lastProgress = currentProgress;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取唯一文件名（避免冲突）
        /// </summary>
        private string GetUniqueFilePath(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;

            string directory = Path.GetDirectoryName(filePath);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            int counter = 1;

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
        private string ExtractFileNameFromUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                string fileName = Path.GetFileName(uri.LocalPath);

                if (string.IsNullOrEmpty(fileName))
                    return null;

                // 移除查询字符串
                if (fileName.Contains('?'))
                    fileName = fileName.Substring(0, fileName.IndexOf('?'));

                return fileName;
            }
            catch
            {
                return null;
            }
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

                if (response.Content.Headers.ContentLength.HasValue)
                {
                    var fileInfo = new FileInfo(filePath);
                    return fileInfo.Length == response.Content.Headers.ContentLength.Value;
                }
                return true;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// 移动文件
        /// </summary>
        private void MoveFile(string sourcePath, string destinationPath)
        {
            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            File.Move(sourcePath, destinationPath);
        }

        protected virtual void OnProgressChanged(double progress)
        {
            ProgressChanged?.Invoke(this, progress);
        }

        protected virtual void OnStatusChanged(string status)
        {
            StatusChanged?.Invoke(this, status);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}