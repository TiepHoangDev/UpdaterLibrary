using System;
using System.Collections.Generic;

namespace UpdaterLibrary
{

    /// <summary>
    /// Param for check update. <see cref="CreateForCheckUpdate"/>
    /// </summary>
    public class UpdateParameter
    {
        /// <summary>
        /// Url get json info update.
        /// if Version = "*" => always update.
        /// <code>{ Version : "", LinkDownloadZipFile : "" }</code>
        /// </summary>
        public string UrlGetInfoUpdate { get; set; }

        /// <summary>
        /// Current version of application. Only can be null when info update == "*";
        /// <see cref="LastestVersionInfo"/>
        /// <code>Assembly.GetExecutingAssembly().GetName().Version.ToString()</code>
        /// </summary>
        public string CurrentVersion { get; set; }

        /// <summary>
        /// Action write log update. allow null
        /// </summary>
        public Action<string> OnLog { get; set; }

        /// <summary>
        /// Action exit app when update ready to run. allow null.
        /// </summary>
        public Action ExitApplication { get; set; }

        /// <summary>
        /// Path file name .zip to save after download update. allow null.
        /// </summary>
        public string PathFileZip { get; set; }

        /// <summary>
        /// ArgumentBuilder parameter to pass to Extrator.
        /// </summary>
        public ArgumentBuilder ArgumentBuilder { get; set; } = new ArgumentBuilder();

        public static UpdateParameter CreateForCheckUpdate(string urlGetInfoUpdate,
            string currentVersion,
            string runProgramFile = default,
            Action exitApplication = default,
            Action<string> onLog = default,
            string folderApplication = default,
            List<string> executeCmdWhenCopySuccessfuls = default,
            string folderExtractedZip = default,
            string pathFileZip = default)
        {
            return new UpdateParameter
            {
                ArgumentBuilder = new ArgumentBuilder
                {
                    RunProgramFile = runProgramFile,
                    ExecuteCmdWhenCopySuccessfuls = executeCmdWhenCopySuccessfuls ?? new List<string>(),
                    FolderDistition = folderApplication,
                    FolderSource = folderExtractedZip,
                },
                CurrentVersion = currentVersion,
                ExitApplication = exitApplication,
                OnLog = onLog,
                PathFileZip = pathFileZip,
                UrlGetInfoUpdate = urlGetInfoUpdate,
            };
        }

    }
}
