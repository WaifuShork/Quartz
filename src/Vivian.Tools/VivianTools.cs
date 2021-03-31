using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Mono.Options;

using Vivian.IO;
using Vivian.CodeAnalysis.Syntax;
using Vivian.Tools.Services;

#nullable disable
namespace Vivian.Tools
{
    public class VivianTools
    {
        private string _configPath;
        private string _projectName;
        
        private bool _helpRequested;
        private OptionSet _options;
        private ConfigurationRoot _config;

        // Files
        private List<string> _sourcePaths;
        private string[] _references;
        
        // Compiler Options
        private string _moduleName;
        private string _outputPath;

        private bool _isBuildingProject;
        private bool _isRunningProject;
        private bool _isCreatingTemplate;

        public int RunVivianTools(string[] args)
        {
            ParseOptions(args);

            if (_isBuildingProject)
            { 
                CompileProgram();
                return 0;
            }

            if (_isCreatingTemplate)
            {
                CreateProjectTemplate(_projectName);
                return 0;
            }
            
            return 0;
        }
        
        public void ParseOptions(string[] args)
        {
            // Options
            // --------------------------- //  
            _options = new OptionSet
            {
                "Usage: vivian [options]",
                "Usage: vivian [config-path]",
                {"b|build=", "The {path} of the .vivconfig file", v =>
                {
                    _isBuildingProject = true;
                    _configPath = v;
                }},
                {"r|run=", "The {path} of the .vivconfig file", v =>
                {
                    _isBuildingProject = true;
                    _isRunningProject = true;
                    _configPath = v;
                }},
                {"n|new=", "Creates a new Vivian project with a {name} and {path}", v => 
                {
                    _isCreatingTemplate = true;
                    _projectName = v;
                }},
                {"?|h|help", "Display help.", _ => _helpRequested = true}
            };
            
            // Attempt to parse the options
            // --------------------------- //  
            try
            {
                _options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Error.WriteLine($"Error parsing {e.OptionName}, please use `-help` for more info");
                return;
            }
            
            if (_helpRequested)
            {
                _options.WriteOptionDescriptions(Console.Out);
            }
        }

        private void ParseConfig()
        {
            // Build the configuration
            // --------------------------- //  
            _config = ParseConfiguration.ParseConfig(_configPath);
            
            if (_config == null)
            {
                Console.Error.WriteLine("Error: No configuration file was passed, or the path is incorrect");
                return;
            }
            
            // Take the current directory and locate a .vivconfig (if one exists)
            try
            {
                if (File.Exists(".vivconfig"))
                {
                    _configPath = ".vivconfig";
                }
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine("Error: Unable to locate any '.vivconfig' file to use, please explicitly state the path");
                return;
            }
            
            // Build the compilation module
            // --------------------------- //  
            // Files
            _references = _config.References.ToArray();
            _sourcePaths = _config.SourceFiles;

            // Compiler Options
            _moduleName = _config.CompilerOptions.ModuleName;
            _outputPath = _config.CompilerOptions.OutputPath;
            
            if (_sourcePaths.Count == 0)
            {
                Console.Error.WriteLine("Error: need at least one source file");
                return;
            }

            if (_outputPath == null)
            {
                _outputPath = Path.ChangeExtension(_sourcePaths[0], ".exe");
            }
            
            if (_moduleName == null)
            {
                _moduleName = Path.GetFileNameWithoutExtension(_outputPath);
            }
        }

        private void CompileProgram()
        {
            ParseConfig();
            var syntaxTrees = new List<SyntaxTree>();
            var hasErrors = false;
 
            foreach (var path in _sourcePaths)
            {
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"Error: file '{path}' doesn't exist");
                    hasErrors = true;
                    continue;
                }
 
                var syntaxTree = SyntaxTree.Load(path);
                syntaxTrees.Add(syntaxTree);
            }
 
