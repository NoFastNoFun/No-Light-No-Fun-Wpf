using Core.Models;
using ConfigModel = Core.Models.Config;
using Core.Messages;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Services.Matrix
{
    public class OptimizedDmxRoutingService : IDmxRoutingService
    {
        private readonly List<MapEntry> _mapping;
        private readonly List<Patch> _patches;
        private readonly ArtNetDmxController _artNet;
        private readonly ConcurrentDictionary<(string ip, byte universe), byte[]> _buffers;

        public OptimizedDmxRoutingService(ConfigModel config, ArtNetDmxController artNet)
        {
            _mapping = config.Mapping;
            _patches = config.Patch;
            _artNet = artNet;
            _buffers = new ConcurrentDictionary<(string ip, byte universe), byte[]>();
        }

        public void RouteUpdate(UpdateMessage packet)
        {
            _buffers.Clear();

            int pixelsRoutés = 0;
            int pixelsIgnorés = 0;

            // Build state map (latest color per entity) - like backend
            var state = new Dictionary<uint, (byte r, byte g, byte b)>();
            foreach (var pix in packet.Pixels)
            {
                state[(uint)pix.Entity] = (pix.R, pix.G, pix.B);
            }

            // Build frames map (ip -> universe -> DMX[]) - like backend
            var frames = new Dictionary<string, Dictionary<byte, byte[]>>();

            foreach (var map in _mapping)
            {
                if (!map.Enable)
                    continue;

                if (!state.TryGetValue(map.Entity, out var color))
                    continue;

                // Select RGB values based on SelectRGBW
                byte[] values;
                switch (map.SelectRGBW)
                {
                    case "R":
                        values = new byte[] { color.r };
                        break;
                    case "G":
                        values = new byte[] { color.g };
                        break;
                    case "B":
                        values = new byte[] { color.b };
                        break;
                    default: // RGB
                        values = new byte[] { color.r, color.g, color.b };
                        break;
                }

                // Get or create frame buffer
                if (!frames.ContainsKey(map.Controller))
                    frames[map.Controller] = new Dictionary<byte, byte[]>();

                var universeMap = frames[map.Controller];
                if (!universeMap.ContainsKey(map.Universe))
                    universeMap[map.Universe] = new byte[512]; // DMX size

                var buf = universeMap[map.Universe];
                
                // Copy values to buffer
                if (map.Channel - 1 + values.Length <= buf.Length)
                {
                    Array.Copy(values, 0, buf, map.Channel - 1, values.Length);
                    pixelsRoutés++;
                }
                else
                {
                    pixelsIgnorés++;
                }
            }

            // Apply patch-map (like backend)
            foreach (var patch in _patches)
            {
                foreach (var universeMap in frames.Values)
                {
                    foreach (var buf in universeMap.Values)
                    {
                        if (patch.From <= buf.Length && patch.To <= buf.Length)
                        {
                            buf[patch.To - 1] = buf[patch.From - 1];
                        }
                    }
                }
            }

            // Send frames (like backend)
            foreach (var (ip, universeMap) in frames)
            {
                foreach (var (universe, data) in universeMap)
                {
                    _artNet.SendDmxFrame(ip, 6454, universe, data);
                }
            }

            Debug.WriteLine($"[Optimized] Pixels routés = {pixelsRoutés}, ignorés = {pixelsIgnorés}, total = {packet.Pixels.Count}");
        }
    }
} 