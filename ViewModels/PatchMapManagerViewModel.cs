using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.Dtos;
using Services.Config;
using Microsoft.Win32;
using System.IO;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class PatchMapManagerViewModel : BaseViewModel {
        private readonly ConfigEditorViewModel _configEditor;
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

        PatchMapEntryViewModel _selected;
        public PatchMapEntryViewModel SelectedEntry {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public PatchMapManagerViewModel(ConfigEditorViewModel configEditor) {
            _configEditor = configEditor ?? throw new ArgumentNullException(nameof(configEditor));
            Entries = new ObservableCollection<PatchMapEntryViewModel>();

            // Génère un fichier CSV si jamais il manque (utile si tu veux garder un fallback manuel)
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patchmap.csv");
            if (!File.Exists(path)) {
                var template = new[] {
                    "EntityStart,EntityEnd,Universe",
                    "100,5099,0",
                    "5100,10099,32",
                    "10100,15199,64",
                    "15200,19858,96"
                };
                File.WriteAllLines(path, template);
            }

            // Synchronise sur la config officielle
            BuildFromConfig();
            _configEditor.ConfigItems.CollectionChanged += (s, e) => BuildFromConfig();

            LoadCommand = new RelayCommand(_ => BuildFromConfig());
            SaveCommand = new RelayCommand(_ => { /* Ici tu pourrais forcer un export si tu veux */ });
            AddCommand = new RelayCommand(_ => AddEntry());
            DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedEntry != null);
        }

        private void BuildFromConfig() {
            Entries.Clear();
            foreach (var cfg in _configEditor.ConfigItems) {
                // Assure-toi que ce constructeur existe bien dans PatchMapEntryViewModel
                Entries.Add(new PatchMapEntryViewModel(cfg.StartEntityId, cfg.EndEntityId, cfg.Universe));
            }
        }

        void AddEntry() {
            var entry = new PatchMapEntryViewModel();
            Entries.Add(entry);
        }

        void DeleteSelected() {
            if (SelectedEntry != null) {
                Entries.Remove(SelectedEntry);
            }
        }

        public List<PatchMapEntryDto> ToDto() {
            return Entries.Select(e => e.ToModel()).ToList();
        }

        public Dictionary<int, (int x, int y)> GenerateEntityMap() {
            var map = new Dictionary<int, (int x, int y)>();
            const int columns = 64;
            const int ledsPerDirection = 128;
            const int startEntityId = 100;

            int entityId = startEntityId;

            for (int x = 0; x < columns; x++) {
                bool upwardsFirst = (x % 2 == 0);

                if (upwardsFirst) {
                    for (int y = 0; y < ledsPerDirection; y++)
                        map[entityId++] = (x, y);
                    entityId++; // skip
                    for (int y = ledsPerDirection; y < ledsPerDirection * 2; y++)
                        map[entityId++] = (x, y);
                    entityId++;
                }
                else {
                    for (int y = (ledsPerDirection * 2) - 1; y >= ledsPerDirection; y--)
                        map[entityId++] = (x, y);
                    entityId++;
                    for (int y = ledsPerDirection - 1; y >= 0; y--)
                        map[entityId++] = (x, y);
                    entityId++;
                }
            }
            return map;
        }

        public Dictionary<int, (int x, int y)> GetEntityToPositionMap() {
            var map = new Dictionary<int, (int x, int y)>();
            foreach (var entry in Entries) {
                if (entry.Width <= 0)
                    continue;
                for (int i = 0; i <= entry.EntityEnd - entry.EntityStart; ++i) {
                    int id = entry.EntityStart + i;
                    int x = entry.X + i % entry.Width;
                    int y = entry.Y + i / entry.Width;
                    map[id] = (x, y);
                }
            }
            return map;
        }
        public void SetEntries(List<PatchMapEntryDto> entries) {
            Entries.Clear();
            foreach (var entry in entries)
                Entries.Add(new PatchMapEntryViewModel(entry));
        }
    }
}

