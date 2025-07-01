using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.Dtos;
using Core.Models;
using Services.Config;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class ReceiverConfigPanelViewModel : BaseViewModel {
        readonly IJsonFileService<DmxSettingsDto> _service;

        public ObservableCollection<DmxRouterSettings> Routers {
            get;
        }

        // Sélection d’un routeur
        DmxRouterSettings _selectedRouter;
        public DmxRouterSettings SelectedRouter {
            get => _selectedRouter;
            set {
                if (SetProperty(ref _selectedRouter, value)) {
                    // réévalue la disponibilité des commandes
                    (AddRangeCmd as RelayCommand)?.RaiseCanExecuteChanged();
                    (DelRouterCmd as RelayCommand)?.RaiseCanExecuteChanged();
                    // on désélectionne aussi la range courante
                    SelectedRange = null;
                }
            }
        }

        // Sélection d’une plage DMX
        UniverseMap _selectedRange;
        public UniverseMap SelectedRange {
            get => _selectedRange;
            set {
                if (SetProperty(ref _selectedRange, value)) {
                    (DelRangeCmd as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Les commandes
        public ICommand LoadCommand {
            get;
        }
        public ICommand SaveCommand {
            get;
        }
        public ICommand AddRouterCmd {
            get;
        }
        public ICommand DelRouterCmd {
            get;
        }
        public ICommand AddRangeCmd {
            get;
        }
        public ICommand DelRangeCmd {
            get;
        }

        public ReceiverConfigPanelViewModel() {
            _service = new JsonFileConfigService<DmxSettingsDto>("dmxsettings.json");
            Routers = new ObservableCollection<DmxRouterSettings>();

            // Charger / Sauver
            LoadCommand = new RelayCommand(_ => Load());
            SaveCommand = new RelayCommand(_ => Save());

            // Ajouter / supprimer un router
            AddRouterCmd = new RelayCommand(_ => {
                var router = new DmxRouterSettings();
                Routers.Add(router);
                SelectedRouter = router;
            });
            DelRouterCmd = new RelayCommand(_ => {
                if (SelectedRouter != null) {
                    Routers.Remove(SelectedRouter);
                    SelectedRouter = null;
                }
            }, _ => SelectedRouter != null);

            // Ajouter / supprimer une range sur le router sélectionné
            AddRangeCmd = new RelayCommand(_ => {
                var range = new UniverseMap();
                SelectedRouter.Universes.Add(range);
                SelectedRange = range;
            }, _ => SelectedRouter != null);

            DelRangeCmd = new RelayCommand(_ => {
                if (SelectedRange != null) {
                    SelectedRouter.Universes.Remove(SelectedRange);
                    SelectedRange = null;
                }
            }, _ => SelectedRange != null);

            // Chargement initial
            Load();
        }

        void Load() {
            var dto = _service.Load();
            Routers.Clear();
            foreach (var r in dto.Routers)
                Routers.Add(r);
            SelectedRouter = Routers.FirstOrDefault();
        }

        void Save() {
            var dto = new DmxSettingsDto {
                Routers = Routers.ToList()
            };
            _service.Save(dto);
        }
    }
}
