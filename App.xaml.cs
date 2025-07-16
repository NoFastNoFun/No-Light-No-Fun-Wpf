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

            // 1) Chargement de la config factorisée
            var config = new JsonFileConfigService<AppConfigDto>("appconfig.json").Load();

            // 2) Instanciation des services DMX
            _artNetController = new ArtNetDmxController();
            _listener = new UdpListenerService();
            _listener.Start(config.ListeningPort);

            // 3) Routage ArtNet avec DmxRoutingService
            var routingSvc = new DmxRoutingService(
                    config.Routers.Select(dto => DmxRouterSettings.FromDto(dto)), // ou une méthode équivalente
                    config.PatchMap,
                    _artNetController
            );



            _listener.OnUpdatePacket += routingSvc.RouteUpdate;

            // 4) Instanciation de la fenêtre principale avec DI
            var mainVm = new MainWindowViewModel(_listener, _artNetController);
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
