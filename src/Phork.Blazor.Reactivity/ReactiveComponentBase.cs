using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Phork.Blazor.Bindings;

namespace Phork.Blazor;

/// <summary>
/// Provides the base class for reactive components. Alternatively, components may implement <see
/// cref="IReactiveComponent"/> directly.
/// </summary>
public abstract class ReactiveComponentBase : ComponentBase, IReactiveComponent, IDisposable
{
    /// <summary>
    /// Gets the reactivity manager of the component.
    /// </summary>
    [Inject]
    protected IReactivityManager ReactivityManager { get; private set; } = default!;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        this.ReactivityManager.Initialize(this);
    }

    /// <inheritdoc/>
    protected virtual void ConfigureBindings()
    {
    }

    /// <inheritdoc cref="IReactivityManager.Observed{T}(Expression{Func{T}})"/>
    protected T Observed<T>(Expression<Func<T>> valueAccessor)
    {
        return this.ReactivityManager.Observed(valueAccessor);
    }

    /// <inheritdoc cref="IReactivityManager.ObservedCollection{T}(Expression{Func{T}})"/>
    protected T ObservedCollection<T>(Expression<Func<T>> valueAccessor)
    {
        return this.ReactivityManager.ObservedCollection(valueAccessor);
    }

    /// <inheritdoc cref="IReactivityManager.Binding{T}(Expression{Func{T}})"/>
    protected IObservedBinding<T> Binding<T>(Expression<Func<T>> valueAccessor)
    {
        return this.ReactivityManager.Binding(valueAccessor);
    }

    /// <inheritdoc cref="IReactivityManager.Binding{TSource, TTarget}(Expression{Func{TSource}}, Func{TSource, TTarget}, Func{TTarget, TSource})"/>
    protected IObservedBinding<TTarget> Binding<TSource, TTarget>(
        Expression<Func<TSource>> valueAccessor,
        Func<TSource, TTarget> converter,
        Func<TTarget, TSource> reverseConverter)
    {
        return this.ReactivityManager.Binding(valueAccessor, converter, reverseConverter);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting
    /// unmanaged resources.
    /// </summary>
    /// <param name="disposing">
    /// A <see cref="bool"/> value indicating whether the method is called from <see cref="IDisposable.Dispose"/>.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.ReactivityManager.Dispose();
        }
    }

    void IReactiveComponent.StateHasChanged()
    {
        this.InvokeAsync(this.StateHasChanged);
    }

    void IReactiveComponent.ConfigureBindings()
    {
        this.ConfigureBindings();
    }
}