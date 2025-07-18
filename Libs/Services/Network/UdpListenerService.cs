// Services/Network/UdpListenerService.cs
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media.Media3D;
using Core.Messages;
using Core.Models;


namespace No_Fast_No_Fun_Wpf.Services.Network {
    public class UdpListenerService {
        public int? _universe;
        UdpClient _udp;
        CancellationTokenSource _cts;
        public event Action<ConfigMessage> OnConfigPacket;
        public event Action<UpdateMessage> OnUpdatePacket;
        public event Action<RemoteControlMessage> OnRemotePacket;
        public static event Action<UpdateMessage>? OnInjectedMessage;
        public List<int> _unityIndexToId {
            get; set;
        }
        public Dictionary<int, Point3D> _entityMap {
            get; set;
        }


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

                    break;
                }
                catch (Exception ex) {
                    Debug.WriteLine($"[UDP] Exception: {ex.Message}");
                    continue;
                }
                var data = result.Buffer;


                if (data.Length < 6) {
                    continue;
                }

                // Vérifie l’en-tête e      HuB
                if (data[0] != (byte)'e' || data[1] != (byte)'H' || data[2] != (byte)'u' || data[3] != (byte)'B') {
                                   continue;
                }

                int opcode = data[4];
                int universe = data[5];


                if (_universe.HasValue && universe != _universe.Value) {
                  
                    continue;
                }

                switch (opcode) {
                    case 1: // Config
                        try {
                            var cfg = ConfigMessage.Parse(data, 6);
                            OnConfigPacket?.Invoke(cfg);
                        }
                        catch (Exception ex) {
                            Debug.WriteLine($"[UDP] Config parse failed: {ex}");
                        }
                        break;

                    case 2:
                        try {
                            int pixelCount = BitConverter.ToUInt16(data, 6);
                            int compressedLen = BitConverter.ToUInt16(data, 8);
                            int payloadOffset = 10;
                            if (payloadOffset + compressedLen > data.Length) {
                                Debug.WriteLine("[UDP] GZIP payload dépasse la taille du buffer !");
                                break;
                            }
                            using var ms = new MemoryStream(data, payloadOffset, compressedLen);
                            using var gzip = new GZipStream(ms, CompressionMode.Decompress);
                            using var decompressed = new MemoryStream();
                            gzip.CopyTo(decompressed);
                            byte[] uncompressed = decompressed.ToArray();

                            var upd = UpdateMessage.Parse(uncompressed, 0);
                            OnUpdatePacket?.Invoke(upd);
                        }
                        catch (Exception ex) {
                            Debug.WriteLine($"[UDP] Update parse failed: {ex.Message}");
                        }
                        break;


                    case 99: // (Supposons) Unity RGB raw packet
                        try {
                            if (_unityIndexToId == null || _entityMap == null) {
                                Debug.WriteLine("[UDP] Unity mapping non initialisé !");
                                break;
                            }
                            int baseOffset = 6;
                            int pixelCount = (data.Length - baseOffset) / 3;
                            var pixels = new List<Pixel>();
                            for (int i = 0; i < pixelCount; i++) {
                                int idx = baseOffset + i * 3;
                                if (idx + 3 > data.Length)
                                    break;
                                byte r = data[idx];
                                byte g = data[idx + 1];
                                byte b = data[idx + 2];
                                if (i < _unityIndexToId.Count) {
                                    var entityId = _unityIndexToId[i];
                                    if (_entityMap.ContainsKey(entityId)) {
                                        pixels.Add(new Pixel((ushort)entityId, r, g, b));
                                    }
                                }
                            }
                            var msg = new UpdateMessage(pixels);
                            OnUpdatePacket?.Invoke(msg);
                        }
                        catch (Exception ex) {
                            Debug.WriteLine($"[UDP] Unity update parse failed: {ex}");
                        }
                        break;

                    case 3: // Remote control
                        try {
                            Debug.WriteLine("[UDP] Remote control packet received.");
                            var cmd = RemoteControlMessage.Parse(data, 6);
                            OnRemotePacket?.Invoke(cmd);
                        }
                        catch (Exception ex) {
                            Debug.WriteLine($"[UDP] Remote parse failed: {ex}");
                        }
                        break;

                    default:
                        Debug.WriteLine($"[UDP] Unknown opcode {opcode}.");

                        break;
                    default:
                        Debug.WriteLine($"[UDP] Unknown opcode {opcode}, ignored.");
                        break;
                }


            }
        }
    }
}
    

