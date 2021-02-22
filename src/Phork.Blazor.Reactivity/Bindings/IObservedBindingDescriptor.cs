using Phork.Data;

namespace Phork.Blazor.Bindings
{
    public interface IObservedBindingDescriptor
    {
        ObservedBindingMode Mode { get; }
    }

    public interface IObservedBindingDescriptor<TSource, TTarget> :
        IObservedBindingDescriptor
    {
        IValueConverter<TSource, TTarget> Converter { get; }
    }
}
