using System.Windows.Input;
using No_Fast_No_Fun_Wpf.Services.Network;

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class SystemSettingsPanelViewModel : BaseViewModel {
        private readonly UdpListenerService _listener;

        private int _selectedPort;
        public int SelectedPort {
            get => _selectedPort;
            set => SetProperty(ref _selectedPort, value);
        }

        private int _selectedUniverse;
        public int SelectedUniverse {
            get => _selectedUniverse;
            set => SetProperty(ref _selectedUniverse, value);
        }

        public ICommand ApplySettingsCommand {
            get;
        }

        public SystemSettingsPanelViewModel(UdpListenerService listener) {
            _listener = listener;

            // Init with current values
            SelectedPort = 8765;          
            SelectedUniverse = 1;         

            ApplySettingsCommand = new RelayCommand(_ => ApplySettings());
        }

        private void ApplySettings() {
            _listener.Stop();
            _listener.UniverseToListen = SelectedUniverse;
            _listener.Start(SelectedPort);
        }
    }
}

