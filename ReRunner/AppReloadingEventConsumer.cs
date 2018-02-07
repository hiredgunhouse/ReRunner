using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using log4net;

namespace ReRunner
{
    public class AppReloadingEventConsumer : IEventConsumer<FileSystemEventArgs>
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public AppReloadingEventConsumer(AppReloadingConfigurationModel configuration)
        {
            this.Configuration = configuration;
            this.RetryCounter = 0;
        }

        protected AppReloadingConfigurationModel Configuration { get; set; }
        protected Process Process { get; set; }
        
        private int RetryCounter { get; set; }
        private Exception LastException { get; set; }

        public void Consume(FileSystemEventArgs eventArgs)
        {
            try
            {
                this.KillTargetApp(this.Process);
                this.CleanupTarget();
                this.DeployToTarget();
                this.Process = this.RunTargetApp();
                
                this.RetryCounter = 0;
            }
            catch (Exception ex)
            {
                if (this.LastException?.GetType() == ex.GetType())
                {
                    this.RetryCounter++;
                    Thread.Sleep(this.Configuration.retryBackOffDelay);
                }
                else
                {
                    this.RetryCounter = 0;
                }
                
                Console.WriteLine(ex);

                if (this.RetryCounter >= this.Configuration.numberOfRetries)
                {
                    throw;
                }
            }
        }

        private void KillTargetApp(Process process)
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    log.Info("Killing the app...");
                    process.Kill();
                    process.WaitForExit();
                }
                else
                {
                    log.Info("Process has exited.");
                }
            }
            else
            {
                log.Info("Process was null.");
            }

            if (this.Configuration.killByProcessNameEnabled)
            {
                var processName = this.Configuration.processName;
                if (string.IsNullOrEmpty(processName))
                {
                    throw new ConfigurationErrorsException("processName cannot be empty when killByProcessNameEnabled is set to true");
                }

                var processToBeKilled = Process.GetProcessesByName(processName).FirstOrDefault();
                if (processToBeKilled != null)
                {
                    log.Info($"Killing process {processName}");
                    processToBeKilled.Kill();
                    processToBeKilled.WaitForExit();
                    log.Info($"Killed process {processName}");
                }
                else
                {
                    log.Warn($"Could not find process with name {processName}");
                }
            }
            
            Thread.Sleep(TimeSpan.FromMilliseconds(this.Configuration.afterAppKillDelay));
        }

        private void CleanupTarget()
        {
            if (!Directory.Exists(this.Configuration.targetDir))
            {
                log.Info("Target directory does not exist, creating it...");
                Directory.CreateDirectory(this.Configuration.targetDir);
            }

            log.Info("Cleaning up target dir...");
            FileSystemUtils.EmptyDirectory(this.Configuration.targetDir);
        }

        private void DeployToTarget()
        {
            log.Info("Deploying from source dir to target dir...");
            var sourceDir = FileSystemUtils.GetRootedPath(this.Configuration.sourceDir);
            var targetDir = FileSystemUtils.GetRootedPath(this.Configuration.targetDir);

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            FileSystemUtils.Copy(sourceDir, targetDir);
        }

        private Process RunTargetApp()
        {
            log.Info("Starting the app...");
            var appPath = Path.Combine(FileSystemUtils.GetRootedPath(this.Configuration.targetDir), this.Configuration.appName);

            return Process.Start(appPath, this.Configuration.appArguments);
        }
    }
}