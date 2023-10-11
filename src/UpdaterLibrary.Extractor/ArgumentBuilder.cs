using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdaterLibrary.Extractor
{
    public class ArgumentBuilder
    {
        /// <summary>
        /// Forder extraced file update
        /// </summary>
        public string FolderSource { get; set; }

        /// <summary>
        /// Directory of application. allow null.
        /// </summary>
        public string FolderDistition { get; set; }

        /// <summary>
        /// After update, this file will execute by cmd. allow null
        /// </summary>
        public string RunProgramFile { get; set; }

        /// <summary>
        /// list command will run after update. split by "|" and replace ` => "
        /// </summary>
        public List<string> ExecuteCmdWhenCopySuccessfuls { get; set; } = new List<string>();

        public string ExecuteCmdWhenCopySuccessful => string.Join("|", ExecuteCmdWhenCopySuccessfuls.Select(q => q.Replace("\"", "`")));

        public string ToCommandArgument()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($" --from \"{FolderSource}\" ");
            stringBuilder.Append($" --to \"{FolderDistition}\" ");
            if (!string.IsNullOrWhiteSpace(ExecuteCmdWhenCopySuccessful))
                stringBuilder.Append($" --executecmd \"{ExecuteCmdWhenCopySuccessful}\" ");
            if (!string.IsNullOrWhiteSpace(RunProgramFile))
                stringBuilder.Append($" --runapp \" {RunProgramFile} \" ");
            return stringBuilder.ToString();
        }

        public static string GetHelpText()
        {
            var texts = new List<string>();
            texts.Add($"Arguments for ExtractorRunner:");
            texts.Add($"--from FolderSource* : forder include all files to copy");
            texts.Add($"--to FolderDistition* : forder to replace all files");
            texts.Add($"[--executecmd ExecuteCmdWhenCopySuccessful] : if provider. list command will run after copy success. split by | and replace ` => \" ");
            texts.Add($"[--runapp RunProgramFile] : if provider. the file will open by cmd.exe ");
            texts.Add($"The end. Thanks for use my program.");
            texts.Add($" tiephoang.dev@gmail.com");
            var help = string.Join("\n", texts);
            return help;
        }

        public static ArgumentBuilder GetCommandLineArgs()
        {
            var argument = new ArgumentBuilder();
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                switch (arg)
                {
                    case "--from":
                        argument.FolderSource = args[i + 1];
                        break;
                    case "--to":
                        argument.FolderDistition = args[i + 1];
                        break;
                    case "--executecmd":
                        var runSuccessCommand = args[i + 1];
                        argument.ExecuteCmdWhenCopySuccessfuls = runSuccessCommand.Split('|')
                            .Select(q => q.Replace("`","\""))
                            .ToList();
                        break;
                    case "--runapp":
                        argument.RunProgramFile = args[i + 1];
                        break;
                    default:
                        break;
                }
            }
            return argument;
        }
    }
}
