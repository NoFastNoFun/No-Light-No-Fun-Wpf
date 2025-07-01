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
                     UniverseEnd = u.UniverseEnd
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

            foreach (var vm in this.Universes)
                model.Universes.Add(vm.ToModel());

            return model;
        }
    }
}