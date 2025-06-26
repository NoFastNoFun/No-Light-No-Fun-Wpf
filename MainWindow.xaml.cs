using System;
using System.Windows;
using No_Fast_No_Fun_Wpf.Services.Network;
using No_Fast_No_Fun_Wpf.Core.Messages;

namespace No_Fast_No_Fun_Wpf {
    public partial class MainWindow : Window {
        readonly UdpListenerService _listener;

        public MainWindow() {
            InitializeComponent();

            // Instancie le listener (universe = 1 par défaut)
            _listener = new UdpListenerService();

            // Abonne les callbacks
            _listener.OnConfigPacket += cfg => Dispatcher.Invoke(() =>
                LogBox.AppendText(
                    $"[{DateTime.Now:HH:mm:ss}] Config reçue : {cfg.Items.Count} séquences\n"));

            _listener.OnUpdatePacket += upd => Dispatcher.Invoke(() =>
                LogBox.AppendText(
                    $"[{DateTime.Now:HH:mm:ss}] Update reçue : {upd.Pixels.Count} pixels\n"));

            _listener.OnRemotePacket += cmd => Dispatcher.Invoke(() =>
                LogBox.AppendText(
                    $"[{DateTime.Now:HH:mm:ss}] Remote reçue : op={(int)cmd.CommandCode}, clip='{cmd.ClipName}'\n"));
        }

        // Attaché au Click de StartButton
        void StartButton_Click(object sender, RoutedEventArgs e) {
            StartButton.IsEnabled = false;
            LogBox.AppendText(
                $"[{DateTime.Now:HH:mm:ss}] Démarrage de l’écoute UDP sur le port 8765…\n");

            // Lancement de la boucle d’écoute
            _listener.Start(8765);
        }

        // Arrêt propre à la fermeture de la fenêtre
        protected override void OnClosed(EventArgs e) {
            _listener.Stop();
            base.OnClosed(e);
        }
    }
}
