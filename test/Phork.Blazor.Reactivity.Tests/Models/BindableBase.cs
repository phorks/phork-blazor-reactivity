using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Phork.Blazor.Reactivity.Tests.Models;

public class BindableBase : INotifyPropertyChanged
{
    protected void SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value))
        {
            return;
        }

        backingField = value;

        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;
}