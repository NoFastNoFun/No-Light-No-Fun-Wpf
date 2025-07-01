using System.Collections.ObjectModel;
using System.Windows.Media;
using No_Fast_No_Fun_Wpf.Services.Network;

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class MatrixPreviewViewModel : BaseViewModel {
        public ObservableCollection<PixelViewModel> Pixels {
            get;
        }
        readonly UdpListenerService _listener;
        readonly int _width;
        readonly int _height;

        public MatrixPreviewViewModel(UdpListenerService listener, int width, int height) {
            _listener = listener;
            _width = width;
            _height = height;
            Pixels = new ObservableCollection<PixelViewModel>();

            for (int y = 0; y < _height; y++)
                for (int x = 0; x < _width; x++)
                    Pixels.Add(new PixelViewModel { X = x, Y = y, CurrentColor = Colors.Black });

            _listener.OnUpdatePacket += packet => {
                
                foreach (var px in packet.Pixels) {
                    int id = px.Entity; 
                    int idx = id;       
                    if (idx >= 0 && idx < Pixels.Count) {
                        var vm = Pixels[idx];
                        vm.CurrentColor = Color.FromRgb(px.R, px.G, px.B);
                    }
                }
            };
        }
    }
}
