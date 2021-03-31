using System;
using System.Diagnostics;
using System.IO;
using Vivian.Tools;

#nullable  disable
namespace Vivian.Installer
{
    internal class Program
    {
        private static string _installationPath;
        private static bool _isAddingToPath;
        
        private static void Main()
        {
            var logo = File.ReadAllText(@"vivian\logo.txt");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Out.WriteLine(logo);
            Console.ResetColor();

            Console.Out.Write("Please specify the path you'd like to install to: ");
            _installationPath = Console.In.ReadLine();
            
            Console.Out.Write("Would you like to add Vivian Tools to PATH? ([Y]es / [N]o): ");
            var input = Console.In.ReadLine();

            if (input != null)
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

            InstallVivianTools(_installationPath!);
        }
        
        // TODO: Does directory already exist? 
        // TODO: What happens if the user has insufficient permissions?
        private static void InstallVivianTools(string path)
        {
            if (OperatingSystem.IsLinux())
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = @"C:\Program Files\vivian";
                }
            }
            else if (OperatingSystem.IsWindows())
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = @"\usr\shared\vivian";
                }
            }
            else
            {
                throw new Exception("Unsupported operating system detected.");
            }
            
            // Attempt to create the directory for both Windows and Linux. 
            try
            {
                Directory.CreateDirectory(path);
                
                DirectoryExtensions.DirectoryCopy(@"vivian", @$"{path}", true);

                if (_isAddingToPath)
                {
                    Environment.SetEnvironmentVariable("Path", @$"{path}");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"Error: Unable to install Vivian Tools at {path}\n" +
                                         "Reason: Insufficient Permissions");
            }
            catch (IOException)
            {
                Console.Error.WriteLine($"Error: Vivian Tools already exists at {path}");
            }
        }
    }
}