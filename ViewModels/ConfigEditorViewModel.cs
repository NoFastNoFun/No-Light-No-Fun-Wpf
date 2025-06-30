using Core.Dtos;         
using Services.Config;
using No_Fast_No_Fun_Wpf.Core.Models;
using No_Fast_No_Fun_Wpf.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace No_Fast_No_Fun_Wpf.ViewModels {
public class ConfigEditorViewModel : BaseViewModel {
    readonly IConfigService _service;

    public ObservableCollection<ConfigItemViewModel> ConfigItems {
        get;
    }

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

    public ConfigEditorViewModel() {
        // Injection du service de lecture/écriture JSON
        _service = new JsonFileConfigService("config.json");

        ConfigItems = new ObservableCollection<ConfigItemViewModel>();
        LoadConfigCommand = new RelayCommand(_ => Load());
        SaveConfigCommand = new RelayCommand(_ => Save());
        AddItemCommand = new RelayCommand(_ => ConfigItems.Add(new ConfigItemViewModel()));


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

        // Met à jour l’aperçu JSON
        RawJson = _service.Serialize(dto);
    }
}
}
