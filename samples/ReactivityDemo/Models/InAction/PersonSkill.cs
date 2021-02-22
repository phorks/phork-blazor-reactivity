using System.ComponentModel;

namespace ReactivityDemo.Models.InAction
{
    public class PersonSkill : INotifyPropertyChanged
    {
        public string Title { get; }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => this._isEnabled;
            set
            {
                if (value == this._isEnabled)
                    return;

                this._isEnabled = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.IsEnabled)));
            }
        }

        public PersonSkill(string title)
        {
            this.Title = title;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
