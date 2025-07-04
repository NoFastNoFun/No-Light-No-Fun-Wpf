using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using No_Fast_No_Fun_Wpf.Services.Network;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class MatrixPreviewViewModel : BaseViewModel {
        public int PreviewWidth {
            get;
        }
        public int PreviewHeight {
            get;
        }
        readonly Dictionary<int, (int x, int y)> _entityMap;
        public ObservableCollection<PixelViewModel> Pixels {
            get;
        }

        public MatrixPreviewViewModel(UdpListenerService listener, int width, int height, Dictionary<int, (int x, int y)> entityMap) {
            PreviewWidth = width;
            PreviewHeight = height;
            _entityMap = entityMap;

            Pixels = new ObservableCollection<PixelViewModel>();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    Pixels.Add(new PixelViewModel(x, y, Colors.Black));

            listener.OnUpdatePacket += msg => {
                foreach (var px in msg.Pixels) {
                    if (_entityMap.TryGetValue(px.Entity, out var pos)) {
                        int idx = pos.y * PreviewWidth + pos.x;
                        if (idx >= 0 && idx < Pixels.Count)
                            Pixels[idx].CurrentColor = Color.FromRgb(px.R, px.G, px.B);
                    }
                }
            };
        }

        // map simple : entity 1 → (0,0), 2→(1,0), etc.
        int ComputeIndex(int entity) {
            int zeroBased = entity - 1;
            int x = zeroBased % PreviewWidth;
            int y = zeroBased / PreviewWidth;
            if (x < 0 || x >= PreviewWidth || y < 0 || y >= PreviewHeight)
                return -1;
            return y * PreviewWidth + x;
        }
    }
}
