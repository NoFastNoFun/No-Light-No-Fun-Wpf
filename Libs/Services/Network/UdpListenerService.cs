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

            // Démarre la boucle d'écoute asynchrone
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
                if (data.Length < 6) // Minimum eHuB header size
                    continue;

                // Check for eHuB protocol header
                if (data[0] != (byte)'e' || data[1] != (byte)'H' || data[2] != (byte)'u' || data[3] != (byte)'B') {
                    Debug.WriteLine($"[UDP] Invalid eHuB header: {BitConverter.ToString(data, 0, Math.Min(4, data.Length))}");
                    continue;
                }

                // Parse eHuB packet type (2 bytes after header)
                if (data.Length < 8) continue;
                
                ushort packetType = (ushort)((data[4] << 8) | data[5]);
                
                try {
                    switch (packetType) {
                        case 1: // Update message
                            if (data.Length >= 8) {
                                var updateMsg = UpdateMessage.Parse(data, 6);
                                OnUpdatePacket?.Invoke(updateMsg);
                                Debug.WriteLine($"[UDP] Received update with {updateMsg.Pixels.Count} pixels");
                            }
                            break;
                            
                        case 2: // Config message
                            if (data.Length >= 8) {
                                var configMsg = ConfigMessage.Parse(data, 6);
                                OnConfigPacket?.Invoke(configMsg);
                                Debug.WriteLine($"[UDP] Received config with {configMsg.Items.Count} items");
                            }
                            break;
                            
                        case 3: // Remote control message
                            if (data.Length >= 8) {
                                var remoteMsg = RemoteControlMessage.Parse(data, 6);
                                OnRemotePacket?.Invoke(remoteMsg);
                                Debug.WriteLine($"[UDP] Received remote control message");
                            }
                            break;
                            
                        default:
                            Debug.WriteLine($"[UDP] Unknown packet type: {packetType}");
                            break;
                    }
                }
                catch (Exception ex) {
                    Debug.WriteLine($"[UDP] Error parsing packet type {packetType}: {ex.Message}");
                }
            }
        }
    }
}
