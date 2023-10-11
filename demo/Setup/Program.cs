using System;
using System.IO;
using System.Reflection;
using UpdaterLibrary;

namespace Setup
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var urlDownloadFileXml = "https://raw.githubusercontent.com/TiepHoangDev/AutoAndroidReleaseApp/master/info/version.xml";
            var runProgramFile = Path.Combine(Directory.GetCurrentDirectory(), "AutoRegFbLite.exe");

            var param = UpdateParameter.CreateForCheckUpdate(
                urlGetInfoUpdate: urlDownloadFileXml,
                currentVersion: Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                runProgramFile: runProgramFile,
                exitApplication: () => Environment.Exit(0),
                onLog: Console.WriteLine,
                folderApplication: Directory.GetCurrentDirectory(),
                executeCmdWhenCopySuccessfuls: default,
                folderExtractedZip: default,
                pathFileZip: default
            );

            var lastestVersion = new UpdateExecuter().GetLatestVerionAsync(param).Result;
            var hasNewVersion = new UpdateExecuter().CheckForUpdateAsync(param, lastestVersion).Result;
            var updateSuccess = new UpdateExecuter().RunUpdateAsync(param).Result;
        }
    }
}
