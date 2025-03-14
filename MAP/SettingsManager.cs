using System;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;

namespace ABSProject
{
    public static class SettingsManager
    {
        public static readonly string ConfigFilePath = Path.Combine(@"C:\LibraryCompare", "config.json");
        public static ConfigModel Current = new ConfigModel();
        public static BindingList<LibrarySettings> Libraries = new BindingList<LibrarySettings>();

        public static void LoadSettings()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var loaded = JsonConvert.DeserializeObject<ConfigModel>(json);
                    if (loaded != null)
                    {
                        Current = loaded;
                        Libraries.Clear();
                        foreach (var lib in Current.Libraries)
                            Libraries.Add(lib);
                        return;
                    }
                }
            }
            catch { }
            Current = new ConfigModel();
            Libraries.Clear();
        }

        public static void SaveSettings()
        {
            try
            {
                Current.Libraries.Clear();
                foreach (var lib in Libraries)
                    Current.Libraries.Add(lib);
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath)!);
                string json = JsonConvert.SerializeObject(Current, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch { }
        }

        public static string MakeBookKey(string title, string author, string series)
        {
            title = (title ?? "").Trim().ToLowerInvariant();
            author = (author ?? "").Trim().ToLowerInvariant();
            series = (series ?? "").Trim().ToLowerInvariant();
            return $"{title}|{author}|{series}";
        }
    }
}
