using System.Collections.ObjectModel;
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
                "100,4858,0,31",
                "5100,9858,32,63",
                "10100,14858,64,95",
                "15100,19858,96,127"
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
    }
}

