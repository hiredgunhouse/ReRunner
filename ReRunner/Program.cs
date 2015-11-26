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
            var done = false;

            while (!done)
            {
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.C:      // clean
                        Console.Clear();
                        break;

                    case ConsoleKey.Q:      // close
                        ////case ConsoleKey.Enter:  // close
                        Console.WriteLine("quiting...");
                        done = true;
                        break;
                }
            }
        }
    }
}
