// Services/Network/UdpListenerService.cs
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Core.Messages;

namespace No_Fast_No_Fun_Wpf.Services.Network {
    public class UdpListenerService {
        public int? _universe;
        UdpClient _udp;
        CancellationTokenSource _cts;
        public event Action<ConfigMessage> OnConfigPacket;
        public event Action<UpdateMessage> OnUpdatePacket;
        public event Action<RemoteControlMessage> OnRemotePacket;
        public static event Action<UpdateMessage>? OnInjectedMessage;
        ConcurrentQueue<UpdateMessage> _updateQueue = new();


        public int? UniverseToListen {
            get => _universe;
            set => _universe = value;
        }

        public UdpListenerService() {
        }

        public void Start(int port) {
            // Si déjà en écoute, on arrête proprement
            if (_udp != null)
                Stop();

            _cts = new CancellationTokenSource();
            _udp = new UdpClient(port);

            // Démarre la boucle d’écoute asynchrone
            _ = Task.Run(() => ListenLoop(_cts.Token));
        }

        public void Stop() {
            _cts?.Cancel();
            _udp?.Close();
            _udp = null;
        }
        public static void FakeReceive(UpdateMessage msg) => OnInjectedMessage?.Invoke(msg);

        public void SimulateUpdate(UpdateMessage msg) {
            OnUpdatePacket?.Invoke(msg);
        }

        async Task ListenLoop(CancellationToken ct) {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);

            while (!ct.IsCancellationRequested) {
                UdpReceiveResult result;
                try {
                    result = await _udp.ReceiveAsync();
                }
                catch (ObjectDisposedException) {
                    // UDP fermé, on sort
                    break;
                }
                catch (Exception) {
                    continue;
                }

                var data = result.Buffer;
                if (data.Length < 6)
                    continue;

                // Vérifie l’en-tête eHuB
                if (data[0] != (byte)'e' || data[1] != (byte)'H' || data[2] != (byte)'u' || data[3] != (byte)'B')
                continue;

                int opcode = data[4];
                int universe = data[5];
                if (_universe.HasValue && universe != _universe.Value)
                    continue;


                switch (opcode) {
                    case 1:
                        var cfg = ConfigMessage.Parse(data, 6);
                        OnConfigPacket?.Invoke(cfg);
                        break;

                    case 2:
                        var upd = UpdateMessage.Parse(data, 6);
                        OnUpdatePacket?.Invoke(upd);
                        break;

                    case 3:
                    case 4:
                    case 5:
                        var cmd = RemoteControlMessage.Parse(data, 6);
                        OnRemotePacket?.Invoke(cmd);
                        break;
                }
            }
        }
    }
}
