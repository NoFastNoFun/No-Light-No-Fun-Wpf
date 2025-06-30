using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Services.Config;
using System.Windows.Input;
using Core.Dtos;

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class PatchMapManagerViewModel : BaseViewModel {
        readonly IJsonFileService<PatchMapDto> _patchService;

        public ObservableCollection<PatchMapEntryViewModel> Entries {
            get;
        }
        public ICommand LoadCommand {
            get;
        }
        public ICommand SaveCommand {
            get;
        }
        public ICommand AddCommand {
            get;
        }
        public ICommand DeleteCommand {
            get;
        }

        PatchMapEntryViewModel _selected;
        public PatchMapEntryViewModel SelectedEntry {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        public PatchMapManagerViewModel() {
            _patchService = new JsonFileConfigService<PatchMapDto>("patchmap.json");
            Entries = new ObservableCollection<PatchMapEntryViewModel>();

            LoadCommand = new RelayCommand(_ => Load());
            SaveCommand = new RelayCommand(_ => Save());
            AddCommand = new RelayCommand(_ => Entries.Add(new PatchMapEntryViewModel()));
            DeleteCommand = new RelayCommand(_ => {
                if (SelectedEntry != null)
                    Entries.Remove(SelectedEntry);
            }, _ => SelectedEntry != null);

            Load();
        }

        void Load() {
            var dto = _patchService.Load(); // DTO doit contenir List<PatchMapEntryDto>
            Entries.Clear();
            foreach (var model in dto.Items)
                Entries.Add(new PatchMapEntryViewModel(model));
        }

        void Save() {
            var dto = new PatchMapDto {
                Items = Entries.Select(vm => vm.ToModel()).ToList()
            };
            _patchService.Save(dto);
        }
    }
}
