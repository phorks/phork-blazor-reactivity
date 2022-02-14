using MvvmBlazor.ViewModel;

namespace ReactivityDemo.Models.Comparison.MvvmBlazorModels
{
    public class Dog : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get => this._name;
            set => this.Set(ref this._name, value);
        }
    }
}
