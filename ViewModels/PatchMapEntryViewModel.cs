using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Dtos;

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class PatchMapEntryViewModel : INotifyPropertyChanged {
        int _from;
        int _to;

        public int From {
            get => _from;
            set => Set(ref _from, value);
        }

        public int To {
            get => _to;
            set => Set(ref _to, value);
        }

        public PatchMapEntryViewModel() {
        }

        public PatchMapEntryViewModel(PatchMapEntryDto dto) {
            From = dto.From;
            To = dto.To;
        }

        public PatchMapEntryDto ToModel()
            => new PatchMapEntryDto { From = From, To = To };

        public event PropertyChangedEventHandler? PropertyChanged;
        void Set<T>(ref T field, T value, [CallerMemberName] string? propName = null) {
            if (!Equals(field, value)) {
                field = value!;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
