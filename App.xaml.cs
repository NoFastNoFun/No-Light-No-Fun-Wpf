using System.Windows;
using Core.Dtos;
using Core.Models;
using No_Fast_No_Fun_Wpf.Services.Network;
using No_Fast_No_Fun_Wpf.ViewModels;
using Services.Config;
using Services.Matrix;

namespace No_Fast_No_Fun_Wpf {
    public partial class App : Application {
        private UdpListenerService _listener = null!;
        private ArtNetDmxController _artNetController = null!;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            // 1. Charge la config appli (unique source de vérité sur disque)
            var configService = new JsonFileConfigService<AppConfigDto>("app_config.json");
            var appConfig = configService.Load();

            // 2. Instancie le service ArtNet + listener UDP
            _artNetController = new ArtNetDmxController();
            _listener = new UdpListenerService();
            _listener.Start(appConfig.ListeningPort);

            // 3. Instancie les ViewModels centraux
            var configEditorVm = new ConfigEditorViewModel();
            var patchMapManagerVm = new PatchMapManagerViewModel(configEditorVm);

            // 4. Génère les routeurs DYNAMIQUEMENT à partir de la config en mémoire
            var routers = BuildRoutersFromConfig(configEditorVm.ConfigItems.Select(x => x.ToModel()));

            // 5. Service de routage
            var routingSvc = new DmxRoutingService(
                routers,
                configEditorVm.ConfigItems.Select(x => x.ToModel()),
                patchMapManagerVm.Entries.Select(x => x.ToModel()),
                _artNetController
            );

            var previewVm = new MatrixPreviewViewModel(_listener, routingSvc, patchMapManagerVm, configEditorVm);

            // 6. Routage DMX
            _listener.OnUpdatePacket += routingSvc.RouteUpdate;
            _listener.OnUpdatePacket += previewVm.HandleUpdateMessage;

            // 7. MainWindowViewModel DI
            var mainVm = new MainWindowViewModel(
                _listener,
                _artNetController,
                configEditorVm,
                patchMapManagerVm,
                previewVm,
                appConfig
            );

            // 8. MainWindow (view) + DataContext
            var window = new MainWindow {
                DataContext = mainVm
            };
            window.Show();
        }

        // ...

        private List<DmxRouterSettings> BuildRoutersFromConfig(IEnumerable<ConfigItem> configItems) {
            var routers = new List<DmxRouterSettings>();
            foreach (var cfg in configItems) {
                if (string.IsNullOrEmpty(cfg.ControllerIp))
                    continue;
                var router = routers.FirstOrDefault(r => r.Ip == cfg.ControllerIp);
                if (router == null) {
                    router = new DmxRouterSettings();
                    router.Ip = cfg.ControllerIp;
                    routers.Add(router);
                }
                router.Universes.Add(new UniverseMap {
                    Universe = cfg.Universe,
                    EntityIdStart = cfg.StartEntityId,
                    EntityIdEnd = cfg.EndEntityId,
                    StartAddress = 0
                });
            }
            return routers;
        }

        protected override void OnExit(ExitEventArgs e) {
            _listener?.Stop();
            _artNetController?.Dispose();
            base.OnExit(e);
        }
    }
}

