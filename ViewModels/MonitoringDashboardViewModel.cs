using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Timers;
using No_Fast_No_Fun_Wpf.Services.Network;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class MonitoringDashboardViewModel : BaseViewModel, IDisposable {
        readonly UdpListenerService _listener;
        readonly System.Timers.Timer _statsTimer;

        public ObservableCollection<string> Logs {
            get;
        }

        int _cfgCount, _updCount, _remCount;

        int _configPerSecond;
        public int ConfigPerSecond {
            get => _configPerSecond;
            private set => SetProperty(ref _configPerSecond, value);
        }

        int _updatesPerSecond;
        public int UpdatesPerSecond {
            get => _updatesPerSecond;
            private set => SetProperty(ref _updatesPerSecond, value);
        }

        int _remotePerSecond;
        public int RemotePerSecond {
            get => _remotePerSecond;
            private set => SetProperty(ref _remotePerSecond, value);
        }

        public ICommand StartCommand {
            get;
        }
        public ICommand StopCommand {
            get;
        }

        public MonitoringDashboardViewModel(UdpListenerService sharedListener) {
            Logs = new ObservableCollection<string>();
            _listener = sharedListener;

            Subscribe();

            // Timer chaque seconde
            _statsTimer = new System.Timers.Timer(1000);
            _statsTimer.Elapsed += (_, __) => {
                ConfigPerSecond = _cfgCount;
                UpdatesPerSecond = _updCount;
                RemotePerSecond = _remCount;

                // Remise à zéro pour la période suivante
                _cfgCount = _updCount = _remCount = 0;
            };

            StartCommand = new RelayCommand(_ => Start());
            StopCommand = new RelayCommand(_ => Stop());
        }

        void Subscribe() {
            _listener.OnConfigPacket += pkt => {
                _cfgCount++;
                App.Current.Dispatcher.Invoke(() =>
                    Logs.Add($"[{DateTime.Now:HH:mm:ss}] CFG ({pkt.Items.Count})"));
            };

            _listener.OnUpdatePacket += pkt => {
                _updCount++;
                App.Current.Dispatcher.Invoke(() =>
                    Logs.Add($"[{DateTime.Now:HH:mm:ss}] UPD ({pkt.Pixels.Count})"));
            };

            _listener.OnRemotePacket += pkt => {
                _remCount++;
                App.Current.Dispatcher.Invoke(() =>
                    Logs.Add($"[{DateTime.Now:HH:mm:ss}] REM (cmd={(int)pkt.CommandCode})"));
            };
        }

        void Start() {
            _listener.Start(8765);
            _statsTimer.Start();
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Monitoring démarré");
        }

        void Stop() {
            _statsTimer.Stop();
            _listener.Stop();
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Monitoring arrêté");
        }

        public void Dispose() {
            _statsTimer?.Dispose();
            _listener?.Stop();
        }
    }
}
