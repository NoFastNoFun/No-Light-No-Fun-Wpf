using System;
using System.Windows;
using Core.Messages;
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

        }

        void OnSettingsChanged(ReceiverSettings newSettings) {
            _listener.Stop();
            _listener.Start(newSettings.Port);
        }

        protected override void OnClosed(EventArgs e) {
            _listener.Stop();
            base.OnClosed(e);
        }
    }
}
