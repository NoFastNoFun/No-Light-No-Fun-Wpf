using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using ExcelDataReader;
using Microsoft.Win32;

namespace No_Fast_No_Fun_Wpf.ViewModels
    {
    public abstract class BaseViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifie l’UI que la propriété <paramref name="propertyName"/> a changé.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Affecte la valeur, notifie si elle a changé, et retourne true si changement.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (Equals(field, value))
                return false;
            field = value!;
            OnPropertyChanged(propertyName);
            return true;
        }
        public List<ConfigItem> ImportConfigItemsFromFile() {
            var dialog = new OpenFileDialog {
                Filter = "Fichiers de config (*.csv;*.xlsx)|*.csv;*.xlsx|Tous les fichiers|*.*",
                Title = "Importer une configuration"
            };
            if (dialog.ShowDialog() != true)
                return null;

            var path = dialog.FileName;
            if (Path.GetExtension(path).ToLower() == ".csv")
                return ImportFromCsv(path);
            else if (Path.GetExtension(path).ToLower() == ".xlsx")
                return ImportFromExcel(path);
            return null;
        }

        private List<ConfigItem> ImportFromCsv(string path) {
            var list = new List<ConfigItem>();
            var lines = File.ReadAllLines(path);
            foreach (var line in lines.Skip(1)) // skip header
            {
                var parts = line.Split(';', ',', '\t'); // accepte différents séparateurs
                if (parts.Length < 4)
                    continue;
                if (!ushort.TryParse(parts[0], out var startId))
                    continue;
                if (!ushort.TryParse(parts[1], out var endId))
                    continue;
                if (!byte.TryParse(parts[2], out var universe))
                    continue;
                var ip = parts[3];
                list.Add(new ConfigItem(startId, endId, universe, ip));
            }
            return list;
        }

        private List<ConfigItem> ImportFromExcel(string path) {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var result = new List<ConfigItem>();

            // Saute l’en-tête
            bool first = true;
            while (reader.Read()) {
                if (first) {
                    first = false;
                    continue;
                }
                if (reader.FieldCount < 4)
                    continue;

                var startId = reader.GetValue(0)?.ToString();
                var endId = reader.GetValue(1)?.ToString();
                var universe = reader.GetValue(2)?.ToString();
                var ip = reader.GetValue(3)?.ToString();

                if (ushort.TryParse(startId, out var s) &&
                    ushort.TryParse(endId, out var e) &&
                    byte.TryParse(universe, out var u))
                    result.Add(new ConfigItem(s, e, u, ip ?? ""));
            }
            return result;
        }
    }
}
