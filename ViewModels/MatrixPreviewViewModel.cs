using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Core.Messages;
using No_Fast_No_Fun_Wpf.Services.Network;
using No_Fast_No_Fun_Wpf.Views.Windows;
using Services.Matrix;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class MatrixPreviewViewModel : BaseViewModel {

        private readonly UdpListenerService _listener;
        private readonly Dictionary<int, (int x, int y)> _entityMap;
        private readonly DmxRoutingService _routingService;


        private int _previewWidth = 64;
        public int PreviewWidth {
            get => _previewWidth;
            set {
                if (SetProperty(ref _previewWidth, value))
                    ;
            }
        }

        private int _previewHeight = 256;
        public int PreviewHeight {
            get => _previewHeight;
            set {
                if (SetProperty(ref _previewHeight, value))
                    ;
            }
        }

        public ObservableCollection<PixelViewModel> Pixels {
            get;
        }

        public ICommand ApplyResolutionCommand {
            get;
        }
        public ICommand OpenConsoleCommand {
            get;
        }

        public MatrixPreviewViewModel(UdpListenerService listener,
                                      DmxRoutingService routingService,
                                      Dictionary<int, (int x, int y)> entityMap) {
            _listener = listener;
            _routingService = routingService;
            _entityMap = entityMap;

            Pixels = new ObservableCollection<PixelViewModel>();

            ApplyResolutionCommand = new RelayCommand(_ => RebuildPixels());
            OpenConsoleCommand = new RelayCommand(_ => OpenConsole());

            RebuildPixels();
        }

        private void RebuildPixels() {
            Pixels.Clear();
            System.Diagnostics.Debug.WriteLine($"EntityMap count: {_entityMap.Count}");
            foreach (var kv in _entityMap)
                System.Diagnostics.Debug.WriteLine($"Entity {kv.Key} => Pos({kv.Value.x},{kv.Value.y})");

            for (int y = 0; y < PreviewHeight; y++) {
                for (int x = 0; x < PreviewWidth; x++) {
                    Pixels.Add(new PixelViewModel(x, y, Colors.Black));
                }
            }
        }

        private void OpenConsole() {
            var vm = new ConsoleWindowViewModel(_listener, _routingService, _entityMap);
            var win = new ConsoleWindow(_listener, _routingService) {
                DataContext = vm
            };
            win.Show();
        }

        public void HandleUpdateMessage(UpdateMessage msg) {
            foreach (var px in msg.Pixels) {
                if (_entityMap.TryGetValue(px.Entity, out var pos)) {
                    int idx = pos.y * PreviewWidth + pos.x;
                    if (idx >= 0 && idx < Pixels.Count) {
                        Pixels[idx].CurrentColor = Color.FromRgb(px.R, px.G, px.B);
                    }
                }
            }
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
