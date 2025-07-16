using Core.Dtos;
using Core.Models;
using Core.Messages;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace Services.Matrix {
    public class DmxRoutingService {
        private readonly IEnumerable<DmxRouterSettings> _routers;
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
            _routers = routers;
            _configItems = configItems;
            _patches = patches;
            _artNet = artNet;
        }

        public void RouteUpdate(UpdateMessage packet) {
            _buffers.Clear();

            int pixelsRoutés = 0;
            int pixelsIgnorés = 0;

            foreach (Pixel pix in packet.Pixels) {
                int entityId = pix.Entity;

                // 1. PatchMap d'abord (correction locale), sinon Config principale
                PatchMapEntryDto patch = _patches.FirstOrDefault(p =>
                    entityId >= p.EntityStart && entityId <= p.EntityEnd);

                ConfigItem config = null;
                byte universe;
                string controllerIp;

                if (patch != null) {
                    universe = patch.Universe;
                    controllerIp = null; // PatchMap ne définit pas l’IP, il faudra la retrouver dans la config
                }
                else {
                    config = _configItems.FirstOrDefault(c =>
                        entityId >= c.StartEntityId && entityId <= c.EndEntityId);

                    if (config == null) {
                        pixelsIgnorés++;
                        continue;
                    }
                    universe = config.Universe;
                    controllerIp = config.ControllerIp;
                }

                // 2. Sélection du routeur : priorité PatchMap (univers+ID) sinon Config
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
                    controllerIp == null || r.Ip == controllerIp) ?? possibleRouters.First();

                // 3. Recherche du mapping dans le routeur pour cet univers et ID
                var map = router.Universes.FirstOrDefault(u =>
                    entityId >= u.EntityIdStart &&
                    entityId <= u.EntityIdEnd &&
                    universe == u.Universe);

                if (map == null) {
                    pixelsIgnorés++;
                    continue;
                }

                int pixelIndex = entityId - map.EntityIdStart;
                int dmxOffset = map.StartAddress + pixelIndex * 3;

                // 4. DMX packing
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
            }

            // 5. Envoi ArtNet (par ip/universe)
            foreach (var kv in _buffers) {
                var (ip, uni) = kv.Key;
                var data = kv.Value;
                _artNet.SendDmxFrame(ip, 6454, uni, data);
            }

            Debug.WriteLine($"[Résumé] Pixels routés = {pixelsRoutés}, ignorés = {pixelsIgnorés}, total = {packet.Pixels.Count}");
        }
    }
}
