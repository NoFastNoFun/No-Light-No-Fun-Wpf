using System.Collections.ObjectModel;
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
        public ObservableCollection<string> Logs {
            get;
        }

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

            Logs = new ObservableCollection<string>();
            ApplySettingsCommand = new RelayCommand(_ => ApplySettings());
            SaveConfigCommand = new RelayCommand(_ => Save());
            LoadConfigCommand = new RelayCommand(_ => Load());

            Load();
        }

        private void ApplySettings() {
            if (SelectedPort < 1024 || SelectedPort > 65535) {
                Logs.Add($"[{DateTime.Now:HH:mm:ss}] Erreur : Port invalide ({SelectedPort})");
                return;
            }

            _listener.Stop();
            _listener.UniverseToListen = SelectedUniverse;
            _listener.Start(SelectedPort);
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Paramètres appliqués avec succès (Port : {SelectedPort}, Univers : {SelectedUniverse})");
        }

        private void Save() {
            var dto = new AppConfigDto {
                ListeningPort = SelectedPort,
                ListeningUniverse = SelectedUniverse,
                PatchMap = _patchVm.Entries.Select(e => e.ToModel()).ToList(),
                Routers = _routerVm.Routers.Select(r => r.ToDto()).ToList()
            };
            _jsonService.Save(dto);
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Paramètres sauvegardés");
        }

        private void Load() {
            var dto = _jsonService.Load();
            SelectedPort = dto.ListeningPort;
            SelectedUniverse = dto.ListeningUniverse;
            _patchVm.SetEntries(dto.PatchMap);
            _routerVm.SetRouters(dto.Routers);
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Paramètres chargés");
        }
    }
}
