using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using Core.Dtos;
using Core.Messages;
using Core.Models;
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

            var configService = new JsonFileConfigService<AppConfigDto>("app_config.json");
            var appConfig = configService.Load();

            var patchVm = new PatchMapManagerViewModel(appConfig);
           

            var routingService = new DmxRoutingService(
                appConfig.Routers.Select(dto => DmxRouterSettings.FromDto(dto)),
                appConfig.PatchMap,
                _artNetController
            );

            var previewVm = new MatrixPreviewViewModel(_listener, routingService, patchVm);
            _previewVm = previewVm;


            // Dictionnaire d’onglets
            _panelViewModels = new Dictionary<string, BaseViewModel> {
        { "Configuration", new ConfigEditorViewModel() },
        { "Monitoring", new MonitoringDashboardViewModel(_listener) },
        { "PatchMap", patchVm },
        { "Streams", new StreamManagerViewModel() },
        { "Preview", previewVm },
        { "DMX Monitor", new DmxMonitorViewModel(_artNetController) },
         
    };
            // Routing eHub → Preview
            _listener.OnUpdatePacket += routingService.RouteUpdate;
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
