using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;
using System.Windows.Input;

namespace No_Fast_No_Fun_Wpf.ViewModels
{
    public class StreamManagerViewModel : BaseViewModel {
        public ObservableCollection<StreamItem> Streams {
            get;
        }

        StreamItem? _selectedStream;
        public StreamItem? SelectedStream {
            get => _selectedStream;
            set {
                if (SetProperty(ref _selectedStream, value)) {
                    (RemoveStreamCmd as RelayCommand)?.RaiseCanExecuteChanged();
                    (ToggleStreamCmd as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand AddStreamCmd {
            get;
        }
        public ICommand RemoveStreamCmd {
            get;
        }
        public ICommand ToggleStreamCmd {
            get;
        }

        public StreamManagerViewModel() {
            Streams = new ObservableCollection<StreamItem>();

            AddStreamCmd = new RelayCommand(_ => AddStream());
            RemoveStreamCmd = new RelayCommand(_ => RemoveStream(), _ => SelectedStream != null);
            ToggleStreamCmd = new RelayCommand(_ => ToggleStream(), _ => SelectedStream != null);
        }

        void AddStream() {
            var stream = new StreamItem {
                Name = $"Flux {Streams.Count + 1}",
                Universe = 0,
                IsActive = false
            };
            Streams.Add(stream);
            SelectedStream = stream;
        }

        void RemoveStream() {
            if (SelectedStream != null) {
                Streams.Remove(SelectedStream);
                SelectedStream = null;
            }
        }

        void ToggleStream() {
            if (SelectedStream != null)
                SelectedStream.IsActive = !SelectedStream.IsActive;
            // Notify UI if needed (StreamItem pourrait implémenter INotifyPropertyChanged si modifié dynamiquement)
        }
    }
}
