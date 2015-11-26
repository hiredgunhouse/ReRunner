using System.Configuration;
using System.IO;
using System.Web.Script.Serialization;

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

            var configuration = File.ReadAllText(configFile);
            var jss = new JavaScriptSerializer();

            return jss.Deserialize<TConfigurationModel>(configuration);
        }
    }
}