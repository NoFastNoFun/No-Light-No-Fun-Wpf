using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.Messages;
using No_Fast_No_Fun_Wpf.Services.Network;
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
        readonly ArtNetDmxController _artNetController;
        UdpListenerService _listener;


        int selectedUniverse = 1;
        int selectedPort = 7777;

        object _currentViewModel;
        public object CurrentViewModel {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand ChangeTabCommand {
            get;
        }

        readonly Dictionary<string, BaseViewModel> _panelViewModels;

        public MainWindowViewModel(UdpListenerService listener,
            ArtNetDmxController artNetController) {
            _listener = listener;
            _listener.UniverseToListen = selectedUniverse;
            _listener.Start(selectedPort);
            _artNetController = artNetController;

            // Preview
            Preview = new MatrixPreviewViewModel(_listener, width: 128, height: 128);

            // Services de settings
            var settingsService = new SettingsService();
            var patchVm = new PatchMapManagerViewModel();
            var routersVm = new ReceiverConfigPanelViewModel();
            var settingsVm = new SystemSettingsPanelViewModel(_listener, patchVm, routersVm);


            // Dictionnaire d’onglets
            _panelViewModels = new Dictionary<string, BaseViewModel>
            {
                { "Settings", settingsVm },
                { "Configuration", new ConfigEditorViewModel() },
                { "Monitoring",    new MonitoringDashboardViewModel(_listener) },
                { "PatchMap", patchVm },
                { "Receivers", routersVm },
                { "Streams",       new StreamManagerViewModel() },
                { "Preview",       Preview },
                { "DMX Monitor",   new DmxMonitorViewModel(_artNetController) },
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
                routersVm.Routers.Select(vm => vm.ToModel()),
                patchEntries,
                _artNetController);


            _listener.OnUpdatePacket += (UpdateMessage pkt)
                => routingService.RouteUpdate(pkt);
        }
    }
}
