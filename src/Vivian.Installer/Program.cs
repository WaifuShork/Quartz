using System;
using System.IO;
using System.Text;
using Vivian.Tools;

#nullable  disable
namespace Vivian.Installer
{
    internal class Program
    {
        private static bool _isAddingToPath;
        
        private static void Main()
        {
            var unicode = new UnicodeEncoding();
            var logo = File.ReadAllText(@"vivian\logo.txt");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Out.WriteLine(logo);
            Console.ResetColor();

            Console.Out.Write("Confirm installation of Vivian Tools? ([Y]es / [N]o): ");
            var input = Console.In.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                if (input.ToLowerInvariant() == "n" && input.ToLowerInvariant() != "y")
                {
                    Environment.Exit(0);
                }
            }
            
            Console.Out.Write("Would you like to add Vivian Tools to PATH? ([Y]es / [N]o): ");
            input = Console.In.ReadLine();

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

            InstallVivianTools();
        }

        private static void InstallVivianTools()
        {
            var path = string.Empty;
            var root = Path.GetPathRoot(Environment.SystemDirectory);
            
            if (OperatingSystem.IsWindows())
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = @$"{root}Program Files\vivian";
                }
            }
            else if (OperatingSystem.IsLinux())
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = @$"\{root}shared\vivian";
                }
            }
            else
            {
                throw new Exception("Unsupported operating system detected.");
            }
            
            // Attempt to create the directory for both Windows and Linux. 
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                
                DirectoryExtensions.DirectoryCopy(@"vivian", @$"{path}", true);

                if (_isAddingToPath)
                {
                    if (OperatingSystem.IsWindows())
                    {
                        Environment.SetEnvironmentVariable("Path", @$"{path}", EnvironmentVariableTarget.User);
                    }
                    else if (OperatingSystem.IsLinux())
                    {
                        
                    }
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