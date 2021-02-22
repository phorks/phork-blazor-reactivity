using Phork.Blazor.Lifecycle;

namespace Phork.Blazor.Bindings
{
    public interface IObservedBinding : IRenderElement
    {
        IObservedBindingDescriptor Descriptor { get; }
    }

    public interface IObservedBinding<T> : IObservedBinding
    {
        T Value { get; set; }
    }
}
