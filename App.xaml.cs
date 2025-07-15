using System.Windows;
using Core.Dtos;
using Core.Models;        
using Services.Config;     
using Services.Matrix;     
using No_Fast_No_Fun_Wpf.Services.Network;  
using No_Fast_No_Fun_Wpf.ViewModels;        

namespace No_Fast_No_Fun_Wpf {
    public partial class App : Application {
        private UdpListenerService _listener;
        private ArtNetDmxController _artNetController;
        private DmxRoutingService _routingService;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            // 1) Démarrage unique de l'écoute eHub UDP
            _listener = new UdpListenerService();
            _listener.Start(8765);

            // 2) Chargement de la config DMX (dmxsettings.json)
            var dmxService = new JsonFileConfigService<DmxSettingsDto>("dmxsettings.json");
            DmxSettingsDto dmxSettings = dmxService.Load();

            // 3) Création du contrôleur Art-Net
            _artNetController = new ArtNetDmxController();

            // 4) Création du service de routage DMX
            var routers = dmxSettings.Routers.Select(r => new DmxRouterSettings {
                Ip = r.Ip,
                Port = r.Port,
                Universes = r.Universes.Select(u => new UniverseMap {
                    EntityIdStart = u.EntityStart,
                    EntityIdEnd = u.EntityEnd,
                    UniverseStart = u.UniverseStart,
                    UniverseEnd = u.UniverseEnd,
                    StartAddress = u.StartAddress
                }).ToList()
            }).ToList();

            var patches = new List<PatchMapEntryDto>(); // Empty for now, can be loaded from file if needed

            _routingService = new DmxRoutingService(routers, patches, _artNetController);

            // 5) Branche chaque update eHub vers le service de routage
            _listener.OnUpdatePacket += packet => {
                _routingService.RouteUpdate(packet);
            };

            // 6) Instanciation et affichage de la fenêtre principale
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
