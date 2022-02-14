using System;

namespace Phork.Blazor.Lifecycle;

/// <summary>
/// Internal implementation of <see cref="IRenderElement"/>.
/// </summary>
internal abstract class RenderElement : IRenderElement
{
    private bool firstActivation = true;

    /// <inheritdoc/>
    public bool IsActive { get; private set; }

    /// <inheritdoc/>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc/>
    public void Touch()
    {
        this.EnsureNotDisposed();

        if (this.IsActive)
        {
            return;
        }

        this.IsActive = true;
        this.OnActivated(this.firstActivation);
        this.firstActivation = false;
    }

    /// <inheritdoc/>
    public virtual bool TryDispose()
    {
        if (this.IsActive)
        {
            this.IsActive = false;
            this.OnRendered();
            return false;
        }

        this.Dispose();
        return true;
    }

    public virtual void Dispose()
    {
        this.EnsureNotDisposed();
        this.IsDisposed = true;
    }

    /// <summary>
    /// Called when the element is accessed for the first time in each render cycle.
    /// </summary>
    protected virtual void OnActivated(bool firstActivation)
    {
    }

    /// <summary>
    /// Called after the end of each render cycle in which the element has been active.
    /// </summary>
    protected virtual void OnRendered()
    {
    }

    protected void EnsureNotDisposed()
    {
        if (this.IsDisposed)
        {
            throw new ObjectDisposedException(nameof(RenderElement));
        }
    }
}