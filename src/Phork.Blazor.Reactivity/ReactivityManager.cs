using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Phork.Blazor.Bindings;
using Phork.Blazor.Expressions;
using Phork.Blazor.Helpers;
using Phork.Blazor.Lifecycle;
using Phork.Blazor.Services;

namespace Phork.Blazor;

internal sealed class ReactivityManager : IReactivityManager
{
    private bool isDisposed = false;

    private IReactiveComponent? component;

    private readonly IPropertyObserver propertyObserver;
    private readonly ICollectionObserver collectionObserver;

    private readonly Dictionary<MemberAccessor, IMemberAccessorElement> memberAccessors = new();

    public ReactivityManager(
        IPropertyObserver propertyObserver,
        ICollectionObserver collectionObserver)
    {
        ArgumentNullException.ThrowIfNull(propertyObserver);
        ArgumentNullException.ThrowIfNull(collectionObserver);

        this.propertyObserver = propertyObserver;
        this.collectionObserver = collectionObserver;

        propertyObserver.ObservedPropertyChanged += this.PropertyObserver_ObservedPropertyChanged;
        collectionObserver.ObservedCollectionChanged += this.CollectionObserver_ObservedCollectionChanged;
    }

    /// <inheritdoc/>
    public void Initialize<TComponent>(TComponent component)
        where TComponent : ComponentBase, IReactiveComponent
    {
        ArgumentNullException.ThrowIfNull(component);

        this.AssertNotDisposed();

        if (this.component is not null)
        {
            throw new InvalidOperationException("Reactivity manager is already initialized.");
        }

        this.ConfigureComponentBase(component);

        this.component = component;
    }

    /// <inheritdoc/>
    public void NotifyCycleEnded()
    {
        this.AssertNotDisposed();
        this.AssertInitialized();

        this.component.ConfigureBindings();

        this.memberAccessors.NotifyCycleEndedAndRemoveDisposedElements();

        this.propertyObserver.OnAfterRender();
        this.collectionObserver.OnAfterRender();
    }

    /// <inheritdoc/>
    [Obsolete]
    public void OnAfterRender()
    {
    }

    /// <inheritdoc/>
    public T Observed<T>(Expression<Func<T>> valueAccessor)
    {
        ArgumentNullException.ThrowIfNull(valueAccessor);

        var element = this.Observe(valueAccessor);

        return element.Accessor.Value;
    }

    /// <inheritdoc/>
    public T ObservedCollection<T>(Expression<Func<T>> valueAccessor)
    {
        ArgumentNullException.ThrowIfNull(valueAccessor);

        var value = this.Observed(valueAccessor);

        if (value is INotifyCollectionChanged notifier)
        {
            this.collectionObserver.Observe(notifier);
        }

        return value;
    }

    /// <inheritdoc/>
    public IObservedBinding<T> Binding<T>(Expression<Func<T>> valueAccessor)
    {
        ArgumentNullException.ThrowIfNull(valueAccessor);

        var element = this.Observe(valueAccessor);

        var binding = new DirectObservedBinding<T>(element.Accessor);

        return binding;
    }

    /// <inheritdoc/>
    public IObservedBinding<TTarget> Binding<TSource, TTarget>(
        Expression<Func<TSource>> valueAccessor,
        Func<TSource, TTarget> converter,
        Func<TTarget, TSource> reverseConverter)
    {
        ArgumentNullException.ThrowIfNull(valueAccessor);
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(reverseConverter);

        var element = this.Observe(valueAccessor);

        var binding = new ConvertedObservedBinding<TSource, TTarget>(element.Accessor, converter, reverseConverter);

        return binding;
    }

    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        foreach (var item in this.memberAccessors.Values)
        {
            item.Dispose();
        }

        this.propertyObserver.Dispose();
        this.collectionObserver.Dispose();

        this.isDisposed = true;
    }

    private MemberAccessorElement<T> Observe<T>(Expression<Func<T>> valueAccessor, bool ensureAccessible = true)
    {
        this.AssertNotDisposed();
        this.AssertInitialized();

        var accessor = MemberAccessor.Create(valueAccessor);

        MemberAccessorElement<T> element;
        if (this.memberAccessors.TryGetValue(accessor, out var existing))
        {
            if (existing is not MemberAccessorElement<T> typedElement)
            {
                throw new InvalidOperationException("A single accessor cannot be used with two different generic type parameters.");
            }

            element = typedElement;
        }
        else
        {
            element = new MemberAccessorElement<T>(accessor, this.propertyObserver);
            this.memberAccessors[accessor] = element;
        }

        element.Touch();

        if (ensureAccessible && !element.IsAccessible)
        {
            if (!element.IsAccessible)
            {
                throw new ArgumentException("Expression path is not resolvable due to some part of it being null.", nameof(valueAccessor));
            }
        }

        return element;
    }

    private void StateHasChanged()
    {
        this.AssertInitialized();

        this.component.StateHasChanged();
    }

    private void PropertyObserver_ObservedPropertyChanged(object? sender, EventArgs e)
    {
        this.StateHasChanged();
    }

    private void CollectionObserver_ObservedCollectionChanged(object? sender, EventArgs e)
    {
        this.StateHasChanged();
    }

    private void AssertNotDisposed()
    {
        if (this.isDisposed)
        {
            throw new ObjectDisposedException(nameof(ReactivityManager));
        }
    }

    [MemberNotNull(nameof(component))]
    private void AssertInitialized()
    {
        if (this.component is null)
        {
            throw new InvalidOperationException("Reactivity manager is not initialized.");
        }
    }

    private void ConfigureComponentBase(ComponentBase componentBase)
    {
        var oldRenderFragment = ComponentBaseHelper.GetRenderFragment(componentBase);

        ComponentBaseHelper.SetRenderFragment(componentBase, NewRenderFragment);

        void NewRenderFragment(RenderTreeBuilder builder)
        {
            oldRenderFragment(builder);
            this.NotifyCycleEnded();
        }
    }
}