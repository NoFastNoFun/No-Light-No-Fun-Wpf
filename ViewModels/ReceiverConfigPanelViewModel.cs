using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.Dtos;
using Services.Config;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class ReceiverConfigPanelViewModel : BaseViewModel {
        readonly IJsonFileService<DmxSettingsDto> _service;

        public ObservableCollection<DmxRouterSettingsViewModel> Routers {
            get;
        }

        public ObservableCollection<UniverseMapViewModel> Universes {
            get;
        }

        DmxRouterSettingsViewModel? _selectedRouter;
        public DmxRouterSettingsViewModel? SelectedRouter {
            get => _selectedRouter;
            set {
                if (SetProperty(ref _selectedRouter, value)) {
                    (AddRangeCmd as RelayCommand)?.RaiseCanExecuteChanged();
                    (DelRouterCmd as RelayCommand)?.RaiseCanExecuteChanged();
                    SelectedRange = null;
                    ReloadUniverses();
                }
            }
        }

        UniverseMapViewModel? _selectedRange;
        public UniverseMapViewModel? SelectedRange {
            get => _selectedRange;
            set {
                if (SetProperty(ref _selectedRange, value)) {
                    (DelRangeCmd as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

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
            Routers = new ObservableCollection<DmxRouterSettingsViewModel>();
            Universes = new ObservableCollection<UniverseMapViewModel>();

            LoadCommand = new RelayCommand(_ => Load());
            SaveCommand = new RelayCommand(_ => Save());

            AddRouterCmd = new RelayCommand(_ => {
                var routerVm = new DmxRouterSettingsViewModel(new DmxRouterSettingsDto());
                Routers.Add(routerVm);
                SelectedRouter = routerVm;
            });


            DelRouterCmd = new RelayCommand(_ => {
                if (SelectedRouter != null) {
                    Routers.Remove(SelectedRouter);
                    SelectedRouter = Routers.FirstOrDefault();
                }
            }, _ => SelectedRouter != null);

            AddRangeCmd = new RelayCommand(_ => {
                if (SelectedRouter != null) {
                    var model = new Core.Models.UniverseMap();
                    var vm = new UniverseMapViewModel(model);
                    SelectedRouter.Universes.Add(vm);
                    Universes.Add(vm);
                    SelectedRange = vm;
                }
            }, _ => SelectedRouter != null);

            DelRangeCmd = new RelayCommand(_ => {
                if (SelectedRouter != null && SelectedRange != null) {
                    SelectedRouter.Universes.Remove(SelectedRange);
                    Universes.Remove(SelectedRange);
                    SelectedRange = null;
                }
            }, _ => SelectedRange != null);

            Load();
        }

        void Load() {
            var dto = _service.Load();
            Routers.Clear();
            foreach (var r in dto.Routers)
                Routers.Add(new DmxRouterSettingsViewModel(r));
            SelectedRouter = Routers.FirstOrDefault();
        }

        void Save() {
            var dto = new DmxSettingsDto {
                Routers = Routers.Select(r => r.ToDto()).ToList()
            };
            _service.Save(dto);
        }

        void ReloadUniverses() {
            Universes.Clear();
            if (SelectedRouter == null)
                return;

            foreach (var vm in SelectedRouter.Universes)
                Universes.Add(vm);
        }
    }
}
