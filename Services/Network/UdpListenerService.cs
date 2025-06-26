// Services/Network/UdpListenerService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace No_Fast_No_Fun_Wpf.Services.Network {
    public class UdpListenerService {
        readonly int _universe;
        UdpClient _udp;
        CancellationTokenSource _cts;

        public event Action<ConfigMessage> OnConfigPacket;
        public event Action<UpdateMessage> OnUpdatePacket;
        public event Action<RemoteControlMessage> OnRemotePacket;

        public UdpListenerService(int universe = 1) {
            _universe = universe;
        }

        public void Start(int port) {
            _cts = new CancellationTokenSource();
            _udp = new UdpClient(port);
            Task.Run(() => ListenLoop(_cts.Token));
        }

        public void Stop() {
            _cts?.Cancel();
            _udp?.Close();
        }

        async Task ListenLoop(CancellationToken ct) {
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            while (!ct.IsCancellationRequested) {
                var result = await _udp.ReceiveAsync();
                var data = result.Buffer;
                if (data.Length < 6)
                    continue;
                // header “eHuB”
                if (System.Text.Encoding.ASCII.GetString(data, 0, 4) != "eHuB")
                    continue;
                int opcode = data[4], universe = data[5];
                if (universe != _universe)
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

    public class ConfigMessage {
        public List<ConfigItem> Items { get; } = new List<ConfigItem>();

        public static ConfigMessage Parse(byte[] data, int offset) {
            var msg = new ConfigMessage();
            // nombre de ranges
            ushort count = BitConverter.ToUInt16(data, offset);
            offset += 2;
            for (int i = 0; i < count; i++) {
                ushort sIdx = BitConverter.ToUInt16(data, offset);
                offset += 2;
                ushort sId = BitConverter.ToUInt16(data, offset);
                offset += 2;
                ushort eIdx = BitConverter.ToUInt16(data, offset);
                offset += 2;
                ushort eId = BitConverter.ToUInt16(data, offset);
                offset += 2;
                msg.Items.Add(new ConfigItem(sIdx, sId, eIdx, eId));
            }
            return msg;
        }
    }

    public class UpdateMessage {
        public List<Pixel> Pixels { get; } = new List<Pixel>();

        public static UpdateMessage Parse(byte[] data, int offset) {
            // count pixels + compressed len
            ushort pixelCount = BitConverter.ToUInt16(data, offset);
            offset += 2;
            ushort compressedLen = BitConverter.ToUInt16(data, offset);
            offset += 2;

            // décompresse
            byte[] raw;
            using (var msIn = new MemoryStream(data, offset, compressedLen))
            using (var gz = new GZipStream(msIn, CompressionMode.Decompress))
            using (var msOut = new MemoryStream()) {
                gz.CopyTo(msOut);
                raw = msOut.ToArray();
            }

            var msg = new UpdateMessage();
            for (int i = 0; i < pixelCount; i++) {
                int baseIdx = i * 5;
                ushort entity = BitConverter.ToUInt16(raw, baseIdx);
                byte r = raw[baseIdx + 2];
                byte g = raw[baseIdx + 3];
                byte b = raw[baseIdx + 4];
                // on ignore le W (alpha) ici ou on peut le stocker
                msg.Pixels.Add(new Pixel(entity, r, g, b));
            }
            return msg;
        }
    }

    public class RemoteControlMessage {
        public byte CommandCode {
            get;
        }
        public string CursorName {
            get;
        }
        public string ClipName {
            get;
        }
        public byte LoopMode {
            get;
        }
        public ushort BPM {
            get;
        }

        RemoteControlMessage(byte code, string cursor, string clip, byte loop, ushort bpm) {
            CommandCode = code;
            CursorName = cursor;
            ClipName = clip;
            LoopMode = loop;
            BPM = bpm;
        }

        public static RemoteControlMessage Parse(byte[] data, int offset) {
            byte code = data[offset++];
            ushort cursorLen = BitConverter.ToUInt16(data, offset);
            offset += 2;
            string cursor = cursorLen > 0
                ? System.Text.Encoding.ASCII.GetString(data, offset, cursorLen)
                : string.Empty;
            offset += cursorLen;

            ushort clipLen = BitConverter.ToUInt16(data, offset);
            offset += 2;
            string clip = clipLen > 0
                ? System.Text.Encoding.ASCII.GetString(data, offset, clipLen)
                : string.Empty;
            offset += clipLen;

            byte loop = 0;
            ushort bpm = 0;
            if (code == 1) {
                loop = data[offset++];
                bpm = BitConverter.ToUInt16(data, offset);
            }
            return new RemoteControlMessage(code, cursor, clip, loop, bpm);
        }
    }

    public class ConfigItem {
        public ushort StartIndex {
            get;
        }
        public ushort StartId {
            get;
        }
        public ushort EndIndex {
            get;
        }
        public ushort EndId {
            get;
        }

        public ConfigItem(ushort si, ushort sid, ushort ei, ushort eid) {
            StartIndex = si;
            StartId = sid;
            EndIndex = ei;
            EndId = eid;
        }
    }

    public class Pixel {
        public ushort Entity {
            get;
        }
        public byte R, G, B;
        public Pixel(ushort e, byte r, byte g, byte b) {
            Entity = e;
            R = r;
            G = g;
            B = b;
        }
    }
}
