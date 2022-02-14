using Phork.Data;

namespace Phork.Blazor.Bindings;

internal interface IObservedBindingDescriptor
{
    ObservedBindingMode Mode { get; }
}

internal interface IObservedBindingDescriptor<TSource, TTarget> :
    IObservedBindingDescriptor
{
    IValueConverter<TSource, TTarget> Converter { get; }
}