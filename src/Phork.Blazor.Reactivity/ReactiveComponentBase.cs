using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Phork.Blazor.Bindings;

namespace Phork.Blazor;

public abstract class ReactiveComponentBase : ComponentBase,
    IReactiveComponent,
    IDisposable
{
    private readonly ReactivityManager reactivityManager;

    public ReactiveComponentBase()
    {
        this.reactivityManager = new ReactivityManager(this);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        this.reactivityManager.OnAfterRender();
    }

    /// <inheritdoc/>
    protected virtual void ConfigureBindings()
    {
    }

    /// <inheritdoc cref="ReactivityManager.Observed{T}(Expression{Func{T}})"/>
    protected T Observed<T>(Expression<Func<T>> valueAccessor)
    {
        return this.reactivityManager.Observed(valueAccessor);
    }

    /// <inheritdoc cref="ReactivityManager.ObservedCollection{T}(Expression{Func{T}})"/>
    protected T ObservedCollection<T>(Expression<Func<T>> valueAccessor)
    {
        return this.reactivityManager.ObservedCollection(valueAccessor);
    }

    /// <inheritdoc cref="ReactivityManager.Binding{T}(Expression{Func{T}})" />
    protected IObservedBinding<T> Binding<T>(Expression<Func<T>> valueAccessor)
    {
        return this.reactivityManager.Binding(valueAccessor);
    }

    /// <inheritdoc cref="ReactivityManager.Binding{TSource, TTarget}(Expression{Func{TSource}}, Func{TSource, TTarget}, Func{TTarget, TSource})"/>
    protected IObservedBinding<TTarget> Binding<TSource, TTarget>(
        Expression<Func<TSource>> valueAccessor,
        Func<TSource, TTarget> converter,
        Func<TTarget, TSource> reverseConverter)
    {
        return this.reactivityManager.Binding(valueAccessor, converter, reverseConverter);
    }

    public virtual void Dispose()
    {
        this.reactivityManager.Dispose();

        GC.SuppressFinalize(this);
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