using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using No_Fast_No_Fun_Wpf.Services.Network;
using System.Windows.Input;

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class SystemSettingsPanelViewModel : BaseViewModel {
        private readonly UdpListenerService _listener;

        public int SelectedPort {
            get => _selectedPort;
            set => SetProperty(ref _selectedPort, value);
        }
        private int _selectedPort = 7777;

        public int SelectedUniverse {
            get => _selectedUniverse;
            set => SetProperty(ref _selectedUniverse, value);
        }
        private int _selectedUniverse = 1;

        public ICommand ApplySettingsCommand {
            get;
        }

        public SystemSettingsPanelViewModel(UdpListenerService listener) {
            _listener = listener;
            SelectedPort = 7777;
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