            foreach (var path in _references)
            {
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"Error: file '{path}' doesn't exist");
                    hasErrors = true;
                }
            }

            if (hasErrors)
            {
                return;
            }
            
            var compilerHost = new ConsoleCompilerHost();
            var compilerService = new Server(compilerHost);
            compilerService.Initialize();
            compilerService.EmitBinary(syntaxTrees, _moduleName, _references, _outputPath);

            if (_isRunningProject)
            {
                // run project
                Console.Out.WriteLine("If you see this message then `vivian -run [project]` is not implemented yet");
            }
            
            compilerService.Shutdown();
            compilerService.Exit();

            Console.Out.WriteBuildSummary(true, compilerHost.Errors, compilerHost.Warnings);
        }

        // Copies all files from the PATH template, and creates directories to build a basic console app
        private void CreateProjectTemplate(string projectName)
        {
            if (OperatingSystem.IsWindows())
            {
                CreateWindowsTemplate(projectName);
            }
            else if (OperatingSystem.IsLinux())
            {
                CreateLinuxTemplate(projectName);
            }
            else if (OperatingSystem.IsMacOS())
            {
                throw new NotImplementedException("MacOS is currently unsupported. Apologies!");
            }
            else
            {
                throw new Exception("Operating System not supported");
            }
        }

        private void CreateWindowsTemplate(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                Console.Error.WriteLine("Error: New projects must specify a name");
                return;
            }

            if (!Directory.Exists(projectName))
            {
                try
                {
                    Directory.CreateDirectory(projectName);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.Error.WriteLine("Error: Insufficient permissions to create directory");
                    return;
                }
            }
            
            if (Directory.Exists(projectName))
            {
                // Build project structure
                //Directory.CreateDirectory(@$"{projectName}/modules");
                //Directory.CreateDirectory(@$"{projectName}/msbuild/config");
                //Directory.CreateDirectory(@$"{projectName}/out");

                if (!Directory.Exists(@"C:/Program Files/vivian"))
                {
                    Console.Error.WriteLine("Unable to locate vivian templates");
                    return;
                }
                
                DirectoryExtensions.DirectoryCopy(@"C:/Program Files/vivian/templates/console", $@"{projectName}", true);
                
                //File.Copy(@"C:/Program Files/vivian/template/.vivconfig", $"{projectName}/.vivconfig");
                //File.Copy(@"C:/Program Files/vivian/template/console/Main.viv", $"{projectName}/Program.viv");
            }
        }
        
        private void CreateLinuxTemplate(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                Console.Error.WriteLine("Error: New projects must specify a name");
                return;
            }

            if (!Directory.Exists(projectName))
            {
                try
                {
                    Directory.CreateDirectory(projectName);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.Error.WriteLine("Error: Insufficient permissions to create directory");
                    return;
                }
            }
            
            if (Directory.Exists(projectName))
            {
                // Build project structure
                Directory.CreateDirectory(@$"{projectName}/modules");
                Directory.CreateDirectory(@$"{projectName}/msbuild/config");
                Directory.CreateDirectory(@$"{projectName}/out");
                
                if (!Directory.Exists(@"/usr/shared/vivian"))
                {
                    Console.Error.WriteLine("Unable to locate vivian templates");
                    return;
                }
                
                File.Copy(@"/usr/shared/vivian/template/.vivconfig", $"{projectName}/.vivconfig");
                File.Copy(@"/usr/shared/vivian/template/Main.viv", $"{projectName}/Program.viv");
            }
            
            try
            {
                // Copy files from the dotnet 3.1 reference folder to pull
                DirectoryExtensions.DirectoryCopy("/usr/shared/dotnet/packs/Microsoft.NETCore.App.Ref/3.1.0/ref/netcoreapp3.1", $"{projectName}/modules", false);
            }
            catch (DirectoryNotFoundException)
            {
                Console.Error.WriteLine("Unable to locate dependency modules. Please ensure you have the right version of dotnet SDK installed");
            }
        }
        
        
    }
}