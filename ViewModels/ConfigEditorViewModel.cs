using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.Dtos;
using Core.Models;
using ExcelDataReader;
using Microsoft.Win32;
using Services.Config;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class ConfigEditorViewModel : BaseViewModel {
        private readonly IJsonFileService<ConfigDto> _service;

        public ObservableCollection<ConfigItemViewModel> ConfigItems {
            get;
        }
        ConfigItemViewModel _selectedConfigItem;
        public ConfigItemViewModel SelectedConfigItem {
            get => _selectedConfigItem;
            set => SetProperty(ref _selectedConfigItem, value);
        }

        public ICommand LoadConfigCommand {
            get;
        }
        public ICommand SaveConfigCommand {
            get;
        }
        public ICommand AddItemCommand {
            get;
        }
        public ICommand DeleteItemCommand {
            get;
        }
        public ICommand ImportConfigCsvCommand {
            get;
        }
        public ICommand ImportConfigExcelCommand {
            get;
        }   
        public ConfigEditorViewModel() {
            _service = new JsonFileConfigService<ConfigDto>("config.json");
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            ConfigItems = new ObservableCollection<ConfigItemViewModel>();
            LoadConfigCommand = new RelayCommand(_ => Load());
            SaveConfigCommand = new RelayCommand(_ => Save());
            AddItemCommand = new RelayCommand(_ => ConfigItems.Add(new ConfigItemViewModel()));
            DeleteItemCommand = new RelayCommand(
                _ => DeleteSelected(),
                _ => SelectedConfigItem != null
            );
            ImportConfigCsvCommand = new RelayCommand(_ => ImportCsv());
            ImportConfigExcelCommand = new RelayCommand(_ => ImportExcel());
            Load();
        }

        void Load() {
            var cfg = _service.Load();
            ConfigItems.Clear();
            foreach (var model in cfg.Items)
                ConfigItems.Add(new ConfigItemViewModel(model));
        }

        void Save() {
            var dto = new ConfigDto {
                Items = ConfigItems.Select(vm => vm.ToModel()).ToList()
            };
            _service.Save(dto);
        }

        void DeleteSelected() {
            if (SelectedConfigItem != null)
                ConfigItems.Remove(SelectedConfigItem);
        }
        private void ImportCsv() {
            var dlg = new Microsoft.Win32.OpenFileDialog {
                Title = "Importer un fichier CSV",
                Filter = "CSV Files (*.csv)|*.csv|Tous les fichiers (*.*)|*.*"
            };
            if (dlg.ShowDialog() != true)
                return;

            var lines = File.ReadAllLines(dlg.FileName);

            ConfigItems.Clear();

            foreach (var line in lines.Skip(1)) { // skip header
                var parts = line.Split(';'); // ou ',' selon le séparateur de ton CSV
                if (parts.Length < 5)
                    continue;

                if (!ushort.TryParse(parts[1], out var startId))
                    continue;
                if (!ushort.TryParse(parts[2], out var endId))
                    continue;
                var ip = parts[3];
                if (!byte.TryParse(parts[4], out var universe))
                    continue;

                var item = new ConfigItem(startId, endId, universe, ip);
                ConfigItems.Add(new ConfigItemViewModel(item));
            }
        }

        private void ImportExcel() {
        var dlg = new OpenFileDialog {
            Title = "Importer un fichier Excel",
            Filter = "Fichier Excel (*.xlsx)|*.xlsx"
        };

        if (dlg.ShowDialog() != true)
            return;

        using (var stream = File.Open(dlg.FileName, FileMode.Open, FileAccess.Read))
        using (var reader = ExcelReaderFactory.CreateReader(stream)) {
            var result = reader.AsDataSet();
            var table = result.Tables[0]; // Feuille 1

            // Supposons que la première ligne est l’entête
            for (int i = 1; i < table.Rows.Count; i++) // i=1 pour sauter l’entête
            {
                var row = table.Rows[i];

                // Adapter les indices si l’ordre change (0 = A, 1 = B, etc.)
                var startEntityStr = row[1]?.ToString();
                var endEntityStr = row[2]?.ToString();
                var ip = row[3]?.ToString();
                var universeStr = row[4]?.ToString();

                if (ushort.TryParse(startEntityStr, out var startEntity)
                    && ushort.TryParse(endEntityStr, out var endEntity)
                    && byte.TryParse(universeStr, out var universe)) {
                    var item = new ConfigItem(startEntity, endEntity, universe, ip);
                    ConfigItems.Add(new ConfigItemViewModel(item)); // Ou autre ajout selon ta collection
                }
            }
        }
    }


}
}
