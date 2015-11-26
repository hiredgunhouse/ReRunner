namespace ReRunner
{
    public class AppReloadingConfigurationModel
    {
        public string sourceDir;
        public string targetDir;
        public string appName;
        public string appArguments;
        public int afterAppKillDelay; // miliseconds - need to make sure locked resources are freed
        public string[] ignores;
        public int eventThrottlingDelay; // miliseconds
    }
}