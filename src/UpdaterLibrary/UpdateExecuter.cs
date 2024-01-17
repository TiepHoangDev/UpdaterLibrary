using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace UpdaterLibrary
{
    /// <summary>
    /// Update Executer
    /// </summary>
    public class UpdateExecuter : IUpdateExecuter
    {
        /// <summary>
        /// Check For Update Async. Return error message.
        /// </summary>
        /// <param name="updateParameter"></param>
        /// <returns></returns>
        public async Task<string> RunUpdateAsync(UpdateParameter updateParameter)
        {
            var assemblyMain = Assembly.GetEntryAssembly();
            if (string.IsNullOrWhiteSpace(updateParameter.ArgumentBuilder.FolderDistition))
            {
                updateParameter.ArgumentBuilder.FolderDistition = Path.GetDirectoryName(assemblyMain.Location);
                var dir = Path.GetDirectoryName(updateParameter.ArgumentBuilder.FolderDistition);
                Directory.CreateDirectory(dir);
            }
            if (string.IsNullOrWhiteSpace(updateParameter.PathFileZip))
            {
                var dir = Path.GetDirectoryName(updateParameter.ArgumentBuilder.FolderDistition);
                var filename = $"updateFor_{updateParameter.CurrentVersion}_{Guid.NewGuid()}.zip";
                updateParameter.PathFileZip = Path.Combine(dir, filename);
                dir = Path.GetDirectoryName(updateParameter.PathFileZip);
                Directory.CreateDirectory(dir);
            }

#if DEBUG
            updateParameter.OnLog?.Invoke($"UrlGetInfoUpdate={updateParameter.UrlGetInfoUpdate}");
#endif
            updateParameter.OnLog?.Invoke($"CurrentVersion={updateParameter.CurrentVersion}");
            updateParameter.OnLog?.Invoke($"PathFolderApplication={updateParameter.ArgumentBuilder.FolderDistition}");
            updateParameter.OnLog?.Invoke($"PathToSaveFile={updateParameter.PathFileZip}");

            var lastestInfo = await GetLatestVerionAsync(updateParameter);
            var hasNewVersion = await CheckForUpdateAsync(updateParameter, lastestInfo);
            if (!hasNewVersion) return "latest";

            if (await DownloadFile(updateParameter, lastestInfo))
            {
                updateParameter.OnLog?.Invoke($"Downloaded Successfully");
                if (ExtractFileUpdate(updateParameter, lastestInfo))
                {
                    if (CallReplaceFileApplication(updateParameter, out var processReplaceAndRun))
                    {
                        updateParameter.ExitApplication?.Invoke();
                        return null;
                    }
                    return $"Can't Call Replace File Application";
                };
                return $"Can't extract file update!";
            }
            return $"Can't download file update!";
        }

        public async Task<bool> CheckForUpdateAsync(UpdateParameter updateParameter, LastestVersionInfo lastestVersionInfo = null)
        {
            var lastestInfo = lastestVersionInfo ?? await GetLatestVerionAsync(updateParameter);
            var isAlwaysUpdate = lastestInfo.Version?.Trim() == "*" || string.IsNullOrWhiteSpace(updateParameter.CurrentVersion);
            if (isAlwaysUpdate)
            {
                updateParameter.OnLog?.Invoke($"Always update application.");
                return true;
            }

            var lastestVersion = new Version(lastestInfo.Version);
            var currentVersion = new Version(updateParameter.CurrentVersion);
            if (lastestVersion < currentVersion)
            {
                var err = $"Current version is higher than latest version. Current Version={currentVersion}?. Lastest version = {lastestVersion}.";
                updateParameter.OnLog?.Invoke(err);
                return false;
            }

            if (lastestVersion == currentVersion)
            {
                var err = $"You version is lastest.";
                updateParameter.OnLog?.Invoke(err);
                return false;
            }
            updateParameter.OnLog?.Invoke($"Have new version. Need to upgrate from {currentVersion} -> {lastestVersion}.");
            return true;
        }

        public async Task<LastestVersionInfo> GetLatestVerionAsync(UpdateParameter updateParameter)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                var url = $"{updateParameter.UrlGetInfoUpdate}?nocahe=true";
                var textInfo = await httpClient.GetStringAsync(url);
                return LastestVersionInfo.LoadFromXml(textInfo);
            }
        }

        private bool CallReplaceFileApplication(UpdateParameter updateParameter, out Process processReplaceAndRun)
        {
            processReplaceAndRun = null;

            var folderExtractor = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var nameExtractor = "UpdaterLibrary.Extractor.exe";
            var resourceExtractor = $"UpdaterLibrary.Runner.{nameExtractor}";
            var pathFileExtractor = Path.Combine(folderExtractor, nameExtractor);

            if (File.Exists(pathFileExtractor)) File.Delete(pathFileExtractor);

            if (!File.Exists(pathFileExtractor))
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceExtractor))
                {
                    // Ghi file exe ra đĩa cứng
                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    File.WriteAllBytes(pathFileExtractor, bytes);
                }
            }

            if (!File.Exists(pathFileExtractor)) return false;

            var argumentString = updateParameter.ArgumentBuilder.ToCommandArgument();
            Process.Start(pathFileExtractor, argumentString);
            return true;
        }

        private bool ExtractFileUpdate(UpdateParameter updateParameter, LastestVersionInfo lastestInfo)
        {
            //check file
            var fileZip = updateParameter.PathFileZip;
            if (!File.Exists(fileZip)) return false;

            //empty folder
            var dir = Path.GetDirectoryName(fileZip);
            var folderExtractZip = Path.Combine(dir, "folderExtractZip");
            updateParameter.ArgumentBuilder.FolderSource = folderExtractZip;
            if (Directory.Exists(folderExtractZip)) Directory.Delete(folderExtractZip, true);
            Directory.CreateDirectory(folderExtractZip);

            //extract zip
            using (var zip = ZipFile.OpenRead(fileZip))
            {
                var count = zip.Entries.Count;
                var index = 0F;
                foreach (var item in zip.Entries)
                {
                    index++;
                    var path = Path.Combine(folderExtractZip, item.FullName);
                    var isDirectory = item.FullName.EndsWith("/") || item.FullName.EndsWith("\\");
                    var directory = isDirectory ? path : Path.GetDirectoryName(path);
                    Directory.CreateDirectory(directory);
                    if (isDirectory)
                    {
                        if (File.Exists(path)) File.Delete(path);
                        item.ExtractToFile(path);
                    }
                    var percent = Math.Round(index * 100 / count);
                    updateParameter.OnLog?.Invoke($"[{percent}%] {path}");
                }
            }

            //delete file zip
            if (File.Exists(fileZip)) File.Delete(fileZip);

            return true;
        }

        private async Task<bool> DownloadFile(UpdateParameter updateParameter, LastestVersionInfo lastestVersionInfo)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(lastestVersionInfo.LinkDownloadZipFile, HttpCompletionOption.ResponseHeadersRead);
                if (response?.IsSuccessStatusCode != true)
                {
                    updateParameter.OnLog($"Download GET from {lastestVersionInfo.LinkDownloadZipFile} is {response?.StatusCode}: {response?.ReasonPhrase}");
                    response?.Dispose();
                    return false;
                }

                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength;
                var readBytes = 0L;

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                {
                    using (var fileStream = new FileStream(updateParameter.PathFileZip,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        8192,
                        true))
                    {
                        var buffer = new byte[8192];
                        double? lastPercent = 0;

                        while (true)
                        {
                            var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;

                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            readBytes += bytesRead;
                            var percent = readBytes * 100D / totalBytes;
                            if (percent - lastPercent >= 1)
                            {
                                updateParameter.OnLog($"Downloaded {readBytes / 1024}Kb ({percent:F2}%)");
                                lastPercent = percent;
                            }
                        }
                        return true;
                    }
                }
            }
        }
    }
}
