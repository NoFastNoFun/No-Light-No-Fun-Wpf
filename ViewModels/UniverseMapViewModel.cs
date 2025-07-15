using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Dtos;
using Core.Models;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class UniverseMapViewModel : INotifyPropertyChanged {
        readonly UniverseMap _model;
        public UniverseMapViewModel(UniverseMap model) => _model = model;

        public int EntityIdStart {
            get => _model.EntityIdStart;
            set {
                if (_model.EntityIdStart != value) {
                    _model.EntityIdStart = value;
                    OnPropertyChanged();
                }
            }
        }

        public int EntityIdEnd {
            get => _model.EntityIdEnd;
            set {
                if (_model.EntityIdEnd != value) {
                    _model.EntityIdEnd = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte UniverseStart {
            get => _model.UniverseStart;
            set {
                if (_model.UniverseStart != value) {
                    _model.UniverseStart = value;
                    OnPropertyChanged();
                }
            }
        }

        public byte UniverseEnd {
            get => _model.UniverseEnd;
            set {
                if (_model.UniverseEnd != value) {
                    _model.UniverseEnd = value;
                    OnPropertyChanged();
                }
            }
        }

        public int StartAddress {
            get => _model.StartAddress;
            set {
                // Vérifie que c’est bien un multiple de 3 et dans la plage DMX
                if (value % 3 != 0 || value < 0 || value > 509) {
                    // Tu peux lever une exception ou juste ignorer ici
                    Console.WriteLine($"[WARNING] StartAddress invalide : {value} (doit être multiple de 3 et <= 509)");
                    return;
                }

                if (_model.StartAddress != value) {
                    _model.StartAddress = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string? n = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        public UniverseMapDto ToDto() {
            return new UniverseMapDto {
                EntityStart = _model.EntityIdStart,
                EntityEnd = _model.EntityIdEnd,
                UniverseStart = _model.UniverseStart,
                UniverseEnd = _model.UniverseEnd,
                StartAddress = _model.StartAddress
            };
        }

        public UniverseMap ToModel() {
            return new UniverseMap {
                EntityIdStart = _model.EntityIdStart,
                EntityIdEnd = _model.EntityIdEnd,
                UniverseStart = _model.UniverseStart,
                UniverseEnd = _model.UniverseEnd,
            };
        }
    }
}
