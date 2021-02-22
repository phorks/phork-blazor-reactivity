using MvvmBlazor.ViewModel;
using System.Collections.ObjectModel;

namespace ReactivityDemo.Models.Comparison.MvvmBlazorModels
{
    public class Person : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get => this._name;
            set => this.Set(ref this._name, value);
        }

        public Dog _dog = new Dog() { Name = "Pammy" };
        public Dog Dog
        {
            get => this._dog;
            set => this.Set(ref this._dog, value);
        }


        public ObservableCollection<Skill> Skills { get; } = new ObservableCollection<Skill>();
    }
}
