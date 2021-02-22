using Phork.Data;
using System.Collections.ObjectModel;

namespace Phork.Blazor.Reactivity.Tests.Models
{
    public class SecondBindable : BindableBase
    {
        private string _value;
        public string Value
        {
            get => this._value;
            set => this.SetProperty(ref this._value, value);
        }

        public ObservableCollection<string> _values = new ObservableCollection<string>();
        public ObservableCollection<string> Values
        {
            get => this._values;
            set => this.SetProperty(ref this._values, value);
        }
    }
}
