using Phork.Blazor.Lifecycle;

namespace Phork.Blazor.Bindings;

/// <summary>
/// Represents the general information of an observed binding.
/// </summary>
/// <remarks>
/// Only the general information of the binding can be accessed through this interface. In order to
/// access the value of the binding, the <see cref="IObservedBinding{T}"/> with an appropriate type
/// parameter needs to be used.
/// </remarks>
public interface IObservedBinding : IRenderElement
{
    internal IObservedBindingDescriptor Descriptor { get; }

    /// <summary>
    /// Gets a value indicating the direction of the data flow in the binding.
    /// </summary>
    ObservedBindingMode Mode { get; }
}

/// <summary>
/// Represents an observed binding.
/// </summary>
/// <typeparam name="T">Type of the binding value.</typeparam>
public interface IObservedBinding<T> : IObservedBinding
{
    /// <summary>
    /// Gets or sets the value of the binding.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown as a response to setting the value, if the binding is not in <see
    /// cref="ObservedBindingMode.TwoWay"/> mode.
    /// </exception>
    /// <exception cref="System.ObjectDisposedException">
    /// Thrown if the binding is disposed. (i.e., <see cref="IRenderElement.IsDisposed"/> is <see langword="true"/>).
    /// </exception>
    /// <remarks>
    /// The set method of the property only works if <see cref="IObservedBinding.Mode"/> property is
    /// <see cref="ObservedBindingMode.TwoWay"/>.
    /// </remarks>
    T Value { get; set; }
}