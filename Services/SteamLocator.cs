using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace CortexCommandModManager.Services
{
    public class SteamLocator
    {
        public static string GetSteamPath()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                if (key != null)
                {
                    var path = key.GetValue("SteamPath") as string;
                    if (!string.IsNullOrEmpty(path))
                    {
                        return path.Replace('/', '\\');
                    }
                }
            }
            catch { }
            return null;
        }

        public static List<string> FindWorkshopModsFolders(string manualSteamPath = null)
        {
            var steamPath = !string.IsNullOrWhiteSpace(manualSteamPath) ? manualSteamPath : GetSteamPath();
            if (string.IsNullOrEmpty(steamPath))
            {
                return new List<string>();
            }

            var userdataDir = Path.Combine(steamPath, "userdata");
            if (!Directory.Exists(userdataDir))
            {
                return new List<string>();
            }

            var workshopModsFolders = new List<string>();
            SearchForWorkshopMods(userdataDir, workshopModsFolders);
            return workshopModsFolders;
        }

        private static void SearchForWorkshopMods(string currentDir, List<string> results)
        {
            try
            {
                var dirs = Directory.GetDirectories(currentDir);
                foreach (var dir in dirs)
                {
                    if (Path.GetFileName(dir).Equals("WorkshopMods", StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(dir);
                    }
                    else
                    {
                        SearchForWorkshopMods(dir, results);
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
        }

        public static string FindCortexCommandPath(string manualSteamPath = null)
        {
            var steamPath = !string.IsNullOrWhiteSpace(manualSteamPath) ? manualSteamPath : GetSteamPath();
            if (string.IsNullOrEmpty(steamPath)) return null;

            var libraryFoldersVdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            var libraryPaths = new List<string> { steamPath };

            if (File.Exists(libraryFoldersVdf))
            {
                try
                {
                    var content = File.ReadAllText(libraryFoldersVdf);
                    var matches = Regex.Matches(content, "\"path\"\\s+\"([^\"]+)\"");
                    foreach (Match match in matches)
                    {
                        if (match.Groups.Count > 1)
                        {
                            libraryPaths.Add(match.Groups[1].Value.Replace("\\\\", "\\"));
                        }
                    }
                }
                catch { }
            }

            foreach (var libraryPath in libraryPaths)
            {
                var ccPath = Path.Combine(libraryPath, "steamapps", "common", "Cortex Command");
                if (Directory.Exists(ccPath))
                {
                    return ccPath;
                }
            }

            return null;
        }
    }
}
