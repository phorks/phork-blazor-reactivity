using System;

namespace Phork.Blazor.Lifecycle;

/// <summary>
/// Internal implementation of <see cref="ILifecycleElement"/>.
/// </summary>
internal abstract class LifecycleElement : ILifecycleElement
{
    private bool firstActivation = true;

    /// <inheritdoc/>
    public bool IsActive { get; private set; }

    /// <inheritdoc/>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc/>
    public void Touch()
    {
        this.AssertNotDisposed();

        this.OnTouched();

        if (this.IsActive)
        {
            return;
        }

        this.IsActive = true;
        this.OnActivated(this.firstActivation);
        this.firstActivation = false;
    }

    /// <inheritdoc/>
    public void NotifyCycleEnded()
    {
        this.AssertNotDisposed();

        if (this.IsActive && this.ShouldSurvive())
        {
            this.IsActive = false;
            this.OnSurvived();
        }
        else
        {
            this.IsActive = false;
            this.Dispose();
        }
    }

    public void Dispose()
    {
        if (this.IsDisposed)
        {
            return;
        }

        this.OnDisposing();
        this.IsDisposed = true;
    }

    /// <summary>
    /// Returns a value indicating whether the active element should survive from getting disposed.
    /// </summary>
    /// <returns>A <see cref="bool"/> indicating whether element should survive.</returns>
    protected virtual bool ShouldSurvive()
    {
        return true;
    }

    /// <summary>
    /// Called when the element is touched regardless of whether the element has already been
    /// activated in that render cycle or not.
    /// </summary>
    protected virtual void OnTouched()
    {
    }

    /// <summary>
    /// Called when the element is touched for the first time in each render cycle.
    /// </summary>
    protected virtual void OnActivated(bool firstActivation)
    {
    }

    /// <summary>
    /// Called after the end of each render cycle in which the element has been active.
    /// </summary>
    protected virtual void OnSurvived()
    {
    }

    /// <summary>
    /// Called after the end of the first render cycle in which the element has not been active or
    /// the return value of <see cref="ShouldSurvive"/> has been <see langword="false"/>. The
    /// element will then become unusable.
    /// </summary>
    protected virtual void OnDisposing()
    {
    }

    protected void AssertNotDisposed()
    {
        if (this.IsDisposed)
        {
            throw new ObjectDisposedException(nameof(LifecycleElement));
        }
    }
}