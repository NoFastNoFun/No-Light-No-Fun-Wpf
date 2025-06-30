using System.ComponentModel;
using System.Runtime.CompilerServices;
using No_Fast_No_Fun_Wpf.Core.Models;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class ConfigItemViewModel : INotifyPropertyChanged {
        ushort _startEntityId, _endEntityId;
        byte _startUniverse, _endUniverse;
        string _controllerIp = "0.0.0.0";

        public ushort StartEntityId {
            get => _startEntityId;
            set => Set(ref _startEntityId, value);
        }
        public ushort EndEntityId {
            get => _endEntityId;
            set => Set(ref _endEntityId, value);
        }
        public byte StartUniverse {
            get => _startUniverse;
            set => Set(ref _startUniverse, value);
        }
        public byte EndUniverse {
            get => _endUniverse;
            set => Set(ref _endUniverse, value);
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
            StartUniverse = model.StartUniverse;
            EndUniverse = model.EndUniverse;
            ControllerIp = model.ControllerIp;
        }

        public ConfigItem ToModel() =>
            new ConfigItem(StartEntityId, EndEntityId,
                           StartUniverse, EndUniverse,
                           ControllerIp);

        public event PropertyChangedEventHandler? PropertyChanged;
        void Set<T>(ref T field, T value, [CallerMemberName] string? prop = null) {
            if (!Equals(field, value)) {
                field = value!;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
            }
        }
    }
}
