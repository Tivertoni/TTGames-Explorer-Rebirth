﻿using System.Text.Json;

namespace TTGamesExplorerRebirthUI
{
    public class AppSettings
    {
        private const string SettingsFilePath = "settings.json";

        public uint Version { get; set; }
        public string GameFolderPath { get; set; }
        public bool KeepLogsOpen {  get; set; }

        public bool UseDecimalFormat { get; set; } = true;

        private static AppSettings _instance;

        public static AppSettings Instance
        {
            get
            {
                _instance ??= new AppSettings();

                _instance.Version = 1;

                return _instance;
            }
        }

        public static void Save()
        {
            File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(_instance));
        }

        public static void Load()
        {
            if (File.Exists(SettingsFilePath))
            {
                _instance = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsFilePath));
            }
        }
    }
}