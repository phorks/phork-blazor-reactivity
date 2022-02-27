using System;
using System.Collections.Specialized;

namespace Phork.Blazor.Services;

internal interface ICollectionObserver : IDisposable
{
    public event EventHandler? ObservedCollectionChanged;

    void Observe(INotifyCollectionChanged collection);

    void OnAfterRender();
}