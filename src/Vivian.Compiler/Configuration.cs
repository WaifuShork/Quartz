using System.Collections.Generic;
using Newtonsoft.Json;

#nullable disable
namespace Vivian.ConfigurationParser
{
    [JsonObjectAttribute]
    public class CompilerOptions
    {
        [JsonProperty("moduleName")]
        public string ModuleName { get; set; }
        
        [JsonProperty("output")]
        public string OutputPath { get; set; }
            
        [JsonProperty("compilerPath")]
        public string CompilerPath { get; set; }
        
        [JsonProperty("useMSBuild")]
        public bool UseMSBuild { get; set; }
    }
    
    public class ConfigurationRoot
    {
        [JsonProperty("compilerOptions")]
        public CompilerOptions CompilerOptions { get; set; }
	    
        [JsonProperty("files")]
        public List<string> SourceFiles { get; set; }
        
        [JsonProperty("references")]
        public List<string> References { get; set; }
    }
}