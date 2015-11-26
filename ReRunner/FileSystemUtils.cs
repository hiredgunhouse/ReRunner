using System;
using System.IO;

namespace ReRunner
{
    /// <summary>
    /// Copy and CopyAll copied from http://stackoverflow.com/questions/58744/best-way-to-copy-the-entire-contents-of-a-directory-in-c-sharp
    /// cause damn .net does not have any method to copy directories
    /// </summary>
    public static class FileSystemUtils
    {
        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static string GetRootedPath(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.Combine(Environment.CurrentDirectory, path);
        }

        public static void EmptyDirectory(string path)
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly))
            {
                if (Directory.Exists(entry))
                    Directory.Delete(entry, true);
                else if (File.Exists(entry))
                    File.Delete(entry);
                else
                    throw new InvalidOperationException(string.Format("Entry is neither file or directory '{0}'", entry));
            }
        }
    }
}