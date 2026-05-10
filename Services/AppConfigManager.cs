using System;
using System.IO;
using System.Text.Json;
using CortexCommandModManager.Models;

namespace CortexCommandModManager.Services
{
    public class AppConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

        public static AppConfig Load()
        {
            if (!File.Exists(ConfigPath))
            {
                return new AppConfig();
            }

            try
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }

        public static void Save(AppConfig config)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving app config: {ex.Message}");
            }
        }
    }
}
