namespace Phork.Blazor.Bindings;

/// <summary>
/// Represents an observed binding.
/// </summary>
/// <typeparam name="T">Type of the binding value.</typeparam>
public interface IObservedBinding<T>
{
    /// <summary>
    /// Gets or sets the value of the binding.
    /// </summary>
    T Value { get; set; }
}