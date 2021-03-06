﻿using System;
using System.IO;

using NConsoler;

namespace ReRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Consolery.Run(typeof(Program), args);
        }

        [Action]
        public static void DoWork(
            [Optional("config.json")] string configFile)
        {
            var configuration = JsonConfigurationParser.LoadConfiguration<AppReloadingConfigurationModel>(configFile);

            var host = new Host<FileSystemEventArgs>(
                new FileSystemWatcherEventProducer(configuration),
                new AppReloadingEventConsumer(configuration));


            host.Run();

            StartCommandLoop(configuration);
        }

        private static void StartCommandLoop(AppReloadingConfigurationModel configuration)
        {
            var quit = false;
            do
            {
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    // clear
                    case ConsoleKey.C:
                        Console.Clear();
                        break;

                    // rerun
                    case ConsoleKey.R:
                        TriggerReRun(configuration);
                        
                        break;

                    // quit
                    case ConsoleKey.Q:
                        quit = true;
                        break;
                }
            } while (!quit);
        }

        private static void TriggerReRun(AppReloadingConfigurationModel configuration)
        {
            using (
                File.Create(
                    Path.Combine(
                        configuration.sourceDir,
                        $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}.tmp")))
            {
                Console.WriteLine("Rerunning app...");
            }
        }
    }
}
