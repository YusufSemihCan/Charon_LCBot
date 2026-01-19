using System;
using System.IO;

namespace Charon.Logic.Navigation
{
    public static class PathResolver
    {
        public static string GetNavigationAssetsPath()
        {
            string? baseDir = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo? dir = new DirectoryInfo(baseDir);

            // Search upwards for the folder named "Assets"
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Assets")))
            {
                dir = dir.Parent;
            }

            if (dir == null)
                throw new DirectoryNotFoundException("Could not find the root 'Assets' folder! Check your repository structure.");

            return Path.Combine(dir.FullName, "Assets", "Navigation");
        }
    }
}