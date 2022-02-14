using System;
using System.Collections.Generic;
using Phork.Blazor.Lifecycle;

namespace Phork.Blazor;

internal abstract class ObservedBase<T> : RenderElement
{
    protected ReactivityEntry<T> Entry { get; }

    private bool hasValue = false;

    private T? cachedValue = default!;

    protected T ValueInternal
    {
        get
        {
            this.UpdateValue();
            return this.cachedValue!;
        }
        set
        {
            this.EnsureNotDisposed();
            this.Entry.MemberAccessor.Value = value;
        }
    }

    public ObservedBase(ReactivityEntry<T> entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        this.Entry = entry;
    }

    public void ClearValue()
    {
        this.EnsureNotDisposed();

        if (!this.hasValue)
        {
            return;
        }

        var oldValue = this.cachedValue;

        this.cachedValue = default;
        this.hasValue = false;

        this.OnValueCleared(oldValue!);
    }

    protected void UpdateValue()
    {
        this.EnsureNotDisposed();

        var newValue = this.Entry.MemberAccessor.Value;

        if (this.hasValue && EqualityComparer<T>.Default.Equals(newValue, this.cachedValue))
        {
            return;
        }

        var shouldClearOldValue = this.hasValue;

        this.hasValue = true;

        var oldValue = this.cachedValue;

        this.cachedValue = newValue;

        if (shouldClearOldValue)
        {
            this.OnValueCleared(oldValue!);
        }

        this.OnValueUpdated(this.cachedValue);
    }

    protected virtual void OnValueCleared(T oldValue)
    {
    }

    protected virtual void OnValueUpdated(T newValue)
    {
    }

    public override void Dispose()
    {
        var cachedValue = this.cachedValue;
        this.cachedValue = default!;

        if (cachedValue is not null)
        {
            this.OnValueCleared(cachedValue);
        }

        base.Dispose();
    }
}