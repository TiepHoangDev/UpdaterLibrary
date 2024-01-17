using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class ProgramHelper
{
    public static bool RunCmdGetSuccessExitCode(string cmd, string FilePath = "cmd.exe", string workingDirectory = null, Action<string> LogMessage = null)
        => RunCmdGetExitCode(cmd, FilePath, workingDirectory, LogMessage) == 0;

    public static int RunCmdGetExitCode(string cmd, string FilePath = "cmd.exe", string workingDirectory = null, Action<string> LogMessage = null)
    {
        RunCmd(cmd, out var exitCode, FilePath, workingDirectory, LogMessage);
        return exitCode;
    }

    public static string RunCmd(string cmd, string FilePath = "cmd.exe", string workingDirectory = null, Action<string> LogMessage = null) => RunCmd(cmd, out var _, FilePath, workingDirectory, LogMessage);

    public static string RunCmd(string cmd, out int ExitCode, string FilePath = "cmd.exe", string workingDirectory = null, Action<string> LogMessage = null)
    {
        var log = LogMessage ?? Console.WriteLine;
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory,

            FileName = FilePath,
            Arguments = cmd,
            Verb = "runas",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        using (var process = new Process())
        {
            process.StartInfo = startInfo;
            process.Start();

            //outputReader
            var output = new List<string>();
            var outputReader = new StreamReader(process.StandardOutput.BaseStream);
            while (true)
            {
                var line = outputReader.ReadLine();
                if (line == null) break;
                log?.Invoke(line);
                output.Add(line);
            }

            //WaitForExit
            process.WaitForExit();
            ExitCode = process.ExitCode;

            //errorReader
            if (ExitCode != 0)
            {
                var errorReader = new StreamReader(process.StandardError.BaseStream);
                var error = new List<string>();
                while (true)
                {
                    var line = errorReader.ReadLine();
                    if (line == null) break;
                    log?.Invoke(line);
                    error.Add(line);
                }
                output.AddRange(error);
            }

            var outputMessage = string.Join("\n", output);
            return outputMessage;
        }
    }
}
