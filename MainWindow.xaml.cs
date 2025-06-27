using System;
using System.Windows;
using Core.Messages;
using No_Fast_No_Fun_Wpf.Services.Config;
using No_Fast_No_Fun_Wpf.Services.Network;
using Services.Config;

namespace No_Fast_No_Fun_Wpf {
    public partial class MainWindow : Window {
        readonly SettingsService _settings;
        UdpListenerService _listener;

        public MainWindow() {
            InitializeComponent();

            _settings = new SettingsService();
            _settings.SettingsChanged += OnSettingsChanged;

            // Démarrage initial de la boucle UDP
            _listener = new UdpListenerService(_settings.Settings.Universe);
            _listener.Start(_settings.Settings.Port);
        }

        void OnSettingsChanged(ReceiverSettings newSettings) {
            // Redémarre proprement l’écoute avec les nouveaux paramètres
            _listener.Stop();
            _listener = new UdpListenerService(newSettings.Universe);
            _listener.Start(newSettings.Port);
        }

        protected override void OnClosed(EventArgs e) {
            _listener.Stop();
            base.OnClosed(e);
        }
    }
}
