using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;

using log4net;

namespace ReRunner
{
    public class FileSystemWatcherEventProducer : IEventProducer<FileSystemEventArgs>
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public FileSystemWatcherEventProducer(AppReloadingConfigurationModel configuration)
        {
            this.Configuration = configuration;
            this.Rs = new ReplaySubject<FileSystemEventArgs>();
        }

        protected AppReloadingConfigurationModel Configuration { get; set; }
        protected ReplaySubject<FileSystemEventArgs> Rs { get; set; }

        public void Start(IEventConsumer<FileSystemEventArgs> eventConsumer)
        {
            log.Info("Starting event provider...");

            log.InfoFormat("Source: '{0}'", this.Configuration.sourceDir);
            log.InfoFormat("Target: '{0}'", this.Configuration.targetDir);
            log.InfoFormat("Command: {0} {1}", this.Configuration.appName, this.Configuration.appArguments);
            log.InfoFormat("Ignoring: {0}", this.Configuration.ignores != null ? "\n" + string.Join("\n", this.Configuration.ignores) : string.Empty);
            log.InfoFormat("After app kill delay: {0} miliseconds", this.Configuration.afterAppKillDelay);
            log.InfoFormat("Event throttling delay: {0} miliseconds", this.Configuration.eventThrottlingDelay);

            // wait till source dir exists (just for user conveniance)
            while (!Directory.Exists(this.Configuration.sourceDir))
            {
                log.InfoFormat("Source dir '{0}' does not exist, waiting till it does...", this.Configuration.sourceDir);
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }

            log.InfoFormat("Source dir '{0}' detected, running...", this.Configuration.sourceDir);

            var fsw = new FileSystemWatcher(this.Configuration.sourceDir);
            fsw.Changed += (sender, args) => this.DoEvent(args);
            fsw.Created += (sender, args) => this.DoEvent(args);
            fsw.Deleted += (sender, args) => this.DoEvent(args);
            fsw.Renamed += (sender, args) => this.DoEvent(args);

            this.Rs
                .Where(e => !this.ShouldBeIgnored(e, this.Configuration.ignores, this.Configuration.sourceDir))
                .Throttle(TimeSpan.FromMilliseconds(this.Configuration.eventThrottlingDelay))
                .Subscribe(eventArgs => eventConsumer.Consume(eventArgs));

            // do an initial start
            eventConsumer.Consume(null);

            log.InfoFormat("Watching source for changes...");
            fsw.EnableRaisingEvents = true;

            // no need for that, the host or it's controller should ensure that we are not ending the process
            ////while (true)
            ////{
            ////    Thread.Sleep(TimeSpan.FromSeconds(1));
            ////}
        }

        protected bool ShouldBeIgnored(FileSystemEventArgs e, IEnumerable<string> ignores, string inputDir)
        {
            var path = e.FullPath.Replace(inputDir, string.Empty);
            var shouldBeIgnored = ignores.Any(path.Contains);
            if (shouldBeIgnored)
            {
                log.DebugFormat("Ignoring '{0}'", path);
            }

            return shouldBeIgnored;
        }

        protected void DoEvent(FileSystemEventArgs args)
        {
            log.DebugFormat("{0} {1}", args.ChangeType.ToString().ToUpper(), args.FullPath); 
            this.Rs.OnNext(args);
        }
    }
}