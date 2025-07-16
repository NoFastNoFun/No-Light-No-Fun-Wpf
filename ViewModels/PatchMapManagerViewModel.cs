using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.Dtos;
using Services.Config;
using Microsoft.Win32;
using System.IO;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class PatchMapManagerViewModel : BaseViewModel {
        private readonly AppConfigDto _config;
        public ObservableCollection<PatchMapEntryViewModel> Entries {
            get;
        }
        public ICommand LoadCommand {
            get;
        }
        public ICommand SaveCommand {
            get;
        }
        public ICommand AddCommand {
            get;
        }
        public ICommand DeleteCommand {
            get;
        }
        public ICommand ImportCommand {
            get;
        }
        public ICommand ExportCommand {
            get;
        }

        PatchMapEntryViewModel _selected;
        public PatchMapEntryViewModel SelectedEntry {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public PatchMapManagerViewModel(AppConfigDto appConfigDto) {
            _config = appConfigDto;
            Entries = new ObservableCollection<PatchMapEntryViewModel>();
            if (_config.PatchMap != null)
                foreach (var patch in _config.PatchMap)
                    Entries.Add(new PatchMapEntryViewModel(patch));

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patchmap.csv");
            if (!File.Exists(path)) {
                var template = new[] {
                    "EntityStart,EntityEnd,UniverseStart,UniverseEnd",
                    "100,5099,0,31",
                    "5100,10099,32,63",
                    "10100,15199,64,95",
                    "15200,19858,96,127"
                };
                File.WriteAllLines(path, template);
            }

            LoadCommand = new RelayCommand(_ => Load());
            SaveCommand = new RelayCommand(_ => Save());
            AddCommand = new RelayCommand(_ => AddEntry());
            DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedEntry != null);
            ImportCommand = new RelayCommand(_ => ImportCsv());
            ExportCommand = new RelayCommand(_ => ExportCsv(), _ => Entries.Any());
        }

        void Load() {
            Entries.Clear();
            if (_config.PatchMap != null)
                foreach (var model in _config.PatchMap)
                    Entries.Add(new PatchMapEntryViewModel(model));
        }

        void Save() {
            _config.PatchMap = Entries.Select(vm => vm.ToModel()).ToList();
        }

        void AddEntry() {
            var entry = new PatchMapEntryViewModel();
            Entries.Add(entry);
            Save(); // Synchronise
        }

        void DeleteSelected() {
            if (SelectedEntry != null) {
                Entries.Remove(SelectedEntry);
                Save();
            }
        }

        private void ImportCsv() {
            var dlg = new OpenFileDialog {
                Title = "Importer un patch map CSV",
                Filter = "CSV Files (*.csv)|*.csv",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (dlg.ShowDialog() != true)
                return;

            var lines = File.ReadAllLines(dlg.FileName)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();
            if (lines.Length <= 1)
                return;

            var header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            if (header.Length < 4 ||
                header[0] != "EntityStart" ||
                header[1] != "EntityEnd" ||
                header[2] != "UniverseStart" ||
                header[3] != "UniverseEnd")
                return;

            Entries.Clear();
            var imported = new List<PatchMapEntryDto>();

            foreach (var line in lines.Skip(1)) {
                var parts = line.Split(',').Select(p => p.Trim()).ToArray();
                if (parts.Length != 4)
                    continue;

                if (!int.TryParse(parts[0], out var entityStart) ||
                    !int.TryParse(parts[1], out var entityEnd) ||
                    !byte.TryParse(parts[2], out var universeStart) ||
                    !byte.TryParse(parts[3], out var universeEnd))
                    continue;

                var dto = new PatchMapEntryDto {
                    EntityStart = entityStart,
                    EntityEnd = entityEnd,
                    UniverseStart = universeStart,
                    UniverseEnd = universeEnd
                };
                imported.Add(dto);
                Entries.Add(new PatchMapEntryViewModel(dto));
            }

            _config.PatchMap = imported;
        }

        private void ExportCsv() {
            var dlg = new SaveFileDialog {
                Title = "Exporter un patch map CSV",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = "patchmap.csv"
            };
            if (dlg.ShowDialog() != true)
                return;

            var lines = new List<string> {
                "EntityStart,EntityEnd,UniverseStart,UniverseEnd"
            };
            foreach (var vm in Entries) {
                var m = vm.ToModel();
                lines.Add($"{m.EntityStart},{m.EntityEnd},{m.UniverseStart},{m.UniverseEnd}");
            }

            File.WriteAllLines(dlg.FileName, lines);
        }

        public void SetEntries(List<PatchMapEntryDto> entries) {
            Entries.Clear();
            foreach (var entry in entries)
                Entries.Add(new PatchMapEntryViewModel(entry));
            Save();
        }

        public List<PatchMapEntryDto> ToDto() {
            return Entries.Select(e => e.ToModel()).ToList();
        }
        public Dictionary<int, (int x, int y)> GenerateEntityMap() {
            var map = new Dictionary<int, (int x, int y)>();

            const int columns = 64;
            const int ledsPerDirection = 128; // montée ou descente
            const int totalVisiblePerColumn = ledsPerDirection * 2;
            const int startEntityId = 100;

            int entityId = startEntityId;

            for (int x = 0; x < columns; x++) {
                bool upwardsFirst = (x % 2 == 0); // colonnes paires : montée puis descente, impaires : descente puis montée

                if (upwardsFirst) {
                    // montée
                    for (int y = 0; y < ledsPerDirection; y++) {
                        map[entityId++] = (x, y);
                    }

                    entityId++; // skip LED invisible du haut

                    // descente
                    for (int y = ledsPerDirection; y < ledsPerDirection * 2; y++) {
                        map[entityId++] = (x, y);
                    }

                    entityId++; // skip LED invisible du bas
                }
                else {
                    // descente
                    for (int y = (ledsPerDirection * 2) - 1; y >= ledsPerDirection; y--) {
                        map[entityId++] = (x, y);
                    }

                    entityId++; // skip LED invisible du bas

                    // montée
                    for (int y = ledsPerDirection - 1; y >= 0; y--) {
                        map[entityId++] = (x, y);
                    }

                    entityId++; // skip LED invisible du haut
                }
            }

            return map;
        }

       

        public Dictionary<int, (int x, int y)> GetEntityToPositionMap() {
            var map = new Dictionary<int, (int x, int y)>();
            foreach (var entry in Entries) {
                System.Diagnostics.Debug.WriteLine($"Entry: Start={entry.EntityStart}, End={entry.EntityEnd}, X={entry.X}, Y={entry.Y}, Width={entry.Width}");

                if (entry.Width <= 0) {
                    System.Diagnostics.Debug.WriteLine("⚠️ Entry ignorée car Width <= 0");
                    continue;
                }

                for (int i = 0; i <= entry.EntityEnd - entry.EntityStart; ++i) {
                    int id = entry.EntityStart + i;
                    int x = entry.X + i % entry.Width;
                    int y = entry.Y + i / entry.Width;
                    map[id] = (x, y);
                }
            }
            return map;
        }

    }
}

