using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CortexCommandModManager.Models;

namespace CortexCommandModManager.Services
{
    public class ModService
    {
        public static List<ModInfo> FindModsInWorkshop(List<string> workshopFolders)
        {
            var mods = new List<ModInfo>();

            foreach (var folder in workshopFolders)
            {
                try
                {
                    // Recursively search for *.rte.* files. 
                    var files = Directory.GetFiles(folder, "*.rte.*", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var mod = AnalyzeArchive(file);
                        if (mod != null)
                        {
                            mods.Add(mod);
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore access errors
                }
            }

            return mods;
        }

        private static ModInfo AnalyzeArchive(string archivePath)
        {
            try
            {
                using var archive = ZipFile.OpenRead(archivePath);
                
                string rteFolderName = null;
                bool hasIcon = false;

                foreach (var entry in archive.Entries)
                {
                    var parts = entry.FullName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    if (parts.Length > 0 && parts[0].EndsWith(".rte", StringComparison.OrdinalIgnoreCase))
                    {
                        rteFolderName = parts[0];
                    }
                    
                    if (entry.FullName.EndsWith("icon.png", StringComparison.OrdinalIgnoreCase) || 
                        entry.FullName.EndsWith("icon.bmp", StringComparison.OrdinalIgnoreCase))
                    {
                        hasIcon = true;
                    }
                }

                if (string.IsNullOrEmpty(rteFolderName))
                {
                    var fileName = Path.GetFileNameWithoutExtension(archivePath);
                    while (fileName.Contains('.'))
                    {
                        var lastDot = fileName.LastIndexOf('.');
                        if (fileName.Substring(lastDot).Equals(".rte", StringComparison.OrdinalIgnoreCase))
                        {
                            break;
                        }
                        fileName = fileName.Substring(0, lastDot);
                    }
                    if (!fileName.EndsWith(".rte", StringComparison.OrdinalIgnoreCase))
                    {
                        fileName += ".rte";
                    }
                    rteFolderName = fileName;
                }

                return new ModInfo
                {
                    Name = rteFolderName.Replace(".rte", "", StringComparison.OrdinalIgnoreCase),
                    ArchivePath = archivePath,
                    RteFolderName = rteFolderName,
                    IsInstalled = false,
                    IconPath = null 
                };
            }
            catch
            {
                return null;
            }
        }

        public static void InstallMod(ModInfo mod, string gamePath)
        {
            if (string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath))
                throw new DirectoryNotFoundException("Game path not found.");

            string targetRtePath = Path.Combine(gamePath, mod.RteFolderName);
            
            using var archive = ZipFile.OpenRead(mod.ArchivePath);
            
            bool hasRootRteFolder = archive.Entries.Any(e => 
            {
                var parts = e.FullName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                return parts.Length > 0 && parts[0].Equals(mod.RteFolderName, StringComparison.OrdinalIgnoreCase);
            });

            if (!Directory.Exists(targetRtePath))
            {
                Directory.CreateDirectory(targetRtePath);
            }

            foreach (var entry in archive.Entries)
            {
                // Skip directories
                if (string.IsNullOrEmpty(entry.Name)) continue;

                string relativePath = entry.FullName;
                if (hasRootRteFolder)
                {
                    var parts = relativePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 0 && parts[0].Equals(mod.RteFolderName, StringComparison.OrdinalIgnoreCase))
                    {
                        relativePath = string.Join(Path.DirectorySeparatorChar.ToString(), parts.Skip(1));
                    }
                }
                
                string destinationPath = Path.Combine(targetRtePath, relativePath);
                string directory = Path.GetDirectoryName(destinationPath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                entry.ExtractToFile(destinationPath, overwrite: true);
            }

            mod.IsInstalled = true;
            mod.InstalledPath = targetRtePath;
        }

        public static void UninstallMod(ModInfo mod, string gamePath)
        {
            if (string.IsNullOrEmpty(gamePath) || !Directory.Exists(gamePath))
                throw new DirectoryNotFoundException("Game path not found.");

            string targetRtePath = Path.Combine(gamePath, mod.RteFolderName);
            
            if (Directory.Exists(targetRtePath))
            {
                Directory.Delete(targetRtePath, true);
            }

            mod.IsInstalled = false;
            mod.InstalledPath = null;
        }
    }
}
