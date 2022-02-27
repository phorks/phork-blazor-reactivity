﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Phork.Blazor.Lifecycle;

namespace Phork.Blazor.Services;

internal class CollectionObserver : ICollectionObserver
{
    private bool isDisposed;
    private readonly Dictionary<INotifyCollectionChanged, NotifyCollectionChangedElement> elements = new();

    public event EventHandler? ObservedCollectionChanged;

    public void Observe(INotifyCollectionChanged collection)
    {
        this.AssertNotDisposed();

        if (!this.elements.TryGetValue(collection, out var element))
        {
            element = new NotifyCollectionChangedElement(collection, this.OnElementCollectionChanged);

            this.elements[collection] = element;
        }

        element.Touch();
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

    private void OnElementCollectionChanged(INotifyCollectionChanged collection, NotifyCollectionChangedEventArgs args)
    {
        this.ObservedCollectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void AssertNotDisposed()
    {
        if (this.isDisposed)
        {
            throw new ObjectDisposedException(nameof(PropertyObserver));
        }
    }
}