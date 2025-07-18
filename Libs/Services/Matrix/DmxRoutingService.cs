using Core.Dtos;
using Core.Models;
using Core.Messages;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace Services.Matrix {
    public class DmxRoutingService {
        private readonly List<DmxRouterSettings> _routers;
        private readonly IEnumerable<PatchMapEntryDto> _patches;
        private readonly IEnumerable<ConfigItem> _configItems;
        private readonly ArtNetDmxController _artNet;

        private readonly ConcurrentDictionary<(string ip, byte universe), byte[]> _buffers
            = new ConcurrentDictionary<(string ip, byte universe), byte[]>();

        public DmxRoutingService(
            IEnumerable<DmxRouterSettings> routers,
            IEnumerable<ConfigItem> configItems,
            IEnumerable<PatchMapEntryDto> patches,
            ArtNetDmxController artNet) {
            // Copie locale modifiable pour patcher à chaud si nécessaire
            _routers = routers.ToList();
            _configItems = configItems;
            _patches = patches;
            _artNet = artNet;
        }

        public void RouteUpdate(UpdateMessage packet) {
            var sw = Stopwatch.StartNew();
            _buffers.Clear();

            int pixelsRoutés = 0;
            int pixelsPatchés = 0;
            int pixelsIgnorés = 0;

            foreach (Pixel pix in packet.Pixels) {
                int entityId = pix.Entity;

                // 1. Cherche d'abord dans la config principale
                ConfigItem config = _configItems.FirstOrDefault(c =>
                    entityId >= c.StartEntityId && entityId <= c.EndEntityId);

                PatchMapEntryDto patch = null;

                byte universe;
                string controllerIp = null;
                int dmxOffset = -1;

                if (config != null) {
                    universe = config.Universe;
                    controllerIp = config.ControllerIp;

                    // Recherche du routeur qui gère ce mapping
                    var possibleRouters = _routers.Where(r =>
                        r.Universes.Any(u =>
                            entityId >= u.EntityIdStart &&
                            entityId <= u.EntityIdEnd &&
                            universe == u.Universe)).ToList();

                    if (possibleRouters.Count == 0) {
                        pixelsIgnorés++;
                        continue;
                    }

                    var router = possibleRouters.FirstOrDefault(r =>
                        r.Ip == controllerIp) ?? possibleRouters.First();

                    var map = router.Universes.FirstOrDefault(u =>
                        entityId >= u.EntityIdStart &&
                        entityId <= u.EntityIdEnd &&
                        universe == u.Universe);

                    if (map == null) {
                        pixelsIgnorés++;
                        continue;
                    }

                    int pixelIndex = entityId - map.EntityIdStart;
                    dmxOffset = map.StartAddress + pixelIndex * 3;
                    var key = (router.Ip, universe);
                    var buf = _buffers.GetOrAdd(key, _ => new byte[512]);

                    if (dmxOffset + 2 < 512) {
                        buf[dmxOffset + 0] = pix.R;
                        buf[dmxOffset + 1] = pix.G;
                        buf[dmxOffset + 2] = pix.B;
                        pixelsRoutés++;
                    }
                    else {
                        pixelsIgnorés++;
                    }
                    continue;
                }

                // 2. Si pas trouvé dans la config principale, tente le patchmap (cas exceptionnel)
                patch = _patches.FirstOrDefault(p =>
                    entityId >= p.EntityStart && entityId <= p.EntityEnd);

                if (patch != null) {
                    universe = patch.Universe;
                    controllerIp = null; 

                    var possibleRouters = _routers.Where(r =>
                        r.Universes.Any(u =>
                            entityId >= u.EntityIdStart &&
                            entityId <= u.EntityIdEnd &&
                            universe == u.Universe)).ToList();

                    if (possibleRouters.Count == 0) {
                        pixelsIgnorés++;
                        continue;
                    }

                    var router = possibleRouters.First();

                    var map = router.Universes.FirstOrDefault(u =>
                        entityId >= u.EntityIdStart &&
                        entityId <= u.EntityIdEnd &&
                        universe == u.Universe);

                    if (map == null) {
                        pixelsIgnorés++;
                        continue;
                    }

                    int pixelIndex = entityId - map.EntityIdStart;
                    dmxOffset = map.StartAddress + pixelIndex * 3;
                    var key = (router.Ip, universe);
                    var buf = _buffers.GetOrAdd(key, _ => new byte[512]);

                    if (dmxOffset + 2 < 512) {
                        buf[dmxOffset + 0] = pix.R;
                        buf[dmxOffset + 1] = pix.G;
                        buf[dmxOffset + 2] = pix.B;
                        pixelsPatchés++;
                    }
                    else {
                        pixelsIgnorés++;
                    }
                    continue;
                }

                // 3. Sinon, ignoré
                pixelsIgnorés++;
            }

            // 4. Envoi ArtNet (par ip/universe)
            foreach (var kv in _buffers) {
                var (ip, uni) = kv.Key;
                var data = kv.Value;
                _artNet.SendDmxFrame(ip, 6454, uni, data);
            }
            sw.Stop();

        }
    }
}

