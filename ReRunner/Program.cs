using System;
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

            StartCommandLoop();
        }

        private static void StartCommandLoop()
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

                    // quit
                    case ConsoleKey.Q:
                        quit = true;
                        break;
                }
            } while (!quit);
        }
    }
}
