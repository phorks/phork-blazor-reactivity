using MvvmBlazor.ViewModel;

namespace ReactivityDemo.Models.Comparison.MvvmBlazorModels
{
    public class Skill : ViewModelBase
    {
        private string _title;
        public string Title
        {
            get => this._title;
            set => this.Set(ref this._title, value);
        }
    }
}
