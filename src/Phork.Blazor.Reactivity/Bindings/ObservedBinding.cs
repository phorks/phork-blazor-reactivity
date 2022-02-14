using System;
using System.Collections.Generic;
using Phork.Data;

namespace Phork.Blazor.Bindings;

internal sealed class ObservedBinding<TSource, TTarget> : ObservedBase<TSource>,
    IObservedBinding<TTarget>,
    IValueReader<TTarget>,
    IValueWriter<TTarget>
{
    private TTarget _value = default!;

    /// <inheritdoc/>
    public TTarget Value
    {
        get
        {
            this.EnsureNotDisposed();
            this.UpdateValue();
            return this._value;
        }
        set
        {
            if (this.Descriptor.Mode != ObservedBindingMode.TwoWay)
            {
                throw new InvalidOperationException($"Unable to set the value of the binding. Only bindings in {ObservedBindingMode.TwoWay} binding mode support modifications to the binding value.");
            }

            this.EnsureNotDisposed();

            if (EqualityComparer<TTarget>.Default.Equals(value, this._value))
            {
                return;
            }

            this._value = value;

            TSource convertedValue = this.Descriptor.Converter.ConvertBack(value);

            this.ValueInternal = convertedValue;
        }
    }

    public IObservedBindingDescriptor<TSource, TTarget> Descriptor { get; }
    IObservedBindingDescriptor IObservedBinding.Descriptor => this.Descriptor;

    /// <inheritdoc/>
    public ObservedBindingMode Mode => this.Descriptor.Mode;

    public ObservedBinding(
        ReactivityEntry<TSource> entry,
        IObservedBindingDescriptor<TSource, TTarget> bindingDescriptor)
        : base(entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(bindingDescriptor);

        if (bindingDescriptor.Mode == ObservedBindingMode.TwoWay
            && entry.MemberAccessor.IsReadOnly)
        {
            throw new InvalidOperationException($"Unable to create observed binding. A two-way binding cannot be created with a read-only source. Try using {nameof(ObservedBindingMode)}.{nameof(ObservedBindingMode.OneWay)} instead.");
        }

        this.Descriptor = bindingDescriptor;
    }

    public override void Dispose()
    {
        this._value = default!;

        base.Dispose();
    }

    protected override void OnValueUpdated(TSource newValue)
    {
        base.OnValueUpdated(newValue);

        var convertedValue = this.Descriptor.Converter.Convert(newValue);

        this._value = convertedValue;
    }
}