using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ArgumentBuilder
{
    /// <summary>
    /// Forder extraced file update
    /// </summary>
    public string FolderSource { get; set; }

    /// <summary>
    /// Keep Folder Source If Success
    /// </summary>
    public bool KeepFolderSourceIfSuccess { get; set; }

    /// <summary>
    /// Force Update
    /// </summary>
    public bool ForceUpdate { get; set; }

    /// <summary>
    /// Directory of application. allow null.
    /// </summary>
    public string FolderDistition { get; set; }

    /// <summary>
    /// After update, this file will execute by cmd. allow null
    /// </summary>
    public string RunProgramFile { get; set; }

    /// <summary>
    /// After update, the time delay console view. allow null
    /// </summary>
    public int? PauseClose { get; set; }

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
        stringBuilder.Append($" --keep \"{KeepFolderSourceIfSuccess}\" ");
        stringBuilder.Append($" --force \"{ForceUpdate}\" ");
        if (PauseClose > 0)
            stringBuilder.Append($" --pause \"{PauseClose}\" ");
        if (!string.IsNullOrWhiteSpace(ExecuteCmdWhenCopySuccessful))
            stringBuilder.Append($" --executecmd \"{ExecuteCmdWhenCopySuccessful}\" ");
        if (!string.IsNullOrWhiteSpace(RunProgramFile))
            stringBuilder.Append($" --runapp \" {RunProgramFile} \" ");

        return stringBuilder.ToString();
    }

    public static string GetHelpText()
    {
        var texts = new List<string>
        {
            $"Arguments for ExtractorRunner:",
            $"--from FolderSource* : forder include all files to copy",
            $"--to FolderDistition* : forder to replace all files",
            $"[--executecmd ExecuteCmdWhenCopySuccessful] : if provider. list command will run after copy success. split by | and replace ` => \" ",
            $"[--runapp RunProgramFile] : if provider. the file will open by cmd.exe ",
            $"[--keep false] : if provider. Keep Folder Source If Success ",
            $"[--force false] : if provider. Force update ",
            $"[--pause 0] : if provider. The view console will Pause Close on ms ",
            $"The end. Thanks for use my program.",
            $" tiephoang.dev@gmail.com"
        };
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
                        .Select(q => q.Replace("`", "\""))
                        .ToList();
                    break;
                case "--runapp":
                    argument.RunProgramFile = args[i + 1];
                    break;
                case "--pause":
                    argument.PauseClose = Convert.ToInt32(args[i + 1]);
                    break;
                case "--keep":
                    argument.KeepFolderSourceIfSuccess = Convert.ToBoolean(args[i + 1]);
                    break;
                case "--force":
                    argument.ForceUpdate = Convert.ToBoolean(args[i + 1]);
                    break;
                default:
                    break;
            }
        }
        return argument;
    }
}
