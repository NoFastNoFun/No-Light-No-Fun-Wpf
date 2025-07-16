using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Core.Dtos;

namespace Core.Models
{
    public class DmxRouterSettings : INotifyPropertyChanged {
        string _ip = "127.0.0.1";
        public string Ip {
            get => _ip;
            set {
                if (_ip == value)
                    return;
                _ip = value;
                OnPropertyChanged();
            }
        }

        int _port = 6454;
        public int Port {
            get => _port;
            set {
                if (_port == value)
                    return;
                _port = value;
                OnPropertyChanged();
            }
        }
        public static DmxRouterSettings FromDto(DmxRouterSettingsDto dto) {
            var router = new DmxRouterSettings {
                Ip = dto.Ip,
                Port = dto.Port
            };
            router.Universes.Clear();
            if (dto.Universes != null)
                foreach (var u in dto.Universes)
                    router.Universes.Add(UniverseMap.FromDto(u));
            return router;
        }
        public ObservableCollection<UniverseMap> Universes {
            get;
        }
            = new ObservableCollection<UniverseMap>();

        // Implémentation de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
