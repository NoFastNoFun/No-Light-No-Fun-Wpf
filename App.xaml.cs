using System.Windows;
using Core.Dtos;
using Core.Models;

using No_Fast_No_Fun_Wpf.Services.Network;
using No_Fast_No_Fun_Wpf.ViewModels;
using Services.Config;
using Services.Matrix;

namespace No_Fast_No_Fun_Wpf {
    public partial class App : Application {
        private UdpListenerService _listener;
        private ArtNetDmxController _artNetController;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            // 1. Charge la config appli (unique source de vérité)
            var configService = new JsonFileConfigService<AppConfigDto>("app_config.json");
            var appConfig = configService.Load();

            // 2. Instancie le service ArtNet + listener UDP
            _artNetController = new ArtNetDmxController();
            _listener = new UdpListenerService();
            _listener.Start(appConfig.ListeningPort);

            // 3. Load backend-style config and create optimized routing service
            var backendConfigService = new ConfigService("config.json");
            var backendConfig = backendConfigService.Load();
            var optimizedRoutingSvc = new OptimizedDmxRoutingService(backendConfig, _artNetController);

            // 4. Instancie les ViewModels centraux (keep old ones for UI compatibility)
            var configEditorVm = new ConfigEditorViewModel(); // source de vérité
            var patchMapManagerVm = new PatchMapManagerViewModel(configEditorVm);
            
            // 5. Use optimized routing service for DMX output
            var previewVm = new MatrixPreviewViewModel(_listener, optimizedRoutingSvc, patchMapManagerVm, configEditorVm);

            // 6. Routage DMX - use optimized service
            _listener.OnUpdatePacket += optimizedRoutingSvc.RouteUpdate;
            _listener.OnUpdatePacket += previewVm.HandleUpdateMessage;

            // 7. MainWindowViewModel DI
            var mainVm = new MainWindowViewModel(_listener, _artNetController, configEditorVm, patchMapManagerVm, previewVm, appConfig, backendConfig, optimizedRoutingSvc);

            // 8. MainWindow (view) + DataContext
            var window = new MainWindow {
                DataContext = mainVm
            };
            window.Show();
        }

        protected override void OnExit(ExitEventArgs e) {
            _listener?.Stop();
            _artNetController?.Dispose();
            base.OnExit(e);
        }
    }
}
