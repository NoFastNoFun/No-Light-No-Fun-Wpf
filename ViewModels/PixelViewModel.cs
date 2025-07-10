using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class PixelViewModel : INotifyPropertyChanged {
        int _x, _y;
        Color _currentColor;
        Point3D _worldPosition;
        int _entityId;

        public int X {
            get => _x;
            set => Set(ref _x, value);
        }

        public int Y {
            get => _y;
            set => Set(ref _y, value);
        }

        public int EntityId {
            get => _entityId;
            set => Set(ref _entityId, value);
        }

        public Point3D WorldPosition {
            get => _worldPosition;
            set {
                if (Set(ref _worldPosition, value)) {
                    OnPropertyChanged(nameof(CanvasX));
                    OnPropertyChanged(nameof(CanvasY));
                }
            }
        }

        public double CanvasX => (WorldPosition.X + 1.0) * 500; 
        public double CanvasY => (1.0 - WorldPosition.Y) * 500;


        public Color CurrentColor {
            get => _currentColor;
            set => Set(ref _currentColor, value);
        }

        public PixelViewModel() {
            _currentColor = Colors.Black;
        }

        public PixelViewModel(int x, int y, Color initial) {
            _x = x;
            _y = y;
            _currentColor = initial;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool Set<T>(ref T field, T value, [CallerMemberName] string? propName = null) {
            if (!Equals(field, value)) {
                field = value!;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
                return true;
            }
            return false;
        }


        void OnPropertyChanged(string propName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
