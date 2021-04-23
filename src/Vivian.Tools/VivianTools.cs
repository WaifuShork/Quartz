#nullable disable

using System;
using System.IO;
using System.Security.Principal;
using System.Collections.Generic;

using Mono.Options;

using Vivian.IO;
using Vivian.CodeAnalysis.Syntax;
using Vivian.Tools.Services;

namespace Vivian.Tools
{
    public static class VivianTools
    {
        private static string _configPath;
        private static string _projectName;
        
        private static OptionSet _options;
        private static ConfigurationRoot _config;

        // Files
        private static List<string> _sourcePaths;
        private static string[] _references;
        
        // Compiler Options
        private static string _moduleName;
        private static string _outputPath;

        private static bool _isBuildingProject;
        private static bool _isRunningProject;
        private static bool _isCreatingTemplate;

        public static void RunVivianTools(IEnumerable<string> args)
        {
            ParseOptions(args);

            if (_isBuildingProject)
            { 
                CompileProgram();
            }

            if (_isCreatingTemplate)
            {
                CreateProjectTemplate(_projectName);
            }
        }
        
        private static void ParseOptions(IEnumerable<string> args)
        {
            var sdkListRequested = false;
            var versionRequested = false;
            var helpRequested = false;
            
            // Options
            // --------------------------- //  
            _options = new()
            {
                $"Vivian SDK ({Version.MidVersion})",
                "Usage: vivian [sdk-options] [command] [command-options] [arguments]",
                "",
                "Execute a Vivian SDK command.",
                "",
                "sdk-options:",
                { "?|h|help", "display help.", _ => helpRequested = true },
                { "list-sdks", "Display the installed SDKs.", _ => sdkListRequested = true },
                { "version", "displays the current version of Vivian installed", _ => versionRequested = true },
                "",
                {  "build=", "builds the {path} of the .vivconfig file", v =>
                {
                    _isBuildingProject = true;
                    _configPath = v;
                }},
                { "run=", "runs project exe with the {path} of the .vivconfig file", v =>
                {
                    _isBuildingProject = true;
                    _isRunningProject = true;
                    _configPath = v;
                }},
                { "new=", "creates a new Vivian project with a {name} and {path}", v =>
                {
                        _isCreatingTemplate = true;
                        _projectName = v;
                }}
            };
            try
            {
                _options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Error.WriteError($"Error parsing {e.OptionName}, please use `-help` for more info");
                return;
            }
            
            if (helpRequested)
            {
                Console.Out.WriteLine($"Vivian SDK ({Version.MidVersion})");
                Console.Out.WriteLine("Usage: vivian [sdk-options] [command] [command-options] [arguments]");
                Console.Out.WriteLine();
                Console.Out.WriteLine("Execute a Vivian SDK command.");
                Console.Out.WriteLine();
                Console.Out.WriteLine("sdk-options:");
                Console.Out.WriteLine("  --version         Display Vivian SDK version in use.");
                Console.Out.WriteLine("  --list-sdks       Display the installed SDKs.");
                Console.Out.WriteLine("  -h|--help         Show command line help.");
                Console.Out.WriteLine();
                Console.Out.WriteLine("  --new             Create a new Vivian project or file.");
                Console.Out.WriteLine("  --run             Build and run a Vivian project output.");
                Console.Out.WriteLine("  --build           Build a Vivian project.");
                Console.Out.WriteLine();
                return;
            }

            if (versionRequested)
            {
                Console.Out.WriteSuccess(Version.FullVersion);
                return;
            }

            if (sdkListRequested)
            {
                Console.Out.WriteLine("");
            }
        }

        private static void ParseConfig()
        {
            // Build the configuration
            // --------------------------- //  
            _config = ParseConfiguration.ParseConfig(_configPath);
            
            if (_config == null)
            {
                Console.Error.WriteError("Error: No configuration file was passed, or the path is incorrect");
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
                Console.Error.WriteError("Error: Unable to locate any '.vivconfig' file to use, please explicitly state the path");
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
                Console.Error.WriteError("Error: need at least one source file");
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

        private static void CompileProgram()
        {
            ParseConfig();
            var syntaxTrees = new List<SyntaxTree>();
            var hasErrors = false;
            
            foreach (var path in _sourcePaths)
            {
                if (!File.Exists(path))
                {
                    Console.Error.WriteError($"Error: file '{path}' doesn't exist");
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
                    Console.Error.WriteError($"Error: file '{path}' doesn't exist");
                    hasErrors = true;
                }
            }

            if (hasErrors)
            {
                return;
            }
            
            // var compilerHost = new ConsoleCompilerHost();
            var compilerService = new CompilerTools();
            compilerService.EmitBinary(syntaxTrees, _moduleName, _references, _outputPath);

            if (_isRunningProject)
            {
                // run project
                Console.Error.WriteError("If you see this message then `vivian -run [project]` is not implemented yet");
                return;
            }

            Console.Out.WriteBuildSummary(true, compilerService.Errors, compilerService.Warnings);
        }

        // Copies all files from the PATH template, and creates directories to build a basic console app
        // This method exists solely for when cross-platform support is added
        private static void CreateProjectTemplate(string projectName)
        {
            if (OperatingSystem.IsWindows())
            {
                CreateWindowsTemplate(projectName);
            }
        }

        private static void CreateWindowsTemplate(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                Console.Error.WriteError("Error: New projects must specify a name");
                return;
            }

            Console.Out.WriteColor($"Confirm project creation at '{projectName}'? ([Y]es / [N]o): ", ConsoleColor.Cyan, false);
            var input = Console.In.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (input.ToLowerInvariant() == "n" && input.ToLowerInvariant() != "y")
                {
                    Environment.Exit(0);
                }
                else if (input.ToLowerInvariant() == "y")
                {
                    Console.Out.WriteColor("Creating Project...");
                }
            }
            
            if (!Directory.Exists(projectName))
            {
                try
                {
                    Directory.CreateDirectory(projectName);
                }
                catch (UnauthorizedAccessException)
                {
                    Console.Error.WriteError("Error: Insufficient permissions to create directory");
                    return;
                }
            }
            
            if (Directory.Exists(projectName))
            {
                if (!Directory.Exists(@"C:/Program Files/vivian"))
                {
                    Console.Error.WriteError("Unable to locate vivian templates");
                    return;
                }
                
                Extensions.DirectoryCopy(@"C:/Program Files/vivian/templates/console", $@"{projectName}", true);
            }

            Console.Out.WriteSuccess($"Project creation at '{projectName}' successful");
        }
    }
}