using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace No_Fast_No_Fun_Wpf.ViewModels
    {
    public abstract class BaseViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifie l’UI que la propriété <paramref name="propertyName"/> a changé.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Affecte la valeur, notifie si elle a changé, et retourne true si changement.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (Equals(field, value))
                return false;
            field = value!;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
