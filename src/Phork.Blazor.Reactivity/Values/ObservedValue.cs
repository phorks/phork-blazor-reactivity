using System;
using System.Collections.Specialized;
using Phork.Data;

namespace Phork.Blazor.Values;

internal sealed class ObservedValue<T> : ObservedBase<T>,
    IValueReader<T?>,
    IDisposable
{
    private bool observeCollectionRequested;

    public T Value
    {
        get => this.ValueInternal;
    }

    private bool _observesCollectionChanges;

    public bool ObservesCollectionChanges
    {
        get => this._observesCollectionChanges;
        set
        {
            if (this._observesCollectionChanges == value)
            {
                return;
            }

            this._observesCollectionChanges = value;

            if (this.Value is INotifyCollectionChanged collection)
            {
                if (value)
                {
                    collection.CollectionChanged += this.Value_CollectionChanged;
                }
                else
                {
                    collection.CollectionChanged -= this.Value_CollectionChanged;
                }
            }
        }
    }

    public ObservedValue(ReactivityEntry<T> entry) : base(entry)
    {
    }

    public void RequestCollectionObserving()
    {
        this.observeCollectionRequested = true;
    }

    protected override void OnValueCleared(T oldValue)
    {
        if (this.ObservesCollectionChanges && oldValue is INotifyCollectionChanged collection)
        {
            collection.CollectionChanged -= this.Value_CollectionChanged;
        }

        base.OnValueCleared(oldValue);
    }

    protected override void OnValueUpdated(T newValue)
    {
        base.OnValueUpdated(newValue);

        if (newValue is INotifyCollectionChanged newCollection)
        {
            newCollection.CollectionChanged += this.Value_CollectionChanged;
        }
    }

    protected override void OnRendered()
    {
        base.OnRendered();

        this.ObservesCollectionChanges = this.observeCollectionRequested;

        this.observeCollectionRequested = false;
    }

    private void Value_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.Entry.StateHasChanged();
    }
}