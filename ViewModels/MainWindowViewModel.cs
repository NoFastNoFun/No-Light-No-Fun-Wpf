using No_Fast_No_Fun_Wpf.Services.Network;
using Services.Config;
using Services.Matrix;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class MainWindowViewModel : BaseViewModel {
        public ObservableCollection<string> Tabs {
            get;
        }

        readonly UdpListenerService _listener;
        readonly ArtNetDmxController _artNetController;


        private object _currentViewModel;
        public object CurrentViewModel {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand ChangeTabCommand {
            get;
        }

        readonly Dictionary<string, BaseViewModel> _panelViewModels;

        public MainWindowViewModel(UdpListenerService listener , ArtNetDmxController artNetController) {
            var settings = new SettingsService();
            _listener = listener;
            _artNetController = artNetController;

            _listener.OnConfigPacket += pkt => {
                // Logique de traitement des paquets de configuration
            };
            _listener.OnUpdatePacket += pkt => {
                // Logique de traitement des paquets de mise à jour
            };
            _listener.OnRemotePacket += pkt => {
                // Logique de traitement des paquets de contrôle à distance
            };

            _panelViewModels = new Dictionary<string, BaseViewModel>
            {
                { "Configuration", new ConfigEditorViewModel() },
                { "Monitoring",    new MonitoringDashboardViewModel(_listener) },
                { "PatchMap",      new PatchMapManagerViewModel() },
                { "Receivers",     new ReceiverConfigPanelViewModel(settings) },
                { "Streams",       new StreamManagerViewModel() },
                { "Settings",      new SystemSettingsPanelViewModel() },
                { "DMX Monitor", new DmxMonitorViewModel(artNetController) }

            };

            Tabs = new ObservableCollection<string>(_panelViewModels.Keys);
            CurrentViewModel = _panelViewModels[Tabs[0]];

            ChangeTabCommand = new RelayCommand(param => {
                if (param is string tab && _panelViewModels.ContainsKey(tab))
                    CurrentViewModel = _panelViewModels[tab];
            });
        }
    }
}
