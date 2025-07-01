using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using No_Fast_No_Fun_Wpf.Services.Network;
using No_Fast_No_Fun_Wpf.Core.Messages;
using Services.Config;
using Services.Matrix;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class MainWindowViewModel : BaseViewModel {
        public ObservableCollection<string> Tabs {
            get;
        }

        public MatrixPreviewViewModel Preview {
            get;
        }

        readonly UdpListenerService _listener;
        readonly ArtNetDmxController _artNetController;

        object _currentViewModel;
        public object CurrentViewModel {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand ChangeTabCommand {
            get;
        }

        readonly Dictionary<string, BaseViewModel> _panelViewModels;

        public MainWindowViewModel(
            UdpListenerService listener,
            ArtNetDmxController artNetController) {
            _listener = listener;
            _artNetController = artNetController;

            // Preview
            Preview = new MatrixPreviewViewModel(_listener, width: 128, height: 128);

            // Services de settings
            var settingsService = new SettingsService();
            var receiversVm = new ReceiverConfigPanelViewModel(settingsService);
            var routersVm = new ReceiverConfigPanelViewModel(settingsService);

            // Dictionnaire d’onglets
            _panelViewModels = new Dictionary<string, BaseViewModel>
            {
                { "Configuration", new ConfigEditorViewModel() },
                { "Monitoring",    new MonitoringDashboardViewModel(_listener) },
                { "PatchMap",      new PatchMapManagerViewModel() },
                { "Receivers",     receiversVm },
                { "Streams",       new StreamManagerViewModel() },
                { "Settings",      new SystemSettingsPanelViewModel() },
                { "DMX Monitor",   new DmxMonitorViewModel(_artNetController) },
                { "Preview",       Preview },
                { "DMX Routers",   routersVm }
            };

            // Initialisation des onglets
            Tabs = new ObservableCollection<string>(_panelViewModels.Keys);
            CurrentViewModel = _panelViewModels[Tabs[0]];

            ChangeTabCommand = new RelayCommand(param => {
                if (param is string tab && _panelViewModels.TryGetValue(tab, out var vm))
                    CurrentViewModel = vm;
            });

            // Routage eHub → DMX
            var patchEntries = ((PatchMapManagerViewModel)_panelViewModels["PatchMap"])
                                .Entries
                                .Select(vm => vm.ToModel());

            var routingService = new DmxRoutingService(
                routersVm.Routers,
                patchEntries,
                _artNetController);

            _listener.OnUpdatePacket += (UpdateMessage pkt)
                => routingService.RouteUpdate(pkt);
        }
    }
}
