using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Dtos;

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class PatchMapEntryViewModel : INotifyPropertyChanged {
        int _entityStart;
        int _entityEnd;
        byte _universeStart;
        byte _universeEnd;

        public int EntityStart {
            get => _entityStart;
            set => Set(ref _entityStart, value);
        }

        public int EntityEnd {
            get => _entityEnd;
            set => Set(ref _entityEnd, value);
        }

        public byte UniverseStart {
            get => _universeStart;
            set => Set(ref _universeStart, value);
        }

        public byte UniverseEnd {
            get => _universeEnd;
            set => Set(ref _universeEnd, value);
        }

        public PatchMapEntryViewModel() {
        }

        public PatchMapEntryViewModel(PatchMapEntryDto dto) {
            EntityStart = dto.EntityStart;
            EntityEnd = dto.EntityEnd;
            UniverseStart = dto.UniverseStart;
            UniverseEnd = dto.UniverseEnd;
        }

        public PatchMapEntryDto ToModel()
            => new PatchMapEntryDto {
                EntityStart = this.EntityStart,
                EntityEnd = this.EntityEnd,
                UniverseStart = this.UniverseStart,
                UniverseEnd = this.UniverseEnd
            };

        public event PropertyChangedEventHandler? PropertyChanged;
        void Set<T>(ref T field, T value, [CallerMemberName] string? propName = null) {
            if (!Equals(field, value)) {
                field = value!;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
