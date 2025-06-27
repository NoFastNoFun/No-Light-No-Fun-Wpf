using Services.Config;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class MainWindowViewModel : BaseViewModel {
        public ObservableCollection<string> Tabs {
            get;
        }
        public object CurrentViewModel {
            get; private set;
        }
        public ICommand ChangeTabCommand {
            get;
        }

        readonly Dictionary<string, BaseViewModel> _panelViewModels;

        public MainWindowViewModel() {
            var settings = new SettingsService();

            _panelViewModels = new Dictionary<string, BaseViewModel>
            {
                { "Configuration", new ConfigEditorViewModel() },
                { "Monitoring",    new MonitoringDashboardViewModel() },
                { "PatchMap",      new PatchMapManagerViewModel() },
                { "Receivers",     new ReceiverConfigPanelViewModel(settings) },
                { "Streams",       new StreamManagerViewModel() },
                { "Settings",      new SystemSettingsPanelViewModel() }
            };

            Tabs = new ObservableCollection<string>(_panelViewModels.Keys);
            CurrentViewModel = _panelViewModels[Tabs[0]];

            ChangeTabCommand = new RelayCommand(param => {
                if (param is string tab && _panelViewModels.ContainsKey(tab))
                    SetProperty(ref CurrentViewModel, _panelViewModels[tab]);
            });
        }
    }
}
