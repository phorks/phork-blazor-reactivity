using System;
using System.ComponentModel;

namespace Phork.Blazor.Services;

internal interface IPropertyObserver : IDisposable
{
    public event EventHandler? ObservedPropertyChanged;

    void Observe(INotifyPropertyChanged notifier, string propertyName);

    void OnAfterRender();
}