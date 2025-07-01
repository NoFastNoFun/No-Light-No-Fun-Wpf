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

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            // 1) Démarrage unique de l'écoute eHub UDP
            _listener = new UdpListenerService();
            _listener.Start(8765);

            // 2) Chargement de la config DMX (dmxsettings.json)
            var dmxService = new JsonFileConfigService<DmxSettingsDto>("dmxsettings.json");
            DmxSettingsDto dmxSettings = dmxService.Load();

            // 3) Création du contrôleur Art-Net avec la liste des routeurs
            _artNetController = new ArtNetDmxController();

            // 4) Branche chaque update eHub vers l’envoi DMX Art-Net
            _listener.OnUpdatePacket += packet => {
                foreach (var router in dmxSettings.Routers) {
                    // Prépare un buffer par univers mappé
                    var frames = new Dictionary<byte, byte[]>();
                    foreach (var map in router.Universes)
                        for (byte uni = map.UniverseStart; uni <= map.UniverseEnd; uni++)
                            frames[uni] = new byte[512];

                    // Remplit les buffers d’après les entités
                    foreach (var px in packet.Pixels) {
                        foreach (var map in router.Universes) {
                            if (px.Entity >= map.EntityIdStart && px.Entity <= map.EntityIdEnd) {
                                int localIndex = px.Entity - map.EntityIdStart;
                                byte uni = (byte)(map.UniverseStart + (localIndex / 170));
                                int channel = (localIndex % 170) * 3;

                                var buf = frames[uni];
                                buf[channel + 0] = px.R;
                                buf[channel + 1] = px.G;
                                buf[channel + 2] = px.B;
                                break;
                            }
                        }
                    }


                    // Envoie toutes les trames univers par univers
                    foreach (var kv in frames) {
                        byte universe = kv.Key;
                        byte[] data = kv.Value;
                        _artNetController.SendDmxFrame(router.Ip, router.Port, universe, data);
                    }
                }
            };

            // 5) Instanciation et affichage de la fenêtre principale
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
