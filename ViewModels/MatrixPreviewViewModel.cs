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
        private readonly ConfigEditorViewModel _configEditor;
        private List<int> _unityIndexToId = new List<int>();
        public IReadOnlyList<int> UnityIndexToId => _unityIndexToId;

        public WriteableBitmap Bitmap {
            get => _bitmap;
            set => SetProperty(ref _bitmap, value);
        }
        private int _targetWidth = 128;
        private int _targetHeight = 128;

        private readonly Dictionary<int, Color> _currentColors = new();
        private readonly Dictionary<int, DateTime> _lastUpdateTime = new();
        private const int TTL_MS = 150;
        private readonly Dictionary<int, ushort> _lastPacketSequence = new();
        private ushort _expectedSequence = 0;

        private int _bitmapWidth;
        private int _bitmapHeight;
        private int _dpi = 96;
        private PixelFormat _pixelFormat = PixelFormats.Bgr32;
        private int _stride;
        private const int pixelSize = 1;

        private readonly UdpListenerService _listener;
        private Dictionary<int, Point3D> _entityMap = new();
        private DmxRoutingService _routingService;
        private System.Timers.Timer _cleanupTimer;
     

        public ICommand OpenConsoleCommand {
            get;
        }
        public ICommand LoadJsonCommand {
            get;
        }


        public MatrixPreviewViewModel(UdpListenerService listener, DmxRoutingService routingService, PatchMapManagerViewModel patchMapManager, ConfigEditorViewModel configEditor) {
            _listener = listener;
            _listener.OnUpdatePacket += HandleUpdateMessage;
            _routingService = routingService;
            _patchMapManager = patchMapManager;
            OpenConsoleCommand = new RelayCommand(_ => OpenConsole());
            LoadJsonCommand = new RelayCommand(_ => LoadJson());
            _configEditor = configEditor;
            
            // Initialize cleanup timer to remove stale pixels
            _cleanupTimer = new System.Timers.Timer(TTL_MS);
            _cleanupTimer.Elapsed += (s, e) => CleanupStalePixels();
            _cleanupTimer.Start();
        }

        public void OnViewActivated() {
            string unityJsonPath = @"C:\Users\banda\Pictures\Screenshots\entity_config.json";
            var (entityMap, unityIndexToId) = LoadFromJsonWithIndex(unityJsonPath, _targetWidth, _targetHeight);

            if (entityMap == null || entityMap.Count == 0) {
                entityMap = GenerateVirtualEntityMap();
                unityIndexToId = entityMap.Keys.OrderBy(x => x).ToList();
            }

            _entityMap = entityMap;
            _unityIndexToId = unityIndexToId;
            BuildPixelsFromMap();
        }



        private void BuildPixelsFromMap() {
            int maxX = (int)_entityMap.Values.Max(p => p.X);
            int maxY = (int)_entityMap.Values.Max(p => p.Y);
            InitializeBitmap();
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
            var vm = new ConsoleWindowViewModel(_listener, _routingService, _entityMap, _patchMapManager, _configEditor);
            var win = new ConsoleWindow(_listener, _routingService, _entityMap) { DataContext = vm };
            win.Show();
        }

        public void HandleUpdateMessage(UpdateMessage msg) {
            try {
                if (msg?.Pixels == null || msg.Pixels.Count == 0)
                    return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Bitmap == null || _entityMap == null || _entityMap.Count == 0)
                        return;

                    int bufferSize = _bitmapWidth * _bitmapHeight * 4;
                    if (Bitmap.BackBuffer == IntPtr.Zero || bufferSize <= 0)
                        return;

                    var now = DateTime.Now;
                    bool hasValidPixels = false;

                    // Update colors and track timing for received pixels
                    foreach (var px in msg.Pixels) {
                        if (px == null)
                            continue;
                        if (px.Entity < 100 || px.Entity > 19858)
                            continue;

                        var color = Color.FromRgb(px.R, px.G, px.B);
                        _currentColors[px.Entity] = color;
                        _lastUpdateTime[px.Entity] = now;
                        hasValidPixels = true;
                    }

                    // If we received valid pixels, update the display
                    if (hasValidPixels) {
                        UpdateDisplay();
                    }
                });
            }
            catch (Exception ex) {
                Debug.WriteLine($"HandleUpdateMessage exception: {ex}");
            }
        }

        private void CleanupStalePixels() {
            try {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Bitmap == null || _entityMap == null || _entityMap.Count == 0)
                        return;

                    var now = DateTime.Now;
                    var staleEntities = new List<int>();

                    // Find pixels that haven't been updated recently
                    foreach (var kvp in _lastUpdateTime) {
                        if ((now - kvp.Value).TotalMilliseconds > TTL_MS) {
                            staleEntities.Add(kvp.Key);
                        }
                    }

                    // Remove stale pixels
                    foreach (var entityId in staleEntities) {
                        _currentColors.Remove(entityId);
                        _lastUpdateTime.Remove(entityId);
                    }

                    // Update display if we removed stale pixels
                    if (staleEntities.Count > 0) {
                        UpdateDisplay();
                        Debug.WriteLine($"[CLEANUP] Removed {staleEntities.Count} stale pixels");
                    }
                });
            }
            catch (Exception ex) {
                Debug.WriteLine($"CleanupStalePixels exception: {ex}");
            }
        }

        private void UpdateDisplay() {
            try {
                int bufferSize = _bitmapWidth * _bitmapHeight * 4;
                if (Bitmap.BackBuffer == IntPtr.Zero || bufferSize <= 0)
                    return;

                Bitmap.Lock();
                unsafe {
                    IntPtr pBackBuffer = Bitmap.BackBuffer;
                    System.Span<byte> span = new Span<byte>((void*)pBackBuffer, bufferSize);
                    span.Clear();

                    foreach (var kv in _currentColors) {
                        if (_entityMap.TryGetValue(kv.Key, out var pos)) {
                            int x = (int)pos.X;
                            int y = (int)pos.Y;
                            if (x >= 0 && x < _bitmapWidth && y >= 0 && y < _bitmapHeight) {
                                var c = kv.Value;
                                int colorData = (c.R << 16) | (c.G << 8) | c.B;
                                int offset = (y * _bitmapWidth + x) * 4;
                                if (offset >= 0 && offset + 4 <= bufferSize)
                                    System.Runtime.InteropServices.Marshal.WriteInt32(pBackBuffer, offset, colorData);
                            }
                        }
                    }
                }
                Bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmapWidth, _bitmapHeight));
                Bitmap.Unlock();
            }
            catch (Exception ex) {
                Debug.WriteLine($"UpdateDisplay exception: {ex}");
            }
        }




        public void LoadJson() {
            var dialog = new OpenFileDialog {
                Title = "Charger la configuration Unity",
                Filter = "Fichiers JSON (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true) {
                var (entityMap, unityIndexToId) = LoadFromJsonWithIndex(dialog.FileName, _targetWidth, _targetHeight);

                if (entityMap != null && entityMap.Count > 0) {
                    _entityMap = entityMap;
                    _unityIndexToId = unityIndexToId;
                    BuildPixelsFromMap();
                }
            }
        }

        public void Dispose() {
            _cleanupTimer?.Stop();
            _cleanupTimer?.Dispose();
            _listener.OnUpdatePacket -= HandleUpdateMessage;
        }


        public static (Dictionary<int, Point3D> entityMap, List<int> unityIndexToId) LoadFromJsonWithIndex(string file, int targetWidth, int targetHeight) {
            var json = File.ReadAllText(file);
            var container = JsonConvert.DeserializeObject<EntityJsonContainer>(json);

            if (container?.Pixels == null || container.Pixels.Count == 0)
                return (new(), new());

            const int minEntity = 100;
            const int maxEntity = 19858;

            // Liste ordonnée
            var idList = container.Pixels
                .Where(p => p.Id >= minEntity && p.Id <= maxEntity)
                .Select(p => p.Id)
                .ToList();

            // Map de position
            var filtered = container.Pixels
                .Where(p => p.Id >= minEntity && p.Id <= maxEntity)
                .ToList();

            if (filtered.Count == 0)
                return (new(), new());

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
                        ((1.0 - (e.Y - minY) / rangeY)) * (targetHeight - 1),  // <- L’axe Y est inversé ici !
                        0
                        )
                        );

            return (map, idList);
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
