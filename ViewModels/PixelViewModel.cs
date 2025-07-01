using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class PixelViewModel : INotifyPropertyChanged {
        int _x, _y;
        Color _currentColor;

        public int X {
            get => _x;
            set => Set(ref _x, value);
        }

        public int Y {
            get => _y;
            set => Set(ref _y, value);
        }

        public Color CurrentColor {
            get => _currentColor;
            set => Set(ref _currentColor, value);
        }

        public PixelViewModel(int x, int y, Color initial) {
            _x = x;
            _y = y;
            _currentColor = initial;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        void Set<T>(ref T field, T value, [CallerMemberName] string? propName = null) {
            if (!Equals(field, value)) {
                field = value!;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
