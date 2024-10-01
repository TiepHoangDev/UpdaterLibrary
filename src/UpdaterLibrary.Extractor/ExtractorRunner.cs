using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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

                            Console.WriteLine($"Please kill process {argument.RunProgramFile.Trim()} to update program. Press enter if sure it not running!");
                            Console.ReadKey();
                        }
                    }
                }

                //BACKUP
                if (Directory.Exists(argument.FolderDistition))
                {
                    Console.WriteLine($"======================== BACKUP =====================");
                    var fileZip = $"{argument.FolderDistition}.backup.zip";
                    if (File.Exists(fileZip)) File.Delete(fileZip);
                    ZipFile.CreateFromDirectory(argument.FolderDistition, fileZip);
                    Console.WriteLine($">\t Backup successfully at {fileZip}.");
                }

                //COPY
                Console.WriteLine($"======================== COPY =====================");
                await CopyForder(argument.FolderSource, argument.FolderDistition);
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

        private async Task CopyForder(string folderSource, string folderDistition)
        {
            var files = Directory.GetFiles(folderSource);
            var folders = Directory.GetDirectories(folderSource);

            if (!Directory.Exists(folderDistition)) Directory.CreateDirectory(folderDistition);

            foreach (var item in files)
            {
                var fileName = Path.GetFileName(item);
                var toFile = Path.Combine(folderDistition, fileName);
                CopyFile(item, toFile);
            }

            foreach (var item in folders)
            {
                var folderName = Path.GetFileName(item);
                await CopyForder(Path.Combine(folderSource, folderName), Path.Combine(folderDistition, folderName));
            }
        }

        private void CopyFile(string fromFile, string toFile)
        {
            do
            {
                try
                {
                    File.Copy(fromFile, toFile, true);
                    Console.WriteLine($"[OK] {fromFile}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Copy {fromFile} -> {toFile}");
                    Console.WriteLine($"[Exception] {ex}");
                    Console.WriteLine($"Please delete file {toFile}");
                    Console.WriteLine($"\t-> Press [Enter] to continue");
                    Console.WriteLine($"\t-> Press [E] to Exit");
                    Console.WriteLine($"\t-> Press [S] to Skip");
                    var input = Console.ReadLine();
                    switch (input.Trim())
                    {
                        case "E":
                            Console.WriteLine($"\t\t-> Exit");
                            throw ex;
                        case "S":
                            Console.WriteLine($"\t\t-> Skip");
                            return;
                        case "":
                            Console.WriteLine($"\t\t-> Continue");
                            break;
                        default:
                            Console.WriteLine($"\t\t-> Unknow [{input}] -> Continue");
                            break;
                    }
                }
            } while (true);
        }
    }
}
