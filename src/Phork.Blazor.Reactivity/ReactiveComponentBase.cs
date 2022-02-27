using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Phork.Blazor.Bindings;

namespace Phork.Blazor;

public abstract class ReactiveComponentBase : ComponentBase, IReactiveComponent, IDisposable
{
    [Inject]
    protected IReactivityManager ReactivityManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        this.ReactivityManager.Initialize(this);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        this.ReactivityManager.OnAfterRender();
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

    /// <inheritdoc cref="IReactivityManager.Binding{T}(Expression{Func{T}})" />
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

    public virtual void Dispose()
    {
        this.ReactivityManager.Dispose();
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