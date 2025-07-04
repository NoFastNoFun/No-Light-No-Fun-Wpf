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

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class ConsoleWindowViewModel : BaseViewModel {
        private readonly UdpListenerService _listener;
        private readonly DmxRoutingService _routingService;

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

        public ConsoleWindowViewModel(UdpListenerService listener, DmxRoutingService routingService, Dictionary<int , (int x, int y)> entitymap) {
            _listener = listener;
            _routingService = routingService;

            SendToPreviewCommand = new RelayCommand(_ => SendToPreview());
            SendToMatrixCommand = new RelayCommand(_ => SendToMatrix());

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
            Debug.WriteLine($"Preview sent: {msg.Pixels.Count} pixels from {FromEntity} to {ToEntity} with color {EffectiveColor}");

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

    }
}
