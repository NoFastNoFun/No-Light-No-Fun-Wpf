using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Messages;
using Services.Config;
using System.Windows.Input;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class ReceiverConfigPanelViewModel : BaseViewModel {
        readonly SettingsService _settings;

        public ReceiverSettings Settings => _settings.Settings;
        public ICommand SaveCommand {
            get;
        }

        public ReceiverConfigPanelViewModel(SettingsService settings) {
            _settings = settings;
            SaveCommand = new RelayCommand(_ => _settings.Save());
        }
    }
}
