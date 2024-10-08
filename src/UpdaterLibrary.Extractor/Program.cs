﻿using System;
using System.IO;
using System.Reflection;
using System.Threading;

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
                Console.WriteLine($"Read log at file: {GetFileLog()}. Press any key to exit...");
                Console.ReadKey();
            }
        }



        public static void LogToFile(object msg)
        {
            string file = GetFileLog();
            var textMessage = $"\n{DateTime.Now:HH:mm:ss}>> {msg}";
            File.AppendAllText(file, textMessage);
            Console.WriteLine(file);
        }

        private static string GetFileLog()
        {
            var dir = Path.Combine(Directory.GetCurrentDirectory(), "ExtractorLog");
            if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, $"{DateTime.Now:yyyy-MM-dd}.Extractor.log");
            return Path.GetFullPath(file);
        }
    }
}
