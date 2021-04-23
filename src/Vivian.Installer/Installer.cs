using System;
using System.IO;
using System.Text;
using System.Security;
using System.Reflection;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Security.Principal;

using Vivian.IO;

// TODO: insert bug tracker links
namespace Vivian.Installer
{
    internal static class Installer
    {
        public static async Task<int> Install()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                var isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
                if (!isElevated)
                {
                    await Console.Error.WriteErrorAsync("Unable to execute installer/uninstall, please run as administrator.");
                    return 0;
                }
            }

            // Force 64 bit
            // Do not allow 32 bit until the language runtime itself supports it
            if (!Environment.Is64BitOperatingSystem)
            {
                await Console.Error.WriteErrorAsync("Error: Unsupported OS Architecture detected, please ensure you're using Windows OS (x64 bit)");
                Console.ReadKey();
                return 0;
            }
            
            // Find the embedded resource 
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Vivian.vivian-x64.zip");
            if (stream == null)
            {
                await Console.Error.WriteErrorAsync($"Internal Error: <{stream}> \n\nPlease see <bug-tracker>");
                Console.ReadKey();
                return 1;
            }
            
            var zip = new ZipArchive(stream);
            
            // Fetch and print the logo
            var logo = await FetchLogoAsync(zip, "logo.txt");
            await Console.Out.WriteColorAsync(logo, ConsoleColor.Magenta);

            await Console.Out.WriteAsync("Install or Uninstall? ([I]nstall / [U]ninstall");
            var input = await Console.In.ReadLineAsync();

            if (!string.IsNullOrWhiteSpace(input))
            {
                var path = @$"{Path.GetPathRoot(Environment.SystemDirectory)}Program Files\vivian";
                if (input.ToLowerInvariant() == "install" | input.ToLowerInvariant() == "i")
                {
                    await Console.Out.WriteAsync("Confirm installation of Vivian Tools? ([Y]es / [N]o): ");
                    input = await Console.In.ReadLineAsync();

                    // prompt user
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        if (!PromptUser(input))
                        {
                            return 0;
                        }
                    }

                    await Console.Out.WriteAsync("Would you like to add Vivian Tools to PATH? ([Y]es / [N]o): ");
                    input = await Console.In.ReadLineAsync();
                    var isAddingToPath = false;

                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        isAddingToPath = PromptUser(input);
                    }

                    await InstallVivianTools(isAddingToPath, path, zip);
                    
                    await Console.Out.WriteSuccessAsync("Successfully installed Vivian Tools. Press any key to exit...");
                    await Task.Run(() => Console.ReadKey(true).Key);
                    return 0;
                }
                
                // Leaving this temporarily unimplemented
                // not tryna fuck up shit
                // if (input.ToLowerInvariant() == "uninstall" | input.ToLowerInvariant() == "u")
                //{
                    // if for some reason the path isn't at the SystemDir\Program Files\vivian
                    // something is wrong and should be aborted
                //    await UninstallVivianTools(path);
                //    return 0;
                //}
            }

            // if we somehow reached this point, then we've done something wrong
            return 1;
        }

        private static async Task UninstallVivianTools(string path)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    Directory.Delete(path);
                }
                catch (UnauthorizedAccessException)
                {
                    await Console.Error.WriteErrorAsync($"Error: Unable to uninstall Vivian Tools at {path}\n" +
                                                         "Reason: Insufficient Permissions");
                }
                catch (IOException)
                {
                    await Console.Error.WriteErrorAsync($"Error: Vivian Tools doesn't exist at {path}");
                }
                catch (Exception)
                {
                    await Console.Error.WriteErrorAsync("Internal Error: Something internal went wrong, please report to <bug-tracker>");
                }
            }
        }

        private static bool PromptUser(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.Error.WriteErrorAsync("Please provide an input");
            }
            
            if (input.ToLowerInvariant() == "y" | input.ToLowerInvariant() == "yes")
            {
                return true;
            }
            if (input.ToLowerInvariant() == "n" | input.ToLowerInvariant() == "no")
            {
                return false;
            }
            
            // if they enter another key, they may have mistyped and don't want to install
            return false;
        }
        
        private static async Task InstallVivianTools(bool isAddingToPath, string path, ZipArchive zip)
        {
            await ExtractResources(path, zip);
            
            if (isAddingToPath)
            {
                await AppendToPath(path);
            }
        }
        
        private static async Task ExtractResources(string path, ZipArchive zip)
        {
            try
            {
                zip.ExtractToDirectory(path);
            }
            catch (UnauthorizedAccessException)
            {
                await Console.Error.WriteErrorAsync($"Error: Unable to install Vivian Tools at {path}\n" +
                                                     "Reason: Insufficient Permissions");
            }
            catch (IOException)
            {
                await Console.Error.WriteErrorAsync($"Error: Vivian Tools already exists at {path}");
            }
            catch (Exception)
            {
                await Console.Error.WriteErrorAsync("Internal Error: Something internal went wrong, please report to <bug-tracker>");
            }
        }
        
        private static async Task<string> FetchLogoAsync(ZipArchive zip, string fileName)
        {
            var logo = string.Empty;
            foreach (var entry in zip.Entries)
            {
                using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
                if (entry.Name == fileName)
                {
                    logo = await reader.ReadToEndAsync();
                }
            }

            if (string.IsNullOrEmpty(logo))
            {
                logo = "Vivian Tools";
            }

            return logo;
        }
        
        // Credit: https://github.com/ANF-Studios/WinPath/blob/master/WinPath/src/Library.cs
        private static async Task AppendToPath(string value)
        {
            var path = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine);
            if (!string.IsNullOrWhiteSpace(path))
            {
                await BackupPath(path);
                try
                {
                    Environment.SetEnvironmentVariable
                    (
                        "Path",
                        path.EndsWith(";")
                            ? $"{path}{value};"
                            : $";{path}{value};",
                        EnvironmentVariableTarget.Machine
                    );
                }
                catch (SecurityException)
                {
                    await Console.Error.WriteErrorAsync("Error: Requested registry access is not allowed.");
                }
                
            }
            else
            {
                await Console.Error.WriteErrorAsync("Something went wrong when attempting to add Vivian into your Path:\n" +
                                                   "- Please make sure that Path exists in your environment variables (for System)\n" +
                                                   "- Please ensure that you have sufficient permissions to add a variable to the (System) path\n\n" + 
                                                   "If you believe this is a bug in the installer, please report an issue to: \n" +
                                                   "(https://github.com/WaifuShork/Vivian/issues)");
            }
        }
        
        private static async Task BackupPath(string pathVariable)
        {
            var backupDirectory = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Vivian\Backup\User";
            await Console.Out.WriteSuccessAsync($"Path has been backed up to: {backupDirectory}");
            try
            {
                Directory.CreateDirectory(backupDirectory);
                await File.WriteAllTextAsync($"{backupDirectory}{DateTime.Now.ToFileTime()}", pathVariable);
            }
            // Make sure to catch insufficient permissions before anything else
            // because the user may not have permissions depending on user type
            catch (UnauthorizedAccessException)
            {
                await Console.Error.WriteErrorAsync($"Error: Unable to backup PATH at {backupDirectory}\n" +
                                                     "Reason: Insufficient Permissions");
            }
            catch (IOException)
            {
                await Console.Error.WriteErrorAsync($"Internal Error: If you see this message please report an issue to <bug-tracker>");
            }
            catch (Exception e)
            {
                await Console.Error.WriteErrorAsync($"Error: Unable to backup PATH at{backupDirectory}\n" +
                                                    $"Reason: {e.Message}");
            }
        }
    }
}