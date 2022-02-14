using System;
using Phork.Data;
using Phork.Data.ValueConverters;

namespace Phork.Blazor.Bindings;

internal static class ObservedBindingDescriptor
{
    public static IObservedBindingDescriptor<T, T> Create<T>(
        ObservedBindingMode mode = ObservedBindingMode.TwoWay)
    {
        return new ObservedBindingDescriptor<T, T>(
            mode,
            new IdentityValueConverter<T>());
    }

    public static IObservedBindingDescriptor<TSource, TTarget> Create<TSource, TTarget>(
        Func<TSource, TTarget> converter)
    {
        return new ObservedBindingDescriptor<TSource, TTarget>(
            ObservedBindingMode.OneWay,
            new DelegateValueConverter<TSource, TTarget>(converter));
    }

    public static IObservedBindingDescriptor<TSource, TTarget> Create<TSource, TTarget>(
        Func<TSource, TTarget> converter,
        Func<TTarget, TSource> reverseConverter)
    {
        return new ObservedBindingDescriptor<TSource, TTarget>(
            ObservedBindingMode.TwoWay,
            new DelegateValueConverter<TSource, TTarget>(converter, reverseConverter));
    }

    public static IObservedBindingDescriptor<TSource, TTarget> Create<TSource, TTarget>(
        IValueConverter<TSource, TTarget> converter,
        ObservedBindingMode mode)
    {
        return new ObservedBindingDescriptor<TSource, TTarget>(
            mode,
            converter);
    }
}

internal sealed class ObservedBindingDescriptor<TSource, TTarget> :
    IObservedBindingDescriptor<TSource, TTarget>,
    IObservedBindingDescriptor,
    IEquatable<ObservedBindingDescriptor<TSource, TTarget>>
{
    public ObservedBindingMode Mode { get; }
    public IValueConverter<TSource, TTarget> Converter { get; }

    public ObservedBindingDescriptor(ObservedBindingMode mode, IValueConverter<TSource, TTarget> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);

        this.Mode = mode;
        this.Converter = converter;
    }

    public bool Equals(ObservedBindingDescriptor<TSource, TTarget>? other)
    {
        return other is not null
            && this.Mode == other.Mode
            && this.Converter.Equals(other.Converter);
    }

    public override bool Equals(object? obj)
    {
        return obj is ObservedBindingDescriptor<TSource, TTarget> typedObj
            && this.Equals(typedObj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Mode, this.Converter);
    }
}