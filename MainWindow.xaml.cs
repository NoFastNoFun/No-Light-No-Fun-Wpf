using System;
using System.Windows;
using No_Fast_No_Fun_Wpf.Services.Network;
using No_Fast_No_Fun_Wpf.Services.Config;
using No_Fast_No_Fun_Wpf.Core.Messages;
using Services.Config;

namespace No_Fast_No_Fun_Wpf {
    public partial class MainWindow : Window {
        readonly SettingsService _settings;
        UdpListenerService _listener;

        public MainWindow() {
            InitializeComponent();

            // Init des settings et abonnement au changement
            _settings = new SettingsService();
            _settings.SettingsChanged += OnSettingsChanged;

            // Création initiale du listener selon les paramètres chargés
            _listener = new UdpListenerService(_settings.Settings.Universe);
            SubscribeListenerEvents();

            // Démarrage immédiat de l'écoute sur le port configuré
            Log($"Démarrage de l’écoute UDP sur port {_settings.Settings.Port}, universe {_settings.Settings.Universe}");
            _listener.Start(_settings.Settings.Port);
            StartButton.IsEnabled = false;
        }

        // Abonne les callbacks une bonne fois pour toutes
        void SubscribeListenerEvents() {
            _listener.OnConfigPacket += cfg => Dispatcher.Invoke(() =>
                Log($"Config reçue : {cfg.Items.Count} séquences"));

            _listener.OnUpdatePacket += upd => Dispatcher.Invoke(() =>
                Log($"Update reçue : {upd.Pixels.Count} pixels"));

            _listener.OnRemotePacket += cmd => Dispatcher.Invoke(() =>
                Log($"Remote reçue : op={(int)cmd.CommandCode}, clip='{cmd.ClipName}'"));
        }

        // Gère le click manuel (si tu souhaites relancer via bouton)
        void StartButton_Click(object sender, RoutedEventArgs e) {
            StartButton.IsEnabled = false;
            Log($"Démarrage manuel de l’écoute UDP sur port {_settings.Settings.Port}");
            _listener.Start(_settings.Settings.Port);
        }

        // Lorsqu’on change les settings (via ton SettingsService), on redémarre le listener
        void OnSettingsChanged(Models.Settings newSettings) {
            Dispatcher.Invoke(() => {
                Log($"Paramètres modifiés : port={newSettings.Port}, universe={newSettings.Universe}. Redémarrage…");
                _listener.Stop();
                _listener = new UdpListenerService(newSettings.Universe);
                SubscribeListenerEvents();
                _listener.Start(newSettings.Port);
            });
        }

        // Log helper pour centraliser l’ajout au TextBox
        void Log(string message) {
            LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            LogBox.ScrollToEnd();
        }

        protected override void OnClosed(EventArgs e) {
            _listener.Stop();
            base.OnClosed(e);
        }
    }
}
