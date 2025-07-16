using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using No_Fast_No_Fun_Wpf.Services.Network;
using Core.Messages;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class EntityState {
        public int EntityId {
            get; set;
        }
        public byte R {
            get; set;
        }
        public byte G {
            get; set;
        }
        public byte B {
            get; set;
        }
        public DateTime LastUpdate {
            get; set;
        }
        public override string ToString() =>
            $"Entity {EntityId}: R={R} G={G} B={B} @ {LastUpdate:HH:mm:ss.fff}";
    }

    public class MonitoringDashboardViewModel : BaseViewModel, IDisposable {
        readonly UdpListenerService _listener;
        readonly Dictionary<int, EntityState> _entityStates;

        public ObservableCollection<EntityState> EntityStates {
            get;
        }
        public ObservableCollection<string> Logs {
            get;
        }

        public ICommand StartCommand {
            get;
        }
        public ICommand StopCommand {
            get;
        }
        public ICommand ClearLogsCommand {
            get;
        }
        public ICommand ClearEntitiesCommand {
            get;
        }

        bool _running = false;

        public MonitoringDashboardViewModel(UdpListenerService sharedListener) {
            _listener = sharedListener;
            _entityStates = new Dictionary<int, EntityState>();
            EntityStates = new ObservableCollection<EntityState>();
            Logs = new ObservableCollection<string>();

            StartCommand = new RelayCommand(_ => Start());
            StopCommand = new RelayCommand(_ => Stop());
            ClearLogsCommand = new RelayCommand(_ => Logs.Clear());
            ClearEntitiesCommand = new RelayCommand(_ => {
                EntityStates.Clear();
                _entityStates.Clear();
            });
        }

        void Start() {
            if (_running)
                return;
            _listener.OnUpdatePacket += OnUpdatePacket;
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Monitoring démarré");
            _running = true;
        }

        void Stop() {
            if (!_running)
                return;
            _listener.OnUpdatePacket -= OnUpdatePacket;
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] Monitoring arrêté");
            _running = false;
        }

        void OnUpdatePacket(Core.Messages.UpdateMessage msg) {
            var now = DateTime.Now;
            foreach (var px in msg.Pixels) {
                if (_entityStates.TryGetValue(px.Entity, out var state)) {
                    state.R = px.R;
                    state.G = px.G;
                    state.B = px.B;
                    state.LastUpdate = now;
                }
                else {
                    state = new EntityState {
                        EntityId = px.Entity,
                        R = px.R,
                        G = px.G,
                        B = px.B,
                        LastUpdate = now
                    };
                    _entityStates[px.Entity] = state;
                    App.Current.Dispatcher.Invoke(() => EntityStates.Add(state));
                }
                // Optionnel : log chaque update (à commenter si trop verbeux)
                App.Current.Dispatcher.Invoke(() =>
                    Logs.Add($"[{now:HH:mm:ss.fff}] Entity {px.Entity}: R={px.R} G={px.G} B={px.B}"));
            }
        }

        public void Dispose() {
            Stop();
        }
    }
}
