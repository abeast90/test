using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SmartYouTubeDownloader.Settings
{
    public sealed class UserSettings
    {
        private const string FileName = "settings.json";

    public string SaveDirectory { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "SmartYouTubeDownloader");
    public bool AdvancedMode { get; set; }
    public bool UseProxy { get; set; }
    public string ProxyHost { get; set; } = string.Empty;
    public int ProxyPort { get; set; }
    public string ProxyUser { get; set; } = string.Empty;
    public string ProxyPassword { get; set; } = string.Empty;
    public string LastUrl { get; set; } = string.Empty;
    public string? YtDlpPath { get; set; }
    public string? FfmpegPath { get; set; }
    public bool CompatibilityMode { get; set; }
    public bool AudioOnly { get; set; }
    public bool BatchMode { get; set; }
    public List<string> BatchUrls { get; set; } = new List<string>();

        public static string GetSettingsDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "SmartYouTubeDownloader");
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static string GetToolsDirectory()
        {
            var dir = Path.Combine(GetSettingsDirectory(), "tools");
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static UserSettings Load()
        {
            try
            {
                var dir = GetSettingsDirectory();
                var file = Path.Combine(dir, FileName);
                if (!File.Exists(file))
                {
                    return new UserSettings();
                }

                var json = File.ReadAllText(file);
                var settings = JsonConvert.DeserializeObject<UserSettings>(json);
                return settings ?? new UserSettings();
            }
            catch
            {
                return new UserSettings();
            }
        }

        public void Save()
        {
            try
            {
                var dir = GetSettingsDirectory();
                var file = Path.Combine(dir, FileName);
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(file, json);
            }
            catch
            {
                // ignored
            }
        }
    }
}
