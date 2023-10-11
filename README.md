# UpdaterLibrary
This library Develop with c#. 
You can use to update for Winform or Service windows.

# How it work?

    => Library will download you info verion 
    => Check version 
    => Download your file .zip 
    => Extract file .zip
    => Call you funtion to close your app
    => Replace all file extracted in your app folder
    => Run your commands after success
    => Open you application

# How to use

### 1. Zip application and upload to your host
ex link to download: http://youlink.com/setup.zip

### 2. Upload file info your version and provider link to download new version.
ex file app.xml
link to download file app.xml: http://youlink.com/version.xml

```
<?xml version="1.0" encoding="utf-16"?>
<LastestVersionInfo xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Version>1.0.0.1</Version>
  <LinkDownloadZipFile>http://youlink.com/setup.zip</LinkDownloadZipFile>
</LastestVersionInfo>
```

### 3. Check for update on you program

```
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
            var urlDownloadFileXml = "http://youlink.com/version.xml";
            var runProgramFile = Path.Combine(Directory.GetCurrentDirectory(), "YouApp.exe");

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
            if (hasNewVersion)
            {
                var messageError = new UpdateExecuter().RunUpdateAsync(param).Result;
                Console.WriteLine(messageError);
            }
            else
            {
                Console.WriteLine("You are lastest version.");
            }
            Console.ReadKey();
        }
    }
}

```

### Thanks you. Contact me: tiephoang.dev@gmail.com