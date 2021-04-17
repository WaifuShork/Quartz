using System;
using System.IO;
using System.Threading.Tasks;
using Vivian.Tools;

namespace Vivian.Installer
{
    internal class Installer
    {
        private bool _isAddingToPath;

        public Installer() { }
        
        public async Task Install()
        {
            var logo = await File.ReadAllTextAsync(@"vivian\logo.txt");
            Console.ForegroundColor = ConsoleColor.Magenta;
            await Console.Out.WriteLineAsync(logo);
            Console.ResetColor();

            await Console.Out.WriteAsync("Confirm installation of Vivian Tools? ([Y]es / [N]o): ");
            var input = await Console.In.ReadLineAsync();

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (input.ToLowerInvariant() == "n" && input.ToLowerInvariant() != "y")
                {
                    Environment.Exit(0);
                }
            }
            
            await Console.Out.WriteAsync("Would you like to add Vivian Tools to PATH? ([Y]es / [N]o): ");
            input = await Console.In.ReadLineAsync();

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (input.ToLowerInvariant() == "y")
                {
                    _isAddingToPath = true;
                }
                else if (input.ToLowerInvariant() == "n")
                {
                    _isAddingToPath = false;
                }
            }

            await InstallVivianTools();
            await Console.Out.WriteLineAsync("Successfully installed Vivian Tools.");
            await Console.Out.WriteLineAsync("Press any key to exit...");
            Console.ReadKey();
        }
        
        private async Task InstallVivianTools()
        {
            var path = string.Empty;
            var root = Path.GetPathRoot(Environment.SystemDirectory);
            
            // Double check OS for safety cause :shrug:
            if (OperatingSystem.IsWindows())
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    if (Environment.Is64BitOperatingSystem)
                    {
                        path = @$"{root}Program Files\vivian";
                    }
                    else
                    {
                        path = $@"{root}Program Files (x86)\vivian";
                    }
                }
            }
            else
            {
                await Console.Error.WriteLineAsync("Unsupported operating system detected.");
                return;
            }
            
            // Attempt to create the directory
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                
                Extensions.DirectoryCopy(@"vivian", @$"{path}", true);

                if (_isAddingToPath)
                {
                    await AppendToPath(path);
                    // Environment.SetEnvironmentVariable("Path", @$"{path}", EnvironmentVariableTarget.User);
                }
            }
            catch (UnauthorizedAccessException)
            {
                await Console.Error.WriteLineAsync($"Error: Unable to install Vivian Tools at {path}\n" +
                                                    "Reason: Insufficient Permissions");
            }
            catch (IOException)
            {
                await Console.Error.WriteLineAsync($"Error: Vivian Tools already exists at {path}");
            }
        }

        // Credit: https://github.com/ANF-Studios/WinPath/blob/master/WinPath/src/Library.cs
        private async Task AppendToPath(string value)
        {
            var path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            if (!string.IsNullOrWhiteSpace(path))
            {
                await BackupPath(path);
                Environment.SetEnvironmentVariable
                (
                    "Path",
                    path.EndsWith(";") 
                        ? $"{path}{value};"
                        : $";{path}{value};",
                    EnvironmentVariableTarget.Machine
                );
            }
            else
            {
                await Console.Error.WriteLineAsync("Something went wrong when attempting to add Vivian into your Path:\n" +
                                                   "- Please make sure that Path exists in your environment variables (for User)\n" +
                                                   "- Please ensure that you have sufficient permissions to add a variable to the (User) path\n\n" + 
                                                   "If you believe this is a bug in the installer, please report an issue to: \n" +
                                                   "(https://github.com/WaifuShork/Vivian/issues)");
            }
        }
        
        private async Task BackupPath(string pathVariable)
        {
            var backupDirectory = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Vivian\Backup\User";
            await Console.Out.WriteLineAsync($"Path has been backed up to: {backupDirectory}");
            Directory.CreateDirectory(backupDirectory);
            try
            {
                await File.WriteAllTextAsync($"{backupDirectory}{DateTime.Now.ToFileTime()}", pathVariable);
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync($"Unable to backup path: {e.Message}");
            }
        }
    }
}