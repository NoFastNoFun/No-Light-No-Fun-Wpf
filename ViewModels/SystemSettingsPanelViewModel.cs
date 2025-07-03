using System.Linq;
using System.Windows.Input;
using Core.Dtos;
using No_Fast_No_Fun_Wpf.Services.Network;
using Services.Config;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class SystemSettingsPanelViewModel : BaseViewModel {
        private readonly UdpListenerService _listener;
        private readonly IJsonFileService<AppConfigDto> _jsonService;
        private readonly PatchMapManagerViewModel _patchVm;
        private readonly ReceiverConfigPanelViewModel _routerVm;

        private int _selectedPort;
        public int SelectedPort {
            get => _selectedPort;
            set => SetProperty(ref _selectedPort, value);
        }

        private int _selectedUniverse;
        public int SelectedUniverse {
            get => _selectedUniverse;
            set => SetProperty(ref _selectedUniverse, value);
        }

        public ICommand ApplySettingsCommand {
            get;
        }
        public ICommand SaveConfigCommand {
            get;
        }
        public ICommand LoadConfigCommand {
            get;
        }

        public SystemSettingsPanelViewModel(
            UdpListenerService listener,
            PatchMapManagerViewModel patchVm,
            ReceiverConfigPanelViewModel routerVm
        ) {
            _listener = listener;
            _patchVm = patchVm;
            _routerVm = routerVm;
            _jsonService = new JsonFileConfigService<AppConfigDto>("app_config.json");

            ApplySettingsCommand = new RelayCommand(_ => ApplySettings());
            SaveConfigCommand = new RelayCommand(_ => Save());
            LoadConfigCommand = new RelayCommand(_ => Load());

            Load();
        }

        private void ApplySettings() {
            _listener.Stop();
            _listener.UniverseToListen = SelectedUniverse;
            _listener.Start(SelectedPort);
        }

        private void Save() {
            var dto = new AppConfigDto {
                PatchMap = _patchVm.Entries.Select(vm => vm.ToModel()).ToList(),
                Routers = _routerVm.Routers.Select(vm => vm.ToDto()).ToList(),
                ListeningPort = SelectedPort,
                ListeningUniverse = SelectedUniverse
            };
            _jsonService.Save(dto);
        }

        private void Load() {
            var dto = _jsonService.Load();
            _patchVm.Entries.Clear();
            foreach (var p in dto.PatchMap)
                _patchVm.Entries.Add(new PatchMapEntryViewModel(p));

            _routerVm.Routers.Clear();
            foreach (var r in dto.Routers)
                _routerVm.Routers.Add(new DmxRouterSettingsViewModel(r));

            SelectedPort = dto.ListeningPort;
            SelectedUniverse = dto.ListeningUniverse;
        }
    }
}
