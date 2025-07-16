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

            // 3. Instancie les ViewModels centraux
            var configEditorVm = new ConfigEditorViewModel(); // source de vérité
            var patchMapManagerVm = new PatchMapManagerViewModel(configEditorVm);
            var routingSvc = new DmxRoutingService(
                                appConfig.Routers.Select(DmxRouterSettings.FromDto),
                                configEditorVm.ConfigItems.Select(x => x.ToModel()),    
                                patchMapManagerVm.Entries.Select(x => x.ToModel()),     
                                _artNetController
                                );
            var previewVm = new MatrixPreviewViewModel(_listener, routingSvc, patchMapManagerVm, configEditorVm);

            // 4. Routage DMX
            _listener.OnUpdatePacket += routingSvc.RouteUpdate;
            _listener.OnUpdatePacket += previewVm.HandleUpdateMessage;

            // 5. MainWindowViewModel DI
            var mainVm = new MainWindowViewModel(_listener, _artNetController, configEditorVm, patchMapManagerVm, previewVm, appConfig);

            // 6. MainWindow (view) + DataContext
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
