using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpdaterLibrary.Extractor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("========================================================================");
                Console.WriteLine($"Wellcome to UpdaterLibrary.Extractor version {Assembly.GetExecutingAssembly().GetName().Version}");
                Console.WriteLine("========================================================================");
                Console.WriteLine(ArgumentBuilder.GetHelpText());
                Console.WriteLine("========================================================================");
                var argument = ArgumentBuilder.GetCommandLineArgs();
                LogToFile(argument.ToCommandArgument());
                new ExtractorRunner().Run(argument).GetAwaiter().GetResult();

                if (argument.PauseClose > 0)
                {
                    Console.WriteLine($"====================================Exit after {argument.PauseClose} seconds====================================");
                    Thread.Sleep(TimeSpan.FromSeconds(argument.PauseClose.Value));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                LogToFile(ex);
            }
        }

        public static void LogToFile(object msg)
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "ExtractorLog");
            if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd}.Extractor.log");
            var textMessage = $"\n{DateTime.Now:HH:mm:ss}>> {msg}";
            File.AppendAllText(file, textMessage);
            Console.WriteLine(file);
        }
    }
}
