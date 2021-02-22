using ReactivityDemo.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ReactivityDemo.Models.Comparison.PhorkModels
{
    public class Person : INotifyPropertyChanged
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

        public Dog _dog = new Dog() { Name = StringHelper.RandomString() };


        public Dog Dog
        {
            get => this._dog;
            set
            {
                this._dog = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Dog)));
            }
        }

        public ObservableCollection<Skill> Skills { get; } = new ObservableCollection<Skill>();

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
