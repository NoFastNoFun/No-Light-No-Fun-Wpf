using Core.Dtos;
using Core.Models;
using Core.Messages;
using System.Collections.Concurrent;
using System.Diagnostics;


namespace Services.Matrix {
    public class DmxRoutingService {
        readonly IEnumerable<DmxRouterSettings> _routers;
        readonly IEnumerable<PatchMapEntryDto> _patches;
        readonly ArtNetDmxController _artNet;

        private readonly ConcurrentDictionary<(string ip, byte universe), byte[]> _buffers
            = new ConcurrentDictionary<(string ip, byte universe), byte[]>();


        public DmxRoutingService(
            IEnumerable<DmxRouterSettings> routers,
            IEnumerable<PatchMapEntryDto> patches,
            ArtNetDmxController artNet) {
            _routers = routers;
            _patches = patches;
            _artNet = artNet;
        }

        public void RouteUpdate(UpdateMessage packet) {
            _buffers.Clear();

            int pixelsRoutés = 0;
            int pixelsIgnorés = 0;

            foreach (Pixel pix in packet.Pixels) {
                int entityId = pix.Entity;

                // Find the router that handles this entity
                var router = _routers.FirstOrDefault(r =>
                    r.Universes.Any(u =>
                        entityId >= u.EntityIdStart &&
                        entityId <= u.EntityIdEnd));
                
                if (router == null) {
                    pixelsIgnorés++;
                    continue;
                }

                // Find the universe mapping for this entity
                var universeMap = router.Universes.First(u =>
                    entityId >= u.EntityIdStart &&
                    entityId <= u.EntityIdEnd);

                // Simple universe calculation: map entity to universe linearly
                int localIndex = entityId - universeMap.EntityIdStart;
                int totalEntities = universeMap.EntityIdEnd - universeMap.EntityIdStart + 1;
                int totalUniverses = universeMap.UniverseEnd - universeMap.UniverseStart + 1;
                
                // Calculate which universe this entity belongs to (simple linear mapping)
                byte universe = (byte)(universeMap.UniverseStart + (localIndex * totalUniverses) / totalEntities);

                // Ensure universe is within range
                if (universe < universeMap.UniverseStart || universe > universeMap.UniverseEnd) {
                    pixelsIgnorés++;
                    continue;
                }

                // Get or create buffer for this IP/universe combination
                var key = (router.Ip, universe);
                var buf = _buffers.GetOrAdd(key, _ => new byte[512]);

                // Calculate DMX channel offset within the universe
                // Each LED takes 3 channels (RGB), max 170 LEDs per universe
                int ledsPerUniverse = 170;
                int localIndexInUniverse = localIndex % ledsPerUniverse;
                int dmxOffset = universeMap.StartAddress + (localIndexInUniverse * 3);

                if (dmxOffset + 2 < 512) {
                    buf[dmxOffset + 0] = pix.R;
                    buf[dmxOffset + 1] = pix.G;
                    buf[dmxOffset + 2] = pix.B;
                    pixelsRoutés++;
                }
                else {
                    pixelsIgnorés++;
                }
            }

            // Send all prepared frames
            foreach (var kv in _buffers) {
                var (ip, uni) = kv.Key;
                var data = kv.Value;
                _artNet.SendDmxFrame(ip, 6454, uni, data);
            }

            Debug.WriteLine($"[Résumé] Pixels routés = {pixelsRoutés}, ignorés = {pixelsIgnorés}, total = {packet.Pixels.Count}");
        }
    }
}

