using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class PixelViewModel : INotifyPropertyChanged {
        int _x, _y;
        Color _color;

        public int X {
            get => _x;
            set => Set(ref _x, value);
        }

        public int Y {
            get => _y;
            set => Set(ref _y, value);
        }

        public Color CurrentColor {
            get => _color;
            set => Set(ref _color, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void Set<T>(ref T field, T value, [CallerMemberName] string? prop = null) {
            if (!Equals(field, value)) {
                field = value!;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
