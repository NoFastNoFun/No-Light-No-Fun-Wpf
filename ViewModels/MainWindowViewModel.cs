using No_Fast_No_Fun_Wpf.Services.Network;
using Services.Config;
using Services.Matrix;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class MainWindowViewModel : BaseViewModel {
        public ObservableCollection<string> Tabs {
            get;
        }

        public MatrixPreviewViewModel Preview {
            get;
        }

        readonly UdpListenerService _listener;
        readonly ArtNetDmxController _artNetController;

        object _currentViewModel;
        public object CurrentViewModel {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand ChangeTabCommand {
            get;
        }

        readonly Dictionary<string, BaseViewModel> _panelViewModels;

        public MainWindowViewModel(UdpListenerService listener, ArtNetDmxController artNetController) {
            _listener = listener;
            _artNetController = artNetController;

            // 1) Crée UNE fois la preview, ici en 128×128
            Preview = new MatrixPreviewViewModel(_listener, width: 128, height: 128);

            // 2) Construit le dictionnaire d’onglets
            _panelViewModels = new Dictionary<string, BaseViewModel>
            {
            { "Configuration",   new ConfigEditorViewModel() },
            { "Monitoring",      new MonitoringDashboardViewModel(_listener) },
            { "PatchMap",        new PatchMapManagerViewModel() },
            { "Receivers",       new ReceiverConfigPanelViewModel(new SettingsService()) },
            { "Streams",         new StreamManagerViewModel() },
            { "Settings",        new SystemSettingsPanelViewModel() },
            { "DMX Monitor",     new DmxMonitorViewModel(_artNetController) },
            { "Preview",         Preview }
        };

            // 3) Initialise les onglets
            Tabs = new ObservableCollection<string>(_panelViewModels.Keys);

            // 4) Onglet courant par défaut
            CurrentViewModel = _panelViewModels[Tabs[0]];

            // 5) Commande de changement d’onglet
            ChangeTabCommand = new RelayCommand(param =>
            {
                if (param is string tab && _panelViewModels.TryGetValue(tab, out var vm))
                    CurrentViewModel = vm;
            });
        }
    }

}
