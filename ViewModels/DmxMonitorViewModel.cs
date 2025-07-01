using System.Collections.ObjectModel;
using System.Windows.Input;
using Services.Matrix;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class DmxMonitorViewModel : BaseViewModel, IDisposable {
        readonly ArtNetDmxController _artNet;
        readonly System.Timers.Timer _statsTimer;

        public ObservableCollection<string> Logs {
            get;
        }
        public int FramesPerSecond {
            get; private set;
        }
        public int OctetsPerSecond {
            get; private set;
        }

        int _frameCount, _octetCount;

        public ICommand StartForwarding {
            get;
        }
        public ICommand StopForwarding {
            get;
        }

        public DmxMonitorViewModel(ArtNetDmxController artNet) {
            _artNet = artNet;
            Logs = new ObservableCollection<string>();

            // Timer pour remettre à zéro chaque seconde
            _statsTimer = new System.Timers.Timer(1000);
            _statsTimer.Elapsed += (_, __) => {
                FramesPerSecond = _frameCount;
                OctetsPerSecond = _octetCount;
                OnPropertyChanged(nameof(FramesPerSecond));
                OnPropertyChanged(nameof(OctetsPerSecond));
                _frameCount = _octetCount = 0;
            };

            StartForwarding = new RelayCommand(_ => {
                _artNet.FrameSent += OnFrameSent;
                _statsTimer.Start();
                Logs.Add($"[{DateTime.Now:HH:mm:ss}] Forwarding DMX started");
            });

            StopForwarding = new RelayCommand(_ => {
                _artNet.FrameSent -= OnFrameSent;
                _statsTimer.Stop();
                Logs.Add($"[{DateTime.Now:HH:mm:ss}] Forwarding DMX stopped");
            });
        }

        void OnFrameSent(string ip, byte universe, int length) {
            _frameCount++;
            _octetCount += length;
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Sent {length}B → {ip} U{universe}");
        }

        public void Dispose() {
            _statsTimer?.Stop();
            _artNet.FrameSent -= OnFrameSent;
        }
    }
}
