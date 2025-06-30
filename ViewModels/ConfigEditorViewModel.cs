using System.Collections.ObjectModel;
using System.Windows.Input;
using Core.Dtos;
using Services.Config;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class ConfigEditorViewModel : BaseViewModel {
        readonly IJsonFileService<ConfigDto> _service;

        public ObservableCollection<ConfigItemViewModel> ConfigItems {
            get;
        }
        ConfigItemViewModel _selectedConfigItem;
        public ConfigItemViewModel SelectedConfigItem {
            get => _selectedConfigItem;
            set => SetProperty(ref _selectedConfigItem, value);
        }

        string _rawJson;

        public string RawJson {
            get; set;
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
            // Injection du service de lecture/écriture JSON
            _service = new JsonFileConfigService<ConfigDto>("config.json");

            ConfigItems = new ObservableCollection<ConfigItemViewModel>();
            LoadConfigCommand = new RelayCommand(_ => Load());
            SaveConfigCommand = new RelayCommand(_ => Save());
            AddItemCommand = new RelayCommand(_ => ConfigItems.Add(new ConfigItemViewModel()));
            DeleteItemCommand = new RelayCommand(
                        _ => DeleteSelected(),
                        _ => SelectedConfigItem != null
                    );

            // Chargement initial
            Load();
        }

        void Load() {
            ConfigDto cfg = _service.Load();

            ConfigItems.Clear();
            foreach (var model in cfg.Items)
                ConfigItems.Add(new ConfigItemViewModel(model));

            RawJson = _service.Serialize(cfg);
        }

        void Save() {
            var dto = new ConfigDto {
                Items = ConfigItems.Select(vm => vm.ToModel()).ToList()
            };

            _service.Save(dto);

            RawJson = _service.Serialize(dto);
        }
        void DeleteSelected() {
            if (SelectedConfigItem != null)
                ConfigItems.Remove(SelectedConfigItem);
        }
    }
}
