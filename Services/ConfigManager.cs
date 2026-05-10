using System;
using System.IO;
using System.Text.Json;
using CortexCommandModManager.Models;

namespace CortexCommandModManager.Services
{
    public class ConfigManager
    {
        private const string ConfigFileName = "modmanager_config.json";

        public static ModManagerConfig LoadConfig(string gamePath)
        {
            if (string.IsNullOrEmpty(gamePath))
                return new ModManagerConfig();

            string configPath = Path.Combine(gamePath, ConfigFileName);
            if (!File.Exists(configPath))
                return new ModManagerConfig();

            try
            {
                string json = File.ReadAllText(configPath);
                return JsonSerializer.Deserialize<ModManagerConfig>(json) ?? new ModManagerConfig();
            }
            catch
            {
                return new ModManagerConfig();
            }
        }

        public static void SaveConfig(string gamePath, ModManagerConfig config)
        {
            if (string.IsNullOrEmpty(gamePath))
                return;

            string configPath = Path.Combine(gamePath, ConfigFileName);
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }
    }
}
