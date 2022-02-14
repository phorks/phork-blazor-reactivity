using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ReactivityDemo.Models.InAction;

public class Person : INotifyPropertyChanged
{
    private string _name;

    public string Name
    {
        get => this._name;
        set
        {
            if (value == this._name)
                return;

            this._name = value;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Name)));
        }
    }

    private ObservableCollection<PersonSkill> _skills = new();

    public ObservableCollection<PersonSkill> Skills
    {
        get => this._skills;
        set
        {
            if (value == this._skills)
                return;

            this._skills = value;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Skills)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}