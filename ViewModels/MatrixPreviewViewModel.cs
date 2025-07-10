using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Core.Dtos;
using Core.Messages;
using Microsoft.Win32;
using Newtonsoft.Json;
using No_Fast_No_Fun_Wpf.Services.Network;
using No_Fast_No_Fun_Wpf.Views.Windows;
using Services.Matrix;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class MatrixPreviewViewModel : BaseViewModel {
        private WriteableBitmap _bitmap;
        public WriteableBitmap Bitmap {
            get => _bitmap;
            set => SetProperty(ref _bitmap, value);
        }
        private int _targetWidth = 128;
        private int _targetHeight = 128;

        private int _bitmapWidth;
        private int _bitmapHeight;
        private int _dpi = 96;
        private PixelFormat _pixelFormat = PixelFormats.Bgr32;
        private int _stride;
        private const int pixelSize = 1;

        private readonly UdpListenerService _listener;
        private Dictionary<int, Point3D> _entityMap = new();
        private Dictionary<int, PixelViewModel> _entityToPixel = new();
        private readonly DmxRoutingService _routingService;

        public ObservableCollection<PixelViewModel> Pixels { get; } = new();

        public ICommand OpenConsoleCommand {
            get;
        }

        public MatrixPreviewViewModel(UdpListenerService listener, DmxRoutingService routingService) {
            _listener = listener;
            _routingService = routingService;
            OpenConsoleCommand = new RelayCommand(_ => OpenConsole());
        }

        public void OnViewActivated() {
            _entityMap = LoadFromJson(_targetWidth, _targetHeight);
            if (_entityMap == null || _entityMap.Count == 0)
                _entityMap = GenerateVirtualEntityMap();

            BuildPixelsFromMap();
        }

        private void BuildPixelsFromMap() {
            Pixels.Clear();
            _entityToPixel.Clear();

            int maxX = (int)_entityMap.Values.Max(p => p.X);
            int maxY = (int)_entityMap.Values.Max(p => p.Y);

            InitializeBitmap();

            foreach (var kv in _entityMap) {
                var pixel = new PixelViewModel(0, 0, Colors.Black) {
                    WorldPosition = kv.Value
                };
                Pixels.Add(pixel);
                _entityToPixel[kv.Key] = pixel;
            }
        }
        private void InitializeBitmap() {
            _bitmapWidth = _targetWidth;
            _bitmapHeight = _targetHeight;
            _stride = (_bitmapWidth * _pixelFormat.BitsPerPixel + 7) / 8;

            Bitmap = new WriteableBitmap(
                _bitmapWidth,
                _bitmapHeight,
                _dpi,
                _dpi,
                _pixelFormat,
                null
            );

            OnPropertyChanged(nameof(Bitmap));
        }

        private void OpenConsole() {
            var vm = new ConsoleWindowViewModel(_listener, _routingService, _entityMap);
            var win = new ConsoleWindow(_listener, _routingService, _entityMap) { DataContext = vm };
            win.Show();
        }

        public void HandleUpdateMessage(UpdateMessage msg) {
            Application.Current.Dispatcher.Invoke(() => {
                if (Bitmap == null)
                    return;
                Bitmap.Lock();

                unsafe {
                    IntPtr pBackBuffer = Bitmap.BackBuffer;

                    foreach (var px in msg.Pixels) {
                        if (_entityMap.TryGetValue(px.Entity, out var pos)) {
                            int x = (int)(pos.X * pixelSize);
                            int y = (int)(pos.Y * pixelSize);

                            if (x < 0 || y < 0 || x >= _bitmapWidth || y >= _bitmapHeight)
                                continue;

                            int color = (px.B) | (px.G << 8) | (px.R << 16);

                            for (int dx = 0; dx < pixelSize; dx++) {
                                for (int dy = 0; dy < pixelSize; dy++) {
                                    int pxX = x + dx;
                                    int pxY = y + dy;

                                    if (pxX < _bitmapWidth && pxY < _bitmapHeight) {
                                        int offset = pxY * _stride + pxX * 4;
                                        *((int*)((byte*)pBackBuffer + offset)) = color;
                                    }
                                }
                            }
                        }
                    }
                }

                Bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmapWidth, _bitmapHeight));
                Bitmap.Unlock();
            });
        }

        public static Dictionary<int, Point3D> LoadFromJson(int targetWidth, int targetHeight) {
            var dialog = new OpenFileDialog {
                Title = "Charger la configuration Unity",
                Filter = "Fichiers JSON (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true) {
                var json = File.ReadAllText(dialog.FileName);
                var container = JsonConvert.DeserializeObject<EntityJsonContainer>(json);

                if (container?.Pixels == null || container.Pixels.Count == 0)
                    return new();

                const int minEntity = 100;
                const int maxEntity = 19858;

                var filtered = container.Pixels
                    .Where(p => p.Id >= minEntity && p.Id <= maxEntity)
                    .ToList();

                if (filtered.Count == 0) {
                    Debug.WriteLine("[ERROR] Aucun pixel dans la plage d'entités sélectionnée.");
                    return new();
                }

                double minX = filtered.Min(p => p.X);
                double maxX = filtered.Max(p => p.X);
                double minY = filtered.Min(p => p.Y);
                double maxY = filtered.Max(p => p.Y);

                double rangeX = maxX - minX;
                double rangeY = maxY - minY;



                var map = filtered.ToDictionary(
                        e => e.Id,
                        e => new Point3D(
                            ((e.X - minX) / rangeX) * (targetWidth - 1),
                            ((1.0 - (e.Y - minY) / rangeY)) * (targetHeight - 1),
                                0
                        )
                );

                Debug.WriteLine($"[LOAD] {map.Count} entités chargées entre {minEntity} et {maxEntity}");
                return map;
            }

            return new();
        }



        public static Dictionary<int, Point3D> GenerateVirtualEntityMap() {
            var map = new Dictionary<int, Point3D>();
            int width = 64;
            int height = 128;
            int entityId = 1;

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    map[entityId++] = new Point3D(x, y, 0);
                }
            }
            return map;
        }
    }
}
