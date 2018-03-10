// from http://sourcechord.hatenablog.com/entry/20130303/1362315081
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeLaboratory.ComponentModel
{
    [DataContract]
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected bool SetProperty<T>(ref T storage, T value, Action DoSet, [CallerMemberName] String propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            RaisePropertyChanged(propertyName);
            DoSet.Invoke();
            return true;
        }

        protected bool SetProperty<T>(T currentValue, T value, Action DoSet, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, value)) return false;
            DoSet.Invoke();
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        protected void ResetPropertyChanged()
        {
            this.PropertyChanged = null;
        }
    }
}
