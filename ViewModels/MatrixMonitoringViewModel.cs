using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core.Messages;
using Services.Matrix;
using Services.Config;
using No_Fast_No_Fun_Wpf.ViewModels;

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class MatrixMonitoringViewModel : BaseViewModel
    {
        private WriteableBitmap _bitmap;
        private readonly IDmxRoutingService _routingService;
        private readonly ConfigModel _config;
        private readonly Dictionary<int, Color> _currentColors = new();
        private readonly Dictionary<int, DateTime> _lastUpdateTime = new();
        private const int TTL_MS = 150;

        private int _bitmapWidth = 128;
        private int _bitmapHeight = 128;
        private int _dpi = 96;
        private PixelFormat _pixelFormat = PixelFormats.Bgr32;
        private int _stride;
        private const int pixelSize = 2; // Larger pixels for better visibility

        public WriteableBitmap Bitmap
        {
            get => _bitmap;
            set => SetProperty(ref _bitmap, value);
        }

        public int FramesPerSecond { get; private set; }
        public int TotalPixels { get; private set; }
        public int ActivePixels { get; private set; }

        public ICommand RefreshCommand { get; }

        public MatrixMonitoringViewModel(IDmxRoutingService routingService, ConfigModel config, UdpListenerService listener = null)
        {
            _routingService = routingService;
            _config = config;
            RefreshCommand = new RelayCommand(_ => RefreshDisplay());
            
            // Subscribe to UDP updates if listener is provided
            if (listener != null)
            {
                listener.OnUpdatePacket += HandleUpdateMessage;
            }
            
            InitializeBitmap();
            UpdateStats();
        }

        private void InitializeBitmap()
        {
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

        public void HandleUpdateMessage(UpdateMessage msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Bitmap == null)
                    return;

                Bitmap.Lock();

                unsafe
                {
                    IntPtr pBackBuffer = Bitmap.BackBuffer;
                    DateTime now = DateTime.UtcNow;

                    // Clear the bitmap first
                    for (int y = 0; y < _bitmapHeight; y++)
                    {
                        for (int x = 0; x < _bitmapWidth; x++)
                        {
                            int offset = y * _stride + x * 4;
                            *((int*)((byte*)pBackBuffer + offset)) = 0x000000; // Black
                        }
                    }

                    // Build state map from incoming pixels
                    var state = new Dictionary<uint, (byte r, byte g, byte b)>();
                    foreach (var pix in msg.Pixels)
                    {
                        state[(uint)pix.Entity] = (pix.R, pix.G, pix.B);
                    }

                    // Draw pixels based on the actual DMX mapping
                    foreach (var map in _config.Mapping)
                    {
                        if (!map.Enable)
                            continue;

                        if (!state.TryGetValue(map.Entity, out var color))
                            continue;

                        // Calculate position based on universe and channel
                        var (x, y) = CalculatePixelPosition(map.Universe, map.Channel);
                        
                        if (x >= 0 && y >= 0 && x < _bitmapWidth && y < _bitmapHeight)
                        {
                            var newColor = Color.FromRgb(color.r, color.g, color.b);
                            _currentColors[(int)map.Entity] = newColor;
                            _lastUpdateTime[(int)map.Entity] = now;

                            int colorInt = (color.b) | (color.g << 8) | (color.r << 16);

                            // Draw pixel with size
                            for (int dx = 0; dx < pixelSize; dx++)
                            {
                                for (int dy = 0; dy < pixelSize; dy++)
                                {
                                    int pxX = x + dx;
                                    int pxY = y + dy;

                                    if (pxX < _bitmapWidth && pxY < _bitmapHeight)
                                    {
                                        int offset = pxY * _stride + pxX * 4;
                                        *((int*)((byte*)pBackBuffer + offset)) = colorInt;
                                    }
                                }
                            }
                        }
                    }

                    // Fade out expired pixels
                    var toExpire = new List<int>();
                    foreach (var kv in _lastUpdateTime)
                    {
                        if ((now - kv.Value).TotalMilliseconds > TTL_MS)
                            toExpire.Add(kv.Key);
                    }

                    foreach (var entity in toExpire)
                    {
                        _currentColors.Remove(entity);
                        _lastUpdateTime.Remove(entity);
                    }
                }

                Bitmap.AddDirtyRect(new Int32Rect(0, 0, _bitmapWidth, _bitmapHeight));
                Bitmap.Unlock();

                UpdateStats();
            });
        }

        private (int x, int y) CalculatePixelPosition(byte universe, ushort channel)
        {
            // Calculate position based on universe and channel
            // This simulates how the actual LED matrix would be arranged
            
            // Assuming 512 channels per universe (DMX standard)
            int channelsPerUniverse = 512;
            
            // Calculate global position
            int globalChannel = (universe * channelsPerUniverse) + (channel - 1);
            
            // Convert to 2D coordinates (assuming a rectangular matrix)
            int x = globalChannel % _bitmapWidth;
            int y = globalChannel / _bitmapWidth;
            
            return (x, y);
        }

        private void UpdateStats()
        {
            ActivePixels = _currentColors.Count;
            TotalPixels = _config.Mapping.Count(m => m.Enable);
            OnPropertyChanged(nameof(ActivePixels));
            OnPropertyChanged(nameof(TotalPixels));
        }

        private void RefreshDisplay()
        {
            InitializeBitmap();
            UpdateStats();
        }

        public void SetMatrixSize(int width, int height)
        {
            _bitmapWidth = width;
            _bitmapHeight = height;
            InitializeBitmap();
        }
    }
} 