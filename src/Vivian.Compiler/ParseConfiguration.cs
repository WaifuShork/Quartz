using System;
using System.IO;
using Newtonsoft.Json;

#nullable disable
namespace Vivian.ConfigurationParser
{
    public class ParseConfiguration
    {
        public static ConfigurationRoot ParseConfig(string path)
        {
            // .vivconfig
            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"The file at '{path}' was not found");
                return null;
            }

            var configFile = File.ReadAllText(path);
            var configuration = JsonConvert.DeserializeObject<ConfigurationRoot>(configFile);
            
            return configuration;
        }
    }
}