#nullable disable
using System;
using System.IO;
using Newtonsoft.Json;
using Vivian.IO;

namespace Vivian.Tools
{
    public static class ParseConfiguration
    {
        public static ConfigurationRoot ParseConfig(string path)
        {
            // .vivconfig
            if (!File.Exists(path))
            {
                Console.Error.WriteError($"The file at '{path}' was not found.");
                return null;
            }

            var configFile = File.ReadAllText(path);

            try
            {
                var configuration = JsonConvert.DeserializeObject<ConfigurationRoot>(configFile);
                return configuration;
            }
            catch (JsonException)
            {
                Console.Error.WriteError("Something went wrong when attempted to parse your configuration file. ");
                throw;
            }
        }
    }
}