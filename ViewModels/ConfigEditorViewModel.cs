using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Core.Dtos;
using Core.Models;
using Services.Config;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class ConfigEditorViewModel : BaseViewModel {
        private readonly IJsonFileService<ConfigDto> _service;

        public ObservableCollection<ConfigItemViewModel> ConfigItems {
            get;
        }
        ConfigItemViewModel _selectedConfigItem;
        public ConfigItemViewModel SelectedConfigItem {
            get => _selectedConfigItem;
            set => SetProperty(ref _selectedConfigItem, value);
        }

        public ICommand LoadConfigCommand {
            get;
        }
        public ICommand SaveConfigCommand {
            get;
        }
        public ICommand AddItemCommand {
            get;
        }
        public ICommand DeleteItemCommand {
            get;
        }

        public ConfigEditorViewModel() {
            _service = new JsonFileConfigService<ConfigDto>("config.json");

            ConfigItems = new ObservableCollection<ConfigItemViewModel>();
            LoadConfigCommand = new RelayCommand(_ => Load());
            SaveConfigCommand = new RelayCommand(_ => Save());
            AddItemCommand = new RelayCommand(_ => ConfigItems.Add(new ConfigItemViewModel()));
            DeleteItemCommand = new RelayCommand(
                _ => DeleteSelected(),
                _ => SelectedConfigItem != null
            );
            Load();
        }

        void Load() {
            var cfg = _service.Load();
            ConfigItems.Clear();
            foreach (var model in cfg.Items)
                ConfigItems.Add(new ConfigItemViewModel(model));
        }

        void Save() {
            var dto = new ConfigDto {
                Items = ConfigItems.Select(vm => vm.ToModel()).ToList()
            };
            _service.Save(dto);
        }

        void DeleteSelected() {
            if (SelectedConfigItem != null)
                ConfigItems.Remove(SelectedConfigItem);
        }
    }
}
