using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                var cmd = $"\"{argument.FolderSource}\\\" \"{argument.FolderDistition}\\\" /e /y /h /c /i /r";
                ProgramHelper.RunCmd(cmd, "xcopy.exe");
                Console.WriteLine(">\t Copy successfully.");

                //RUN SUCCESS COMMAND
                foreach (var command in argument.ExecuteCmdWhenCopySuccessfuls)
                {
                    Console.WriteLine($"======================= RUN SUCCESS COMMAND ======================");
                    Console.WriteLine($">\t Execute command: {command}");
                    var output = ProgramHelper.RunCmd($"/C {command}", "cmd.exe", LogMessage: Console.WriteLine);
                    await Task.Delay(1000);
                }

                if (!argument.KeepFolderSourceIfSuccess)
                {
                    Console.WriteLine($">\t Delete folder source after successful.");
                    Directory.Delete(argument.FolderSource, true);
                }

                return true;
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
                        Process.Start(argument.RunProgramFile.Trim());
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
