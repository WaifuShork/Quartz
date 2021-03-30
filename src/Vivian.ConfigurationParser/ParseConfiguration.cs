using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Vivian.ConfigurationParser
{
    public class ParseConfiguration
    {
        private static Configuration ParseConfig(string path)
        {
            // .vivconfig

            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"The file at {path} was not found");
                return null;
            }
            
            var configFile = File.ReadAllText(path);
            var configuration = JsonConvert.DeserializeObject<Configuration>(configFile);
            
            return configuration;
        }
    }

    public class Configuration
    {
        [JsonProperty("includes")]
        public List<string> Includes { get; set; }
        
        [JsonProperty("references")]
        public List<string> References { get; set; }
    }
}