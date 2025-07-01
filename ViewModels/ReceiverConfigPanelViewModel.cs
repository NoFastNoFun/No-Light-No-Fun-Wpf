using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Messages;
using Services.Config;
using System.Windows.Input;
using Core.Dtos;
using Core.Models;
using System.Collections.ObjectModel;

namespace No_Fast_No_Fun_Wpf.ViewModels {
    public class ReceiverConfigPanelViewModel : BaseViewModel {
        readonly IJsonFileService<DmxSettingsDto> _service;

        public ObservableCollection<DmxRouterSettings> Routers {
            get;
        }
        DmxRouterSettings _selectedRouter;
        public DmxRouterSettings SelectedRouter {
            get => _selectedRouter;
            set => SetProperty(ref _selectedRouter, value);
        }

        public ICommand LoadCommand {
            get;
        }
        public ICommand SaveCommand {
            get;
        }
        public ICommand AddRouterCmd {
            get;
        }
        public ICommand DelRouterCmd {
            get;
        }
        public ICommand AddRangeCmd {
            get;
        }
        public ICommand DelRangeCmd {
            get;
        }

        public ReceiverConfigPanelViewModel() {
            _service = new JsonFileConfigService<DmxSettingsDto>("dmxsettings.json");
            Routers = new ObservableCollection<DmxRouterSettings>();

            LoadCommand = new RelayCommand(_ => Load());
            SaveCommand = new RelayCommand(_ => Save());
            AddRouterCmd = new RelayCommand(_ => Routers.Add(new DmxRouterSettings()));
            DelRouterCmd = new RelayCommand(_ => {
                if (SelectedRouter != null)
                    Routers.Remove(SelectedRouter);
            }, _ => SelectedRouter != null);

            AddRangeCmd = new RelayCommand(_ => {
                SelectedRouter?.Universes.Add(new UniverseMap());
            }, _ => SelectedRouter != null);

            DelRangeCmd = new RelayCommand(_ => {
                // il te faudra stocker un SelectedRange, jette un œil ci-dessous
            }, _ => SelectedRouter?.Universes.Any() == true);

            Load();
        }

        void Load() {
            var dto = _service.Load();
            Routers.Clear();
            foreach (var r in dto.Routers)
                Routers.Add(r);
            SelectedRouter = Routers.FirstOrDefault();
        }

        void Save() {
            var dto = new DmxSettingsDto { Routers = Routers.ToList() };
            _service.Save(dto);
        }
    }
}
