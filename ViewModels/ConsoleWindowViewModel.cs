﻿using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Core.Messages;
using Core.Models;
using Microsoft.Win32;
using No_Fast_No_Fun_Wpf.Services.Network;
using Services.Matrix;
using OpenCvSharp;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class ConsoleWindowViewModel : BaseViewModel {
        private readonly UdpListenerService _listener;
        private readonly DmxRoutingService _routingService;
        private readonly Dictionary<int, Point3D> _entityMap;
        private CancellationTokenSource? _mediaCancellation;
        private CancellationTokenSource? _videoCancellation;
        private readonly PatchMapManagerViewModel _patchMapManager;

        private System.Timers.Timer? _rainbowTimer;
        private double _frame = 0;
        public int FromEntity {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        public int ToEntity {
            get => _to;
            set => SetProperty(ref _to, value);
        }

        public byte Brightness {
            get => _brightness;
            set {
                SetProperty(ref _brightness, value);
                UpdateEffectiveColor();
            }
        }

        public Color SelectedColor {
            get => _selectedColor;
            set {
                SetProperty(ref _selectedColor, value);
                UpdateEffectiveColor();
            }
        }

        public Color EffectiveColor {
            get => _effectiveColor;
            private set => SetProperty(ref _effectiveColor, value);
        }
        public byte Red {
            get => SelectedColor.R;
            set {
                SelectedColor = Color.FromRgb(value, SelectedColor.G, SelectedColor.B);
            }
        }

        public byte Green {
            get => SelectedColor.G;
            set {
                SelectedColor = Color.FromRgb(SelectedColor.R, value, SelectedColor.B);
            }
        }

        public byte Blue {
            get => SelectedColor.B;
            set {
                SelectedColor = Color.FromRgb(SelectedColor.R, SelectedColor.G, value);
            }
        }

        public ICommand StartRainbowCommand {
            get;
        }
        public ICommand StopRainbowCommand {
            get;
        }
        public ICommand SendToPreviewCommand {
            get;
        }
        public ICommand SendToMatrixCommand {
            get;
        }
        public ICommand LoadMediaCommand {
            get;
        }
        public ICommand StopMediaCommand {
            get;
        }
        private int _from = 100;
        private int _to = 19858;
        private byte _brightness = 255;
        private readonly int _matrixWidth = 128;
        private readonly int _matrixHeight = 128;
        private Color _selectedColor = Colors.Red;
        private Color _effectiveColor = Colors.Red;

        public ConsoleWindowViewModel(UdpListenerService listener, DmxRoutingService routingService, Dictionary<int, Point3D> entityMap, PatchMapManagerViewModel patchMapManager) {
            _listener = listener;
            _routingService = routingService;
            _entityMap = entityMap;

            SendToPreviewCommand = new RelayCommand(_ => SendToPreview());
            SendToMatrixCommand = new RelayCommand(_ => SendToMatrix());
            StartRainbowCommand = new RelayCommand(_ => StartRainbowAnimation());
            StopRainbowCommand = new RelayCommand(_ => StopRainbowAnimation());
            LoadMediaCommand = new RelayCommand(_ => LoadMediaFile());
            StopMediaCommand = new RelayCommand(_ => StopMedia());

            UpdateEffectiveColor();
            _patchMapManager = patchMapManager;
        }

        private void UpdateEffectiveColor() {
            byte r = (byte)(SelectedColor.R * Brightness / 255);
            byte g = (byte)(SelectedColor.G * Brightness / 255);
            byte b = (byte)(SelectedColor.B * Brightness / 255);
            EffectiveColor = Color.FromRgb(r, g, b);
        }

        private UpdateMessage BuildUpdateMessage() {
            var pixels = new List<Pixel>();

            foreach (var entry in _patchMapManager.Entries) {
                for (int i = entry.EntityStart; i <= entry.EntityEnd; i++) {
                    pixels.Add(new Pixel((ushort)i, EffectiveColor.R, EffectiveColor.G, EffectiveColor.B));
                }
            }
            return new UpdateMessage(pixels);
        }



        private void SendToPreview() {
            var msg = BuildUpdateMessage();
            _listener.SimulateUpdate(msg);

        }

        private void SendToMatrix() {
            var msg = BuildUpdateMessage();
            _routingService?.RouteUpdate(msg);
        }
        private string _selectedMode = "Solid";
        public string SelectedMode {
            get => _selectedMode;
            set => SetProperty(ref _selectedMode, value);
        }

        public List<string> Modes {
            get;
        } = new() {
                "Solid", "Blink", "Gradient"
                  };

        private void StartRainbowAnimation() {
            _rainbowTimer?.Stop();

            _frame = 0;
            _rainbowTimer = new System.Timers.Timer(1000.0 / 60.0); // pour calculer les fps il faut 1000ms / par le nombre de fps voulu
            _rainbowTimer.Elapsed += (s, e) => {
                var msg = BuildRainbowMessage(_frame);
                _listener.SimulateUpdate(msg);
                _routingService.RouteUpdate(msg);
                _frame += 5;
            };
            _rainbowTimer.Start();
        }
        private UpdateMessage BuildRainbowMessage(double frameOffset) {
            var pixels = new List<Pixel>();
            ushort start = (ushort)FromEntity;
            ushort end = (ushort)ToEntity;
            int count = end - start + 1;

            for (int i = 0; i < count; i++) {
                double hue = (i * 360.0 / count + frameOffset) % 360;
                var color = HsvToRgb(hue, 1, 1);
                ushort entity = (ushort)(start + i);
                pixels.Add(new Pixel(entity, color.R, color.G, color.B));
            }

            return new UpdateMessage(pixels);
        }
        private Color HsvToRgb(double h, double s, double v) {
            h = h % 360;
            int i = (int)(h / 60);
            double f = h / 60 - i;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            double r = 0, g = 0, b = 0;

            switch (i % 6) {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }

            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        private void StopRainbowAnimation() {
            _rainbowTimer?.Stop();
            _rainbowTimer?.Dispose();
            _rainbowTimer = null;
        }
        private void StopMedia() {
            _mediaCancellation?.Cancel();
            _mediaCancellation = null;
            _videoCancellation?.Cancel();
            _videoCancellation = null;
        }

        private void LoadMediaFile() {
            var dialog = new OpenFileDialog {
                Filter = "Images and Videos|*.png;*.jpg;*.bmp;*.gif;*.mp4;*.avi"
            };

            if (dialog.ShowDialog() == true) {
                string path = dialog.FileName;
                // À ce stade, tu dispatches la suite du traitement :
                _ = ProcessMediaFile(path);
            }
        }
        private async Task ProcessMediaFile(string path) {
            if (path.EndsWith(".gif")) {
                await PlayGif(path);
            }
            else if (path.EndsWith(".mp4") || path.EndsWith(".avi")) {
                await PlayVideo(path);
            }
            else {
                await SendImageFrame(path);
            }
        }

        private async Task SendImageFrame(string path) {
            using var bitmap = new System.Drawing.Bitmap(path);

            var resized = new System.Drawing.Bitmap(_matrixWidth, _matrixHeight);
            using (var g = System.Drawing.Graphics.FromImage(resized)) {
                g.DrawImage(bitmap, 0, 0, _matrixWidth, _matrixHeight);
            }

            var pixels = new List<Pixel>();
            foreach (var kvp in _entityMap) {
                int entityId = kvp.Key;
                var pos = kvp.Value;

                int x = (int)pos.X;
                int y = (int)pos.Y;
                if (x >= 0 && x < resized.Width && y >= 0 && y < resized.Height) {
                    System.Drawing.Color color = resized.GetPixel(x, y);
                    pixels.Add(new Pixel((ushort)entityId, color.R, color.G, color.B));
                }
            }

            var updateMsg = new UpdateMessage(pixels);

            _listener.SimulateUpdate(updateMsg);
            _routingService.RouteUpdate(updateMsg);
        }



        private async Task PlayGif(string path) {
            StopMedia(); // Stoppe une animation précédente si elle tourne

            _mediaCancellation = new CancellationTokenSource();
            var token = _mediaCancellation.Token;

            using var gif = System.Drawing.Image.FromFile(path);
            var dimension = new System.Drawing.Imaging.FrameDimension(gif.FrameDimensionsList[0]);
            int frameCount = gif.GetFrameCount(dimension);
            int delayMs = 1000 / 60; // 60 FPS par défaut

            try {
                while (!token.IsCancellationRequested) {
                    for (int i = 0; i < frameCount; i++) {
                        gif.SelectActiveFrame(dimension, i);

                        using var frame = new System.Drawing.Bitmap(gif);
                        var resized = new System.Drawing.Bitmap(_matrixWidth, _matrixHeight);
                        using (var g = System.Drawing.Graphics.FromImage(resized)) {
                            g.DrawImage(frame, 0, 0, _matrixWidth, _matrixHeight);
                        }

                        var pixels = new List<Pixel>();
                        foreach (var kvp in _entityMap) {
                            int entityId = kvp.Key;
                            var pos = kvp.Value;

                            int x = (int)pos.X;
                            int y = (int)pos.Y;
                            if (x >= 0 && x < resized.Width && y >= 0 && y < resized.Height) {
                                var color = resized.GetPixel(x, y);
                                pixels.Add(new Pixel((ushort)entityId, color.R, color.G, color.B));
                            }
                        }

                        var updateMsg = new UpdateMessage(pixels);
                        _listener.SimulateUpdate(updateMsg);
                        _routingService.RouteUpdate(updateMsg);

                        await Task.Delay(delayMs, token); // interruption propre
                    }
                }
            }
            catch (TaskCanceledException) {
                Debug.WriteLine("Lecture du GIF arrêtée.");
            }
        }


        private async Task PlayVideo(string path) {
            StopMedia(); // Stoppe une vidéo ou GIF en cours
            _videoCancellation = new CancellationTokenSource();
            var token = _videoCancellation.Token;

            using var capture = new VideoCapture(path);
            if (!capture.IsOpened()) {
                Debug.WriteLine($"Impossible d’ouvrir la vidéo {path}");
                return;
            }

            int delay = (int)(1000.0 / 60);

            using var frame = new Mat();

            try {
                while (!token.IsCancellationRequested) {
                    if (!capture.Read(frame) || frame.Empty()) {
                        capture.Set(VideoCaptureProperties.PosFrames, 0);
                        continue;
                    }

                    using var resized = frame.Resize(new OpenCvSharp.Size(_matrixWidth, _matrixHeight));
                    var updateMsg = BuildUpdateFromMat(resized);
                    _listener.SimulateUpdate(updateMsg);
                    _routingService.RouteUpdate(updateMsg);

                    await Task.Delay(delay, token);
                }
            }
            catch (TaskCanceledException) {
                Debug.WriteLine("Lecture de la vidéo arrêtée.");
            }
        }
        private UpdateMessage BuildUpdateFromMat(Mat mat) {
            var pixels = new List<Pixel>();

            foreach (var kvp in _entityMap) {
                int entityId = kvp.Key;
                var pos = kvp.Value;

                int x = (int)pos.X;
                int y = (int)pos.Y;

                if (x >= 0 && x < mat.Width && y >= 0 && y < mat.Height) {
                    var color = mat.At<Vec3b>(y, x);
                    pixels.Add(new Pixel((ushort)entityId, color[2], color[1], color[0])); // BGR → RGB
                }
            }

            return new UpdateMessage(pixels);
        }

    }
}
