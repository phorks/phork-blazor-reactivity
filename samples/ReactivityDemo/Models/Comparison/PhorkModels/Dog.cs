using System.ComponentModel;

namespace ReactivityDemo.Models.Comparison.PhorkModels
{
    public class Dog : INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get => this._name;
            set
            {
                this._name = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Name)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
