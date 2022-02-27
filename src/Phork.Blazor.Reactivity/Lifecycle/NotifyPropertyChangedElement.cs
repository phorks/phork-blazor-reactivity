using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Phork.Blazor.Lifecycle;

internal class NotifyPropertyChangedElement : LifecycleElement
{
    private readonly INotifyPropertyChanged notifier;
    private readonly Action<INotifyPropertyChanged, string?> callback;

    // Key: PropertyName,
    // Value: a boolean indicating whether the property has been active in the last cycle
    private readonly Dictionary<string, bool> properties = new();

    public NotifyPropertyChangedElement(INotifyPropertyChanged notifier, Action<INotifyPropertyChanged, string?> callback)
    {
        ArgumentNullException.ThrowIfNull(notifier);
        ArgumentNullException.ThrowIfNull(callback);

        this.notifier = notifier;
        this.callback = callback;
    }

    public void Observe(string property)
    {
        ArgumentNullException.ThrowIfNull(property);

        this.AssertNotDisposed();

        this.properties[property] = true;

        this.Touch();
    }

    protected override void OnActivated(bool firstActivation)
    {
        base.OnActivated(firstActivation);

        if (firstActivation)
        {
            this.notifier.PropertyChanged += this.Notifier_PropertyChanged;
        }
    }

    protected override bool ShouldSurvive()
    {
        return this.properties.Any(x => x.Value);
    }

    protected override void OnSurvived()
    {
        base.OnSurvived();

        foreach (var item in this.properties)
        {
            if (item.Value)
            {
                this.properties[item.Key] = false;
            }
            else
            {
                this.properties.Remove(item.Key);
            }
        }
    }

    protected override void OnDisposing()
    {
        this.notifier.PropertyChanged -= this.Notifier_PropertyChanged;

        base.OnDisposing();
    }

    private void Notifier_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null || this.properties.ContainsKey(e.PropertyName))
        {
            this.callback.Invoke(this.notifier, e.PropertyName);
        }
    }
}