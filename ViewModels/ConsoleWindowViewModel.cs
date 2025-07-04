using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Messages;
using Core.Models;
using No_Fast_No_Fun_Wpf.Services.Network;
using Services.Matrix;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class ConsoleWindowViewModel : BaseViewModel {
        private readonly UdpListenerService _listener;
        private readonly DmxRoutingService _routingService;

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

        private int _from = 100;
        private int _to = 19858;
        private byte _brightness = 255;
        private Color _selectedColor = Colors.Red;
        private Color _effectiveColor = Colors.Red;

        public ConsoleWindowViewModel(UdpListenerService listener, DmxRoutingService routingService, Dictionary<int, (int x, int y)> entitymap) {
            _listener = listener;
            _routingService = routingService;

            SendToPreviewCommand = new RelayCommand(_ => SendToPreview());
            SendToMatrixCommand = new RelayCommand(_ => SendToMatrix());
            StartRainbowCommand = new RelayCommand(_ => StartRainbowAnimation());
            StopRainbowCommand = new RelayCommand(_ => StopRainbowAnimation());


            UpdateEffectiveColor();
        }

        private void UpdateEffectiveColor() {
            byte r = (byte)(SelectedColor.R * Brightness / 255);
            byte g = (byte)(SelectedColor.G * Brightness / 255);
            byte b = (byte)(SelectedColor.B * Brightness / 255);
            EffectiveColor = Color.FromRgb(r, g, b);
        }

        private UpdateMessage BuildUpdateMessage() {
            var pixels = new List<Pixel>();
            ushort start = (ushort)FromEntity;
            ushort end = (ushort)ToEntity;

            for (ushort i = start; i <= end; i++) {
                pixels.Add(new Pixel(i, EffectiveColor.R, EffectiveColor.G, EffectiveColor.B));
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

    }
}
