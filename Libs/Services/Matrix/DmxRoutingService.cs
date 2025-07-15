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

                // 1) Patch correspondant
                var patch = _patches.FirstOrDefault(p =>
                    entityId >= p.EntityStart && entityId <= p.EntityEnd);
                if (patch == null) {
                    pixelsIgnorés++;
                    continue;
                }

                // 2) Calcul de l’univers cible
                int relIndex = entityId - patch.EntityStart;
                int totalEnt = patch.EntityEnd - patch.EntityStart + 1;
                int totalUni = patch.UniverseEnd - patch.UniverseStart + 1;
                byte universe = (byte)(patch.UniverseStart + (relIndex * totalUni) / totalEnt);

                // 3) Routeur gérant cette entité
                // Ajoute ce debug juste avant le FirstOrDefault
                foreach (var r in _routers)
                    foreach (var u in r.Universes)
                        if (entityId >= u.EntityIdStart && entityId <= u.EntityIdEnd &&
                            universe >= u.UniverseStart && universe <= u.UniverseEnd)
                            ;
                           
                            

                var router = _routers.FirstOrDefault(r =>
                    r.Universes.Any(u =>
                        entityId >= u.EntityIdStart &&
                        entityId <= u.EntityIdEnd &&
                        universe >= u.UniverseStart &&
                        universe <= u.UniverseEnd));
                if (router == null) {
                    pixelsIgnorés++;
                    continue;
                }

                // 4) Buffer DMX
                var key = (router.Ip, universe);
                var buf = _buffers.GetOrAdd(key, _ => new byte[512]);

                // 5) Mapping interne
                var map = router.Universes.First(u =>
                    entityId >= u.EntityIdStart &&
                    entityId <= u.EntityIdEnd &&
                    universe >= u.UniverseStart &&
                    universe <= u.UniverseEnd);

                int pixelIndex = entityId - map.EntityIdStart;
                int dmxOffset = map.StartAddress + pixelIndex * 3;

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

            // 6) Univers préparés
            foreach (var kv in _buffers) {
                var (ip, uni) = kv.Key;
            
            }

            // 7) Envoi ArtNet
            foreach (var kv in _buffers) {
                var (ip, uni) = kv.Key;
                var data = kv.Value;

               
                _artNet.SendDmxFrame(ip, 6454, uni, data);
            }

            Debug.WriteLine($"[Résumé] Pixels routés = {pixelsRoutés}, ignorés = {pixelsIgnorés}, total = {packet.Pixels.Count}");
        }
    }
}

