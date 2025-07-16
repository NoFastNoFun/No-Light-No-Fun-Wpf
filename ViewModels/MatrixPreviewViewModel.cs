using System.Collections.Concurrent;
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
        public IReadOnlyDictionary<int, Point3D> EntityMap => _entityMap;
        private readonly PatchMapManagerViewModel _patchMapManager;


        public WriteableBitmap Bitmap {
            get => _bitmap;
            set => SetProperty(ref _bitmap, value);
        }

        // Remove hardcoded target width/height
        // private int _targetWidth = 128;
        // private int _targetHeight = 128;

        private readonly Dictionary<int, Color> _currentColors = new();
        private readonly Dictionary<int, DateTime> _lastUpdateTime = new();
        private const int TTL_MS = 150;

        private int _bitmapWidth;
        private int _bitmapHeight;
        private int _dpi = 96;
        private PixelFormat _pixelFormat = PixelFormats.Bgr32;
        private int _stride;
        private const int pixelSize = 1;

        private readonly UdpListenerService _listener;
        private Dictionary<int, Point3D> _entityMap = new();
        private DmxRoutingService _routingService;
     

        public ICommand OpenConsoleCommand {
            get;
        }
        public ICommand LoadJsonCommand {
            get;
        }


        public MatrixPreviewViewModel(UdpListenerService listener, DmxRoutingService routingService, PatchMapManagerViewModel patchMapManager) {
            _listener = listener;
            _routingService = routingService;
            _patchMapManager = patchMapManager;
            OpenConsoleCommand = new RelayCommand(_ => OpenConsole());
            LoadJsonCommand = new RelayCommand(_ => LoadJson());

        }

        public void OnViewActivated() {
            // Always try to load from JSON or patch
            _entityMap = LoadFromJsonOrPatch();
            if (_entityMap == null || _entityMap.Count == 0) {
                ShowError("Entity map is empty or invalid. Please load a valid layout.");
                return;
            }
            BuildPixelsFromMap();
        }

        private void BuildPixelsFromMap() {
            if (_entityMap == null || _entityMap.Count == 0) {
                ShowError("Entity map is empty or invalid. Please load a valid layout.");
                return;
            }
            int maxX = (int)_entityMap.Values.Max(p => p.X);
            int maxY = (int)_entityMap.Values.Max(p => p.Y);
            InitializeBitmap(maxX + 1, maxY + 1);
        }

        private void InitializeBitmap(int width, int height) {
            _bitmapWidth = width;
            _bitmapHeight = height;
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
            var vm = new ConsoleWindowViewModel(_listener, _routingService, _entityMap, _patchMapManager);
            var win = new ConsoleWindow(_listener, _routingService, _entityMap) { DataContext = vm };
            win.Show();
        }

        public void HandleUpdateMessage(UpdateMessage msg) {
            Application.Current.Dispatcher.Invoke(() => {
                if (Bitmap == null || _entityMap == null || _entityMap.Count == 0)
                    return;
                Bitmap.Lock();
                // Clear the bitmap
                unsafe {
                    IntPtr pBackBuffer = Bitmap.BackBuffer;
                    System.Buffer.MemoryCopy(new byte[_bitmapWidth * _bitmapHeight * 4], (void*)pBackBuffer, _bitmapWidth * _bitmapHeight * 4, _bitmapWidth * _bitmapHeight * 4);
                }
                // Draw each pixel at its mapped position
                foreach (var px in msg.Pixels) {
                    if (_entityMap.TryGetValue(px.Entity, out var pos)) {
                        int x = (int)pos.X;
                        int y = (int)pos.Y;
                        if (x >= 0 && x < _bitmapWidth && y >= 0 && y < _bitmapHeight) {
                            unsafe {
                                IntPtr pBackBuffer = Bitmap.BackBuffer;
                                int colorData = (px.B << 16) | (px.G << 8) | (px.R);
                                int offset = (y * _bitmapWidth + x) * 4;
                                System.Runtime.InteropServices.Marshal.WriteInt32(pBackBuffer, offset, colorData);
                            }
                        }
                    }
                }
                Bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmapWidth, _bitmapHeight));
                Bitmap.Unlock();
            });
        }


        public void LoadJson() {
            var map = LoadFromJsonOrPatch();
            if (map != null && map.Count > 0) {
                _entityMap = map;
                BuildPixelsFromMap(); // recreate bitmap according to mapping
            } else {
                ShowError("Failed to load entity map from JSON or patch.");
            }
        }

        private Dictionary<int, Point3D> LoadFromJsonOrPatch() {
            // Try to load from JSON file first
            var dialog = new Microsoft.Win32.OpenFileDialog {
                Title = "Load LED Layout JSON",
                Filter = "JSON Files (*.json)|*.json"
            };
            if (dialog.ShowDialog() == true) {
                try {
                    var json = System.IO.File.ReadAllText(dialog.FileName);
                    var container = Newtonsoft.Json.JsonConvert.DeserializeObject<EntityJsonContainer>(json);
                    if (container?.Pixels != null && container.Pixels.Count > 0) {
                        return container.Pixels.ToDictionary(p => p.Id, p => new Point3D(p.X, p.Y, 0));
                    }
                } catch {
                    // Ignore and fallback
                }
            }
            // Fallback: try to use patch map
            var patchMap = _patchMapManager.GetEntityToPositionMap();
            if (patchMap != null && patchMap.Count > 0) {
                return patchMap.ToDictionary(kvp => kvp.Key, kvp => new Point3D(kvp.Value.x, kvp.Value.y, 0));
            }
            return new Dictionary<int, Point3D>();
        }

        private void ShowError(string message) {
            System.Windows.MessageBox.Show(message, "Preview Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
