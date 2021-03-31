using System.IO;

namespace Vivian.Tools
{
    public static class DirectoryExtensions
    {
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
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