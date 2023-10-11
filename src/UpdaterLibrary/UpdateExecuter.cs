using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UpdaterLibrary.Extractor;
using System.Xml.Serialization;
using System.Xml;

namespace UpdaterLibrary
{
    public class UpdateExecuter : IUpdateExecuter
    {
        public async Task<string> CheckForUpdate(UpdateParameter updateParameter)
        {
            var assemblyCaller = Assembly.GetCallingAssembly();
            if (string.IsNullOrWhiteSpace(updateParameter.ArgumentBuilder.FolderDistition))
            {
                updateParameter.ArgumentBuilder.FolderDistition = assemblyCaller.Location;
            }
            if (string.IsNullOrWhiteSpace(updateParameter.PathFileZip))
            {
                updateParameter.PathFileZip = Path.Combine(
                    Path.GetTempPath(),
                    $"updateFor_{updateParameter.CurrentVersion}_{Guid.NewGuid()}.zip");
            }

            updateParameter.OnLog?.Invoke($"UrlGetInfoUpdate={updateParameter.UrlGetInfoUpdate}");
            updateParameter.OnLog?.Invoke($"CurrentVersion={updateParameter.CurrentVersion}");
            updateParameter.OnLog?.Invoke($"PathFolderApplication={updateParameter.ArgumentBuilder.FolderDistition}");
            updateParameter.OnLog?.Invoke($"PathToSaveFile={updateParameter.PathFileZip}");

            using (var httpClient = new HttpClient())
            {
                var textInfo = await httpClient.GetStringAsync(updateParameter.UrlGetInfoUpdate);
                var lastestInfo = GetInfoUpdate<LastestVersionInfo>(textInfo);
                var isAlwaysUpdate = lastestInfo.Version?.Trim() == "*" || string.IsNullOrWhiteSpace(updateParameter.CurrentVersion);
                if (isAlwaysUpdate)
                {
                    updateParameter.OnLog?.Invoke($"Always update application.");
                }
                else
                {
                    var lastestVersion = new Version(lastestInfo.Version);
                    var currentVersion = new Version(updateParameter.CurrentVersion);
                    if (lastestVersion < currentVersion)
                    {
                        var err = $"What wrong with your version ?({currentVersion}?. Lastest version = {lastestVersion}.";
                        updateParameter.OnLog?.Invoke(err);
                        return err;
                    }
                    if (lastestVersion == currentVersion)
                    {
                        var err = $"You version is lastest.";
                        updateParameter.OnLog?.Invoke(err);
                        return err;
                    }
                    updateParameter.OnLog?.Invoke($"Have new version. Need to upgratefrom {currentVersion} -> {lastestVersion}.");
                }

                if (await DownloadFile(updateParameter, lastestInfo))
                {
                    updateParameter.OnLog($"Downloaded Successfully");
                    if (ExtractFileUpdate(updateParameter, lastestInfo))
                    {
                        if (CallReplaceFileApplication(updateParameter, out var processReplaceAndRun))
                        {
                            updateParameter.ExitApplication?.Invoke();
                            return null;
                        }
                        return $"Can't copy file update to folder service!";
                    };
                    return $"Can't extract file update!";
                }
                return $"Can't download file update!";
            }
        }

        private T GetInfoUpdate<T>(string textInfo)
        {
            var xmlSerializer = new XmlSerializer(typeof(T), new XmlRootAttribute("root"));
            var settings = new XmlWriterSettings()
            {
                //format the xml string
                Indent = true,
                //skip declare xml
                OmitXmlDeclaration = true,
            };
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            StringReader stringReader = new StringReader(textInfo);
            T xmlObject = (T)xmlSerializer.Deserialize(stringReader);
            return xmlObject;
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
            processReplaceAndRun = Process.Start(new ProcessStartInfo
            {
                FileName = pathFileExtractor,
                Arguments = argumentString,
                Verb = "runas",
            });

            return true;
        }

        private bool ExtractFileUpdate(UpdateParameter updateParameter, LastestVersionInfo lastestInfo)
        {
            var fileZip = updateParameter.PathFileZip;
            if (!File.Exists(fileZip)) return false;

            var folderExtractZip = Path.Combine(
                    Path.GetDirectoryName(fileZip),
                    "folderExtractZip"
                );
            updateParameter.ArgumentBuilder.FolderSource = folderExtractZip;

            using (var archive = ZipFile.OpenRead(fileZip))
            {
                var entries = archive.Entries;
                for (int i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    var filePath = Path.Combine(folderExtractZip, entry.FullName);
                    var folder = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    if (IsDirectory(entry))
                    {
                        if (!Directory.Exists(filePath))
                        {
                            Directory.CreateDirectory(filePath);
                        }
                    }
                    else
                    {
                        if (File.Exists(filePath))
                        {
                            updateParameter.OnLog($"Delete file {filePath}");
                            File.Delete(filePath);
                        }

                        using (Stream destination = File.Open(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            using (Stream stream = entry.Open())
                            {
                                stream.CopyTo(destination);
                                destination.SetLength(destination.Position);
                            }
                        }
                    }

                    updateParameter.OnLog($"{i + 1}. Extracted {entry.FullName} {i + 1}/{entries.Count}");
                }
                return true;
            }
        }

        public static bool IsDirectory(ZipArchiveEntry entry)
        {
            return string.IsNullOrEmpty(entry.Name) && (entry.FullName.EndsWith("/") || entry.FullName.EndsWith(@"\"));
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
