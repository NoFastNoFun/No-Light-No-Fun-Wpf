using System.Collections.ObjectModel;
using System.Linq;
using Core.Dtos;
using Core.Models;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class DmxRouterSettingsViewModel {
        public string Ip {
            get; set;
        }
        public int Port {
            get; set;
        }
        public ObservableCollection<UniverseMapViewModel> Universes {
            get;
        }

        public DmxRouterSettingsViewModel(DmxRouterSettingsDto dto) {
            Ip = dto.Ip;
            Port = dto.Port;
            Universes = new ObservableCollection<UniverseMapViewModel>(
                 dto.Universes.Select(u => new UniverseMapViewModel(new UniverseMap {
                     EntityIdStart = u.EntityStart,
                     EntityIdEnd = u.EntityEnd,
                     UniverseStart = u.UniverseStart,
                     UniverseEnd = u.UniverseEnd,
                     StartAddress = u.StartAddress // ← important !
                 }))

            );
        }

        public DmxRouterSettingsDto ToDto() {
            return new DmxRouterSettingsDto {
                Ip = this.Ip,
                Port = this.Port,
                Universes = this.Universes.Select(u => u.ToDto()).ToList()
            };
        }
        public DmxRouterSettings ToModel() {
            var model = new DmxRouterSettings {
                Ip = this.Ip,
                Port = this.Port
            };

            int maxPixelsPerUniverse = 170;
            byte currentUniverse = Universes.Min(u => u.UniverseStart); // point de départ global

            foreach (var vm in this.Universes) {
                int totalPixels = vm.EntityIdEnd - vm.EntityIdStart + 1;
                int pixelOffset = 0;

                while (totalPixels > 0) {
                    int pixelsInThisUniverse = Math.Min(totalPixels, maxPixelsPerUniverse);
                    int startEntity = vm.EntityIdStart + pixelOffset;
                    int endEntity = startEntity + pixelsInThisUniverse - 1;

                    var um = new UniverseMap {
                        EntityIdStart = startEntity,
                        EntityIdEnd = endEntity,
                        UniverseStart = currentUniverse,
                        UniverseEnd = currentUniverse,
                        StartAddress = 0
                    };

                    model.Universes.Add(um);

                    totalPixels -= pixelsInThisUniverse;
                    pixelOffset += pixelsInThisUniverse;
                    currentUniverse++; // on incrémente après chaque split
                }
            }

            return model;
        }
        public List<PatchMapEntryDto> ToPatchMap() {
            return this.Universes
                .SelectMany(vm => {
                    int totalPixels = vm.EntityIdEnd - vm.EntityIdStart + 1;
                    int maxPixelsPerUniverse = 170;
                    byte currentUniverse = vm.UniverseStart;

                    var patches = new List<PatchMapEntryDto>();
                    int pixelOffset = 0;

                    while (totalPixels > 0) {
                        int pixelsInThisUniverse = Math.Min(totalPixels, maxPixelsPerUniverse);
                        int startEntity = vm.EntityIdStart + pixelOffset;
                        int endEntity = startEntity + pixelsInThisUniverse - 1;

                        patches.Add(new PatchMapEntryDto {
                            EntityStart = startEntity,
                            EntityEnd = endEntity,
                            UniverseStart = currentUniverse,
                            UniverseEnd = currentUniverse
                        });

                        totalPixels -= pixelsInThisUniverse;
                        pixelOffset += pixelsInThisUniverse;
                        currentUniverse++;
                    }

                    return patches;
                }).ToList();
        }

    }
}