using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdaterLibrary.Extractor
{
    public class ExtractorRunner
    {
        public async Task<bool> Run(ArgumentBuilder argument)
        {
            try
            {
                //PRINT CMD
                Console.WriteLine($"======================= PRINT CMD ======================");
                foreach (var item in argument.GetType().GetProperties())
                {
                    var value = item.GetValue(argument);
                    Console.WriteLine($"{item.Name} = {value}");
                }

                //WAIT PROGRAM EXIT
                if (!string.IsNullOrWhiteSpace(argument.RunProgramFile))
                {
                    Console.WriteLine($"======================= WAIT PROGRAM EXIT ======================");
                    var name = Path.GetFileNameWithoutExtension(argument.RunProgramFile);
                    while (true)
                    {
                        try
                        {
                            var process = Process.GetProcessesByName(name).FirstOrDefault(q => q.MainModule?.FileName?.Trim().Equals(argument.RunProgramFile.Trim(), StringComparison.CurrentCultureIgnoreCase) == true);
                            if (process == null) break;
                            var waitMs = 30;
                            while (!process.HasExited && waitMs > 0)
                            {
                                Console.Write($"\r>\t Wait {process.ProcessName} [Id={process.Id}] exit in {waitMs} seconds... ");
                                await Task.Delay(1000);
                                waitMs--;
                            }
                            Console.WriteLine();

                            if (process.HasExited)
                            {
                                Console.WriteLine($">\t {process.ProcessName} [Id={process.Id}] EXITED");
                            }
                            else
                            {
                                Console.WriteLine($">\t PROGRAM STILL RUNNING. KILL {process.ProcessName}");
                                process.Kill();
                                Console.WriteLine($">\t PROGRAM STILL RUNNING. FORCE KILL OK.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            Program.LogToFile(ex);
                        }
                    }
                }

                //COPY
                Console.WriteLine($"======================== COPY =====================");
                var cmd = $"\"{argument.FolderSource}\\\" \"{argument.FolderDistition}\\\" /s /e /y";
                var processCopy = Process.Start(new ProcessStartInfo
                {
                    FileName = "xcopy.exe",
                    Arguments = cmd,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });
                processCopy.WaitForExit();
                Console.WriteLine(">\t Copy successfully.");

                //RUN SUCCESS COMMAND
                foreach (var command in argument.ExecuteCmdWhenCopySuccessfuls)
                {
                    Console.WriteLine($"======================= RUN SUCCESS COMMAND ======================");
                    Console.WriteLine($">\t Execute command: {command}");
                    var myCommand = $"/C {command}";
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false,

                        FileName = "cmd.exe",
                        Arguments = myCommand,
                        Verb = "runas",
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    };
                    var process = Process.Start(startInfo);

                    string error = process.StandardError.ReadToEnd()?.Trim();
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();

                    Console.WriteLine($">\t Output: {output}");
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        Console.WriteLine($">\t Exception: {error}");
                        return false;
                    }
                    await Task.Delay(1000);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($">\t Exception: {ex}");
                Program.LogToFile(ex);
            }
            finally
            {
                //run app
                if (!string.IsNullOrWhiteSpace(argument.RunProgramFile))
                {
                    await Task.Delay(2000);
                    Console.WriteLine($"====================== RunProgramFile =======================");
                    if (File.Exists(argument.RunProgramFile))
                    {
                        Console.WriteLine($">\t Run RunProgramFile: {argument.RunProgramFile}");
                        var start = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/C \"{argument.RunProgramFile}\" ",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                        };
                        Process.Start(start);
                    }
                    else
                    {
                        Console.WriteLine($">\t Run RunProgramFile: Not found {argument.RunProgramFile}");
                    }
                }
            }

            return false;
        }
    }
}
