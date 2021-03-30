using System;
using System.IO;
using System.Collections.Generic;

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
        private string _projectPath;
        
        private bool _helpRequested;
        private OptionSet _options;
        private ConfigurationRoot _config;

        // Files
        private List<string> _sourcePaths;
        private string[] _references;
        
        // Compiler Options
        private string _moduleName;
        private string _outputPath;

        private bool _isCompilingProject;
        private bool _isCreatingTemplate;

        public int RunVivianTools(string[] args)
        {
            ParseOptions(args);

            if (_isCompilingProject)
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
                {"c|compile=", "The {path} of the .vivconfig file", v =>
                {
                    _isCompilingProject = true;
                    _configPath = v;
                }},
                {"n|new=", "Creates a new Vivian project with a {name} and {path}", (name, path) => 
                {
                    _isCreatingTemplate = true;
                    _projectName = name;

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        _projectPath = name;
                    }
                    else
                    {
                        _projectPath = path;
                    }
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

            if (OperatingSystem.IsLinux())
            {
                CreateLinuxTemplate(projectName);
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
                Directory.CreateDirectory(@$"{projectName}/modules");
                Directory.CreateDirectory(@$"{projectName}/msbuild/config");
                Directory.CreateDirectory(@$"{projectName}/out");
                
                File.Copy(@"C:/Program Files/vivian/template/.vivconfig", $"{projectName}/.vivconfig");
                File.Copy(@"C:/Program Files/vivian/template/Main.viv", $"{projectName}/Program.viv");
            }
            
            try
            {
                // Copy files from the dotnet 3.1 reference folder to pull
                DirectoryCopy("C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Ref/3.1.0/ref/netcoreapp3.1", $"{projectName}/modules", false);
            }
            catch (DirectoryNotFoundException)
            {
                Console.Error.WriteLine("Unable to locate dependency modules. Please ensure you have the right version of dotnet SDK installed");
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
                
                File.Copy(@"/usr/shared/vivian/template/.vivconfig", $"{projectName}/.vivconfig");
                File.Copy(@"/usr/shared/vivian/template/Main.viv", $"{projectName}/Program.viv");
            }
            
            try
            {
                // Copy files from the dotnet 3.1 reference folder to pull
                DirectoryCopy("/usr/shared/dotnet/packs/Microsoft.NETCore.App.Ref/3.1.0/ref/netcoreapp3.1", $"{projectName}/modules", false);
            }
            catch (DirectoryNotFoundException)
            {
                Console.Error.WriteLine("Unable to locate dependency modules. Please ensure you have the right version of dotnet SDK installed");
            }
        }
        
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var directory = new DirectoryInfo(sourceDirName);

            if (!directory.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
            }

            var directories = directory.GetDirectories();
        
            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);        

            // Get the files in the directory and copy them to the new location.
            var files = directory.GetFiles();

            foreach (var file in files)
            {
                var tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (var subDir in directories)
                {
                    var tempPath = Path.Combine(destDirName, subDir.Name);
                    DirectoryCopy(subDir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}