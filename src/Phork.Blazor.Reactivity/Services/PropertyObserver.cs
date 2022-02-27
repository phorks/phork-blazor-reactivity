using System;
using System.Collections.Generic;
using System.ComponentModel;
using Phork.Blazor.Lifecycle;

namespace Phork.Blazor.Services;

internal class PropertyObserver : IPropertyObserver
{
    private bool isDisposed;

    private readonly Dictionary<INotifyPropertyChanged, NotifyPropertyChangedElement> elements = new();

    public event EventHandler? ObservedPropertyChanged;

    public void Observe(INotifyPropertyChanged notifier, string propertyName)
    {
        this.AssertNotDisposed();

        if (!this.elements.TryGetValue(notifier, out var element))
        {
            element = new NotifyPropertyChangedElement(notifier, this.OnElementPropertyChanged);

            this.elements[notifier] = element;
        }

        element.Observe(propertyName);
    }

    public void OnAfterRender()
    {
        this.AssertNotDisposed();

        this.elements.NotifyCycleEndedAndRemoveDisposedElements();
    }

    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        foreach (var element in this.elements.Values)
        {
            element.Dispose();
        }

        this.isDisposed = true;
    }

    private void OnElementPropertyChanged(INotifyPropertyChanged notifier, string? propertyName)
    {
        this.ObservedPropertyChanged?.Invoke(this, EventArgs.Empty);
    }

    private void AssertNotDisposed()
    {
        if (this.isDisposed)
        {
            throw new ObjectDisposedException(nameof(PropertyObserver));
        }
    }
}