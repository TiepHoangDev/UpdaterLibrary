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
link to download file app.xml: http://youlink.com/app.xml

```
<root>
	<Version>1.0.0.2</Version>
	<LinkDownloadZipFile>http://youlink.com/setup.zip</LinkDownloadZipFile>
</root>
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
            var urlDownloadFileXml = "http://youlink.com/app.xml";
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
            new UpdateExecuter().CheckForUpdateAsync(param).GetAwaiter().GetResult();
        }
    }
}

```

### Thanks you. Contact me: tiephoang.dev@gmail.com