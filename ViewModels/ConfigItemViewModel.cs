using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Models;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class ConfigItemViewModel : INotifyPropertyChanged {
        ushort _startEntityId, _endEntityId;
        byte _universe;
        string _controllerIp = "0.0.0.0";

        public ushort StartEntityId {
            get => _startEntityId;
            set => Set(ref _startEntityId, value);
        }
        public ushort EndEntityId {
            get => _endEntityId;
            set => Set(ref _endEntityId, value);
        }
        public byte Universe {
            get => _universe;
            set => Set(ref _universe, value);
        }
        public string ControllerIp {
            get => _controllerIp;
            set => Set(ref _controllerIp, value);
        }

        public ConfigItemViewModel() {
        }

        public ConfigItemViewModel(ConfigItem model) {
            StartEntityId = model.StartEntityId;
            EndEntityId = model.EndEntityId;
            Universe = model.Universe;
            ControllerIp = model.ControllerIp;
        }

        public ConfigItem ToModel() =>
            new ConfigItem(StartEntityId, EndEntityId, Universe, ControllerIp);

        public event PropertyChangedEventHandler? PropertyChanged;
        void Set<T>(ref T field, T value, [CallerMemberName] string? prop = null) {
            if (!Equals(field, value)) {
                field = value!;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
