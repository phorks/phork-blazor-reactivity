using System;
using System.Collections.Specialized;

namespace Phork.Blazor.Lifecycle;

internal class NotifyCollectionChangedElement : LifecycleElement
{
    private readonly INotifyCollectionChanged collection;
    private readonly Action<INotifyCollectionChanged, NotifyCollectionChangedEventArgs> callback;

    public NotifyCollectionChangedElement(INotifyCollectionChanged collection, Action<INotifyCollectionChanged, NotifyCollectionChangedEventArgs> callback)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(callback);

        this.collection = collection;
        this.callback = callback;
    }

    protected override void OnActivated(bool firstActivation)
    {
        base.OnActivated(firstActivation);

        if (firstActivation)
        {
            this.collection.CollectionChanged += this.Collection_CollectionChanged;
        }
    }

    protected override void OnDisposing()
    {
        this.collection.CollectionChanged -= this.Collection_CollectionChanged;

        base.OnDisposing();
    }

    private void Collection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.callback.Invoke(this.collection, e);
    }
}