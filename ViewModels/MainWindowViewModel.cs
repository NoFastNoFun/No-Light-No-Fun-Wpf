using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Core.Messages;
using No_Fast_No_Fun_Wpf.Services.Network;
using Services.Config;
using Services.Matrix;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class MainWindowViewModel : BaseViewModel {
        public ObservableCollection<string> Tabs {
            get;
        }
        private readonly MatrixPreviewViewModel _previewVm;

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

        public MainWindowViewModel(UdpListenerService listener, ArtNetDmxController artNetController) {
            _listener = listener;
            _listener.UniverseToListen = selectedUniverse;
            _listener.Start(selectedPort);
            _artNetController = artNetController;

            // Services de settings
            var settingsService = new SettingsService();
            var patchVm = new PatchMapManagerViewModel();
            var routersVm = new ReceiverConfigPanelViewModel();
            var settingsVm = new SystemSettingsPanelViewModel(_listener, patchVm, routersVm);


            // Génération cohérente des univers + patch map dynamiques
            var routers = routersVm.Routers.Select(vm => vm.ToModel()).ToList();
            var patchEntries = patchVm.Entries.Select(vm => vm.ToModel()).ToList();


            var routingService = new DmxRoutingService(
                routers,
                patchEntries,
                _artNetController
            );

            var previewVm = new MatrixPreviewViewModel(_listener, routingService, patchVm);
            _previewVm = previewVm;



            // Routing eHub → DMX
            _listener.OnUpdatePacket += (UpdateMessage pkt)
                => routingService.RouteUpdate(pkt);
           

            // Dictionnaire d’onglets
            _panelViewModels = new Dictionary<string, BaseViewModel> {
        { "Settings", settingsVm },
        { "Configuration", new ConfigEditorViewModel() },
        { "Monitoring", new MonitoringDashboardViewModel(_listener) },
        { "PatchMap", patchVm },
        { "Receivers", routersVm },
        { "Streams", new StreamManagerViewModel() },
        { "Preview", previewVm },
        { "DMX Monitor", new DmxMonitorViewModel(_artNetController) },
        { "DMX Routers", routersVm }
    };
            // Routing eHub → Preview
            _listener.OnUpdatePacket += previewVm.HandleUpdateMessage;

            // Initialisation des onglets
            Tabs = new ObservableCollection<string>(_panelViewModels.Keys);
            CurrentViewModel = _panelViewModels[Tabs[0]];

            ChangeTabCommand = new RelayCommand(param => {
                if (param is string tab && _panelViewModels.TryGetValue(tab, out var vm)) {
                    CurrentViewModel = vm;

                }
            });
        }
    }
}
