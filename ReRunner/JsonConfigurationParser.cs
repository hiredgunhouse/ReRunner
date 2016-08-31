using System.Configuration;
using System.IO;

using Newtonsoft.Json;

namespace ReRunner
{
    public static class JsonConfigurationParser 
    {
        public static TConfigurationModel LoadConfiguration<TConfigurationModel>(string configFile)
        {
            if (!File.Exists(configFile))
            {
                throw new ConfigurationErrorsException(string.Format("Configuration file: '{0}' does not exist!", configFile));
            }

            var js = new JsonSerializer();

            return js.Deserialize<TConfigurationModel>(new JsonTextReader(new StreamReader(configFile)));
        }
    }
}