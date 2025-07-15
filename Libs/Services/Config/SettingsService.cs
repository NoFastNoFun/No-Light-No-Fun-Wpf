using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Messages;

namespace Services.Config
{
    public class SettingsService {
        const string FILE = "appsettings.json";
        public ReceiverSettings Settings {
            get; private set;
        }

        public event Action<ReceiverSettings> SettingsChanged;

        public SettingsService() {
            if (File.Exists(FILE)) {
                var json = File.ReadAllText(FILE);
                Settings = JsonSerializer.Deserialize<ReceiverSettings>(json);
            }
            else {
                Settings = new ReceiverSettings();
                Save();
            }
            WatchFile();
        }

        void WatchFile() {
            var watcher = new FileSystemWatcher(".", FILE);
            watcher.Changed += (_, __) => {
                var json = File.ReadAllText(FILE);
                Settings = JsonSerializer.Deserialize<ReceiverSettings>(json);
                SettingsChanged?.Invoke(Settings);
            };
            watcher.EnableRaisingEvents = true;
        }

        public void Save() {
            var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FILE, json);
            SettingsChanged?.Invoke(Settings);
        }
    }
}
