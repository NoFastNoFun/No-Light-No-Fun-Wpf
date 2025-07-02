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
        public ObservableCollection<StreamEntry> Streams {
            get;
        }

        public ICommand AddStreamCommand {
            get;
        }
        public ICommand RemoveStreamCommand {
            get;
        }
        public ICommand ToggleStreamCommand {
            get;
        }

        private StreamEntry _selectedStream;
        public StreamEntry SelectedStream {
            get => _selectedStream;
            set => SetProperty(ref _selectedStream, value);
        }

        public StreamManagerViewModel() {
            Streams = new ObservableCollection<StreamEntry>
            {
                new StreamEntry { Name = "Cam Entrée", Url = "rtsp://192.168.1.10/stream", IsActive = false },
                new StreamEntry { Name = "Cam Scene", Url = "rtsp://192.168.1.11/stream", IsActive = true }
            };

            AddStreamCommand = new RelayCommand(_ => AddStream());
            RemoveStreamCommand = new RelayCommand(_ => RemoveStream(), _ => SelectedStream != null);
            ToggleStreamCommand = new RelayCommand(_ => ToggleStream(), _ => SelectedStream != null);
        }

        private void AddStream() {
            var newStream = new StreamEntry { Name = "New Stream", Url = "", IsActive = false };
            Streams.Add(newStream);
            SelectedStream = newStream;
        }

        private void RemoveStream() {
            if (SelectedStream != null)
                Streams.Remove(SelectedStream);
        }

        private void ToggleStream() {
            if (SelectedStream != null)
                SelectedStream.IsActive = !SelectedStream.IsActive;
        }
    }
}
