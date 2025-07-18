using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.Dtos;
using No_Fast_No_Fun_Wpf.Services.Network;
using Services.Config;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class SystemSettingsPanelViewModel : BaseViewModel {
        private readonly UdpListenerService _listener;
        private readonly IJsonFileService<AppConfigDto> _jsonService;
        private readonly PatchMapManagerViewModel _patchVm;
        private readonly AppConfigDto _appConfig;
        public ObservableCollection<string> Logs {
            get;
        }

        private int _selectedPort;
        public int SelectedPort {
            get => _selectedPort;
            set => SetProperty(ref _selectedPort, value);
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
            AppConfigDto appConfig
        ) {
            _listener = listener;
            _patchVm = patchVm;
            _appConfig = appConfig;
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
            // Plus de gestion d'univers, on écoute tout sur ce port.
            _listener.Start(SelectedPort); 
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Paramètres appliqués avec succès (Port : {SelectedPort})");
        }

        private void Save() {
            _appConfig.ListeningPort = SelectedPort;
            _appConfig.PatchMap = _patchVm.ToDto(); // on sauvegarde ce qui est dans le patch manager
            _jsonService.Save(_appConfig);
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Paramètres sauvegardés.");
        }

        private void Load() {
            var dto = _jsonService.Load();
            SelectedPort = dto.ListeningPort;
            // Pas de chargement d'univers.
            _patchVm.SetEntries(dto.PatchMap);
            _appConfig.ListeningPort = dto.ListeningPort;
            _appConfig.PatchMap = dto.PatchMap;
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Paramètres chargés");
        }
    }
}
