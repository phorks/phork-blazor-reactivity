using Microsoft.AspNetCore.Components;

namespace Phork.Blazor;

/// <summary>
/// Represents a reactive UI component.
/// </summary>
public interface IReactiveComponent : IComponent
{
    /// <summary>
    /// Configures bindings in each render cycle.
    /// </summary>
    void ConfigureBindings();

    /// <summary>
    /// Notifies the component that its state has changed.
    /// </summary>
    void StateHasChanged();
}