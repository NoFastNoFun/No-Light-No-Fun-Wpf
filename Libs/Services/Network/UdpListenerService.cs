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
                    Debug.WriteLine("[UDP] Socket closed, exiting listen loop.");
                    break;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"[UDP] Exception: {ex.Message}");
                    continue;
                }

                var data = result.Buffer;
                Debug.WriteLine($"[UDP] Packet received: {data.Length} bytes from {result.RemoteEndPoint}");
                if (data.Length < 6) {
                    Debug.WriteLine("[UDP] Packet too short, ignored.");
                    continue;
                }

                // Vérifie l’en-tête eHuB
                if (data[0] != (byte)'e' || data[1] != (byte)'H' || data[2] != (byte)'u' || data[3] != (byte)'B') {
                    Debug.WriteLine("[UDP] Invalid eHuB header, ignored.");
                    continue;
                }

                int opcode = data[4];
                int universe = data[5];
                if (_universe.HasValue && universe != _universe.Value) {
                    Debug.WriteLine($"[UDP] Universe mismatch (got {universe}, expected {_universe.Value}), ignored.");
                    continue;
                }

                Debug.WriteLine($"[UDP] eHuB packet: opcode={opcode}, universe={universe}");

                switch (opcode) {
                    case 1:
                        Debug.WriteLine("[UDP] Parsing ConfigMessage");
                        var cfg = ConfigMessage.Parse(data, 6);
                        OnConfigPacket?.Invoke(cfg);
                        break;

                    case 2:
                        Debug.WriteLine("[UDP] Parsing UpdateMessage");
                        var upd = UpdateMessage.Parse(data, 6);
                        OnUpdatePacket?.Invoke(upd);
                        break;

                    case 3:
                    case 4:
                    case 5:
                        Debug.WriteLine("[UDP] Parsing RemoteControlMessage");
                        var cmd = RemoteControlMessage.Parse(data, 6);
                        OnRemotePacket?.Invoke(cmd);
                        break;
                    default:
                        Debug.WriteLine($"[UDP] Unknown opcode {opcode}, ignored.");
                        break;
                }
            }
        }
    }
}
