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

        public ObservableCollection<PixelViewModel> Pixels {
            get;
        }

        public MatrixPreviewViewModel(UdpListenerService listener, int width, int height) {
            PreviewWidth = width;
            PreviewHeight = height;

            // Initialise la grille de pixels
            Pixels = new ObservableCollection<PixelViewModel>(
                Enumerable.Range(0, height)
                          .SelectMany(y =>
                              Enumerable.Range(0, width)
                                        .Select(x => new PixelViewModel(x, y, Colors.Black))
                          )
            );

            listener.OnUpdatePacket += msg => {
                foreach (var px in msg.Pixels) {
                    int idx = ComputeIndex(px.Entity);
                    if (idx >= 0 && idx < Pixels.Count) {
                        // applique la nouvelle couleur
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
