﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.Dtos;
using Services.Config;
using Microsoft.Win32;
using System.IO;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class PatchMapManagerViewModel : BaseViewModel {
        readonly IJsonFileService<PatchMapDto> _patchService;


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

        public PatchMapManagerViewModel() {
            _patchService = new JsonFileConfigService<PatchMapDto>("patchmap.json");
            Entries = new ObservableCollection<PatchMapEntryViewModel>();
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
            AddCommand = new RelayCommand(_ => Entries.Add(new PatchMapEntryViewModel()));
            DeleteCommand = new RelayCommand(_ => {
                if (SelectedEntry != null)
                    Entries.Remove(SelectedEntry);
            }, _ => SelectedEntry != null);
            ImportCommand = new RelayCommand(_ => ImportCsv());
            ExportCommand = new RelayCommand(_ => ExportCsv(), _ => Entries.Any());

            Load();
        }

        void Load() {
            var dto = _patchService.Load(); // DTO doit contenir List<PatchMapEntryDto>
            Entries.Clear();
            foreach (var model in dto.Items)
                Entries.Add(new PatchMapEntryViewModel(model));
        }

        void Save() {
            var dto = new PatchMapDto {
                Items = Entries.Select(vm => vm.ToModel()).ToList()
            };
            _patchService.Save(dto);
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
                            // ignore les lignes vides
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToArray();
            if (lines.Length <= 1)
                return;  // pas de données

            // Optionnel : vérifier que l'en-tête correspond
            var header = lines[0].Split(',').Select(h => h.Trim()).ToArray();
            if (header.Length < 4 ||
                header[0] != "EntityStart" ||
                header[1] != "EntityEnd" ||
                header[2] != "UniverseStart" ||
                header[3] != "UniverseEnd") {
                // l’en-tête ne matche pas ce qu’on attend : on peut logguer ou échouer
                return;
            }

            Entries.Clear();

            foreach (var line in lines.Skip(1)) {
                var parts = line.Split(',')
                                .Select(p => p.Trim())
                                .ToArray();
                if (parts.Length != 4)
                    continue;

                // Essaie de parser chaque colonne
                if (!int.TryParse(parts[0], out var entityStart) ||
                    !int.TryParse(parts[1], out var entityEnd) ||
                    !byte.TryParse(parts[2], out var universeStart) ||
                    !byte.TryParse(parts[3], out var universeEnd)) {
                    // Si un parse échoue, on skip cette ligne
                    continue;
                }

                var dto = new PatchMapEntryDto {
                    EntityStart = entityStart,
                    EntityEnd = entityEnd,
                    UniverseStart = universeStart,
                    UniverseEnd = universeEnd
                };
                Entries.Add(new PatchMapEntryViewModel(dto));
            }
        }
        private void ExportCsv() {
            var dlg = new SaveFileDialog {
                Title = "Exporter un patch map CSV",
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = "patchmap.csv"
            };
            if (dlg.ShowDialog() != true)
                return;

            // Prépare les lignes
            var lines = new List<string>
            {
            "EntityStart,EntityEnd,UniverseStart,UniverseEnd"
        };
            foreach (var vm in Entries) {
                var m = vm.ToModel(); // retourne un PatchMapEntryDto
                lines.Add($"{m.EntityStart},{m.EntityEnd},{m.UniverseStart},{m.UniverseEnd}");
            }

            File.WriteAllLines(dlg.FileName, lines);
        }
        public void SetEntries(List<PatchMapEntryDto> entries) {
            Entries.Clear();
            foreach (var entry in entries)
                Entries.Add(new PatchMapEntryViewModel(entry));
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

        // Add: Generate entity map using backend's ledToXY logic
        public static Dictionary<int, (int x, int y)> GenerateEntityMapLikeBackend()
        {
            // Constants from backend
            const int gridW = 128;
            const int gridH = 128;
            const int ledsPerFull = 170; // even universe
            const int ledsPerHalf = 85;  // odd universe
            const int missingOdd = 3;    // last 3 LEDs physically absent
            const int gapEven = 6;       // 6-pixel gap after even-universe lower strip
            const int ledsPerPair = ledsPerFull + ledsPerHalf;
            const int total = 16320; // Use the same total as backend
            const int entityOffset = 100;

            var map = new Dictionary<int, (int x, int y)>();

            for (int idx = 0; idx < total; idx++)
            {
                int pair = idx / ledsPerPair;
                int off = idx % ledsPerPair;
                int row = pair * 2;
                int x = 0, y = 0;
                bool ok = false;

                if (off < gridW)
                {
                    // top row (128 px)
                    x = row;
                    y = off;
                    ok = true;
                }
                else
                {
                    off -= gridW; // 0‥126 in lower half
                    if (off < ledsPerFull - gridW - gapEven)
                    {
                        // 36 px driven by even universe
                        x = row + 1;
                        y = off;
                        ok = true;
                    }
                    else
                    {
                        off -= ledsPerFull - gridW - gapEven; // skip 6-px gap
                        if (off >= ledsPerHalf - missingOdd)
                        {
                            // last 3 px of odd universe absent
                            ok = false;
                        }
                        else
                        {
                            x = row + 1;
                            y = (ledsPerFull - gridW - gapEven) + off;
                            ok = true;
                        }
                    }
                }
                if (ok)
                {
                    int eid = idx + entityOffset;
                    map[eid] = (x, y);
                }
            }
            return map;
        }

        // In GetEntityToPositionMap, fallback to this if no entries
        public Dictionary<int, (int x, int y)> GetEntityToPositionMap() {
            if (Entries.Count == 0)
            {
                return GenerateEntityMapLikeBackend();
            }
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

