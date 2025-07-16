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

        public MatrixPreviewViewModel Preview => _previewVm;
        private readonly ArtNetDmxController _artNetController;
        private UdpListenerService _listener;

        private object _currentViewModel;
        public object CurrentViewModel {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand ChangeTabCommand {
            get;
        }
        private readonly Dictionary<string, BaseViewModel> _panelViewModels;
        private readonly MatrixMonitoringViewModel _monitoringVm;
        public MatrixMonitoringViewModel Monitoring => _monitoringVm;

        public MainWindowViewModel(
            UdpListenerService listener,
            ArtNetDmxController artNetController,
            ConfigEditorViewModel configEditorVm,
            PatchMapManagerViewModel patchMapManagerVm,
            MatrixPreviewViewModel previewVm,
            AppConfigDto appConfig,
            ConfigModel backendConfig,
            IDmxRoutingService routingService
        ) {
            _listener = listener;
            _artNetController = artNetController;
            _previewVm = previewVm;
            _monitoringVm = new MatrixMonitoringViewModel(routingService, backendConfig, _listener);

            _panelViewModels = new Dictionary<string, BaseViewModel>
            {
                    { "System Settings", new SystemSettingsPanelViewModel(_listener, patchMapManagerVm, appConfig) },
                    { "Configuration", configEditorVm },
                    { "Monitoring", new MonitoringDashboardViewModel(_listener) },
                    { "PatchMap", patchMapManagerVm },
                    { "Streams", new StreamManagerViewModel() },
                    { "Preview", previewVm },
                    { "Matrix Monitor", _monitoringVm },
                    { "DMX Monitor", new DmxMonitorViewModel(_artNetController) },
                };

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
