using System;

namespace Phork.Blazor.Lifecycle;

/// <summary>
/// Represents an element that is expected to be used in a Blazor component's render tree.
/// </summary>
/// <remarks>
/// The element will be created the first time it gets accessed by the render tree. It is supposed
/// to be reused in consequent renders. If in any render cycle the element is not considered to be
/// active (i.e, the code that uses the element is out of reach) the element will be disposed of and
/// become unusable.
/// </remarks>
public interface IRenderElement : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the element has been active in the current render cycle.
    /// (i.e, <see cref="Touch"/> has been called at least once since its creation or last call of
    /// <see cref="TryDispose"/>).
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Gets a value indicating whether the element is disposed of. (i.e, after each render, every
    /// inactive element will be disposed of. Disposed elements are not supposed to be used again.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Called every time the element is accessed by the render tree.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the element is disposed of.</exception>
    void Touch();

    /// <summary>
    /// Tries to dispose the element.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the component is disposed of; <see langword="false"/> otherwise.
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the element is already disposed of.
    /// </exception>
    /// <remarks>
    /// If this element has not been active in the current render cycle (i.e., <see cref="Touch"/>
    /// has not been called at least once since the creation of the element or the previous call of
    /// the method), it will be disposed of and <see langword="true"/> will be returned. Otherwise,
    /// the element will be marked as inactive (i.e, <see cref="IsActive"/> will become <see
    /// langword="false"/>) and <see langword="false"/> will be returned.
    /// </remarks>
    bool TryDispose();
}