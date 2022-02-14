using System.ComponentModel;

namespace ReactivityDemo.Models.Comparison.PhorkModels
{
    public class Skill : INotifyPropertyChanged
    {
        private string _title;
        public string Title
        {
            get => this._title;
            set
            {
                this._title = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Title)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
