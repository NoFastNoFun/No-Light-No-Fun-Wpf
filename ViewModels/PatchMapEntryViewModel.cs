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
        int _x;
        int _y;
        int _width;

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

        public int X {
            get => _x;
            set => Set(ref _x, value);
        }

        public int Y {
            get => _y;
            set => Set(ref _y, value);
        }

        public int Width {
            get => _width;
            set => Set(ref _width, value);
        }

        public int EntityIdStart => EntityStart;
        public int EntityIdEnd => EntityEnd;

        public PatchMapEntryViewModel() {
            Width = 128; 
        }

        public PatchMapEntryViewModel(PatchMapEntryDto dto) {
            EntityStart = dto.EntityStart;
            EntityEnd = dto.EntityEnd;
            UniverseStart = dto.UniverseStart;
            UniverseEnd = dto.UniverseEnd;
            X = dto.X;
            Y = dto.Y;
            Width = dto.Width > 0 ? dto.Width : 128;
        }

        public PatchMapEntryDto ToModel()
            => new PatchMapEntryDto {
                EntityStart = this.EntityStart,
                EntityEnd = this.EntityEnd,
                UniverseStart = this.UniverseStart,
                UniverseEnd = this.UniverseEnd,
                X = this.X,
                Y = this.Y,
                Width = this.Width
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
