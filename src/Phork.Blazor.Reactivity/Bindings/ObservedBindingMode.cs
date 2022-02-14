namespace Phork.Blazor;

/// <summary>
/// Describes binding mode of observed bindings.
/// </summary>
public enum ObservedBindingMode
{
    /// <summary>
    /// Updating the source value will cause the target value to update.
    /// </summary>
    OneWay,

    /// <summary>
    /// Updating either the source or the target value will cause the other value to update.
    /// </summary>
    TwoWay
}