using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Phork.Blazor.Bindings;
using Phork.Expressions;

namespace Phork.Blazor;

public sealed class ReactivityManager : IDisposable
{
    private bool isDisposed = false;

    private readonly Dictionary<MemberAccessor, IReactivityEntry> entries = new();

    private readonly IReactiveComponent component;

    public ReactivityManager(IReactiveComponent component)
    {
        ArgumentNullException.ThrowIfNull(component);

        this.component = component;
    }

    /// <summary>
    /// Notifies the manager that a render cycle has been finished. The manager will subsequently
    /// get rid of inactive observed values and bindings.
    /// </summary>
    public void OnAfterRender()
    {
        this.EnsureNotDisposed();

        this.component.ConfigureBindings();

        List<MemberAccessor> inactiveEntries = new List<MemberAccessor>();
        foreach (var entry in this.entries)
        {
            if (entry.Value.TryDispose())
            {
                inactiveEntries.Add(entry.Key);
            }
        }

        foreach (var item in inactiveEntries)
        {
            this.entries.Remove(item);
        }
    }

    /// <summary>
    /// Observes the changes to the value represented by the <paramref name="valueAccessor"/>
    /// expression and returns its value.
    /// </summary>
    /// <typeparam name="T">Type of the value represented by <paramref name="valueAccessor"/>.</typeparam>
    /// <param name="valueAccessor">An expression representing the value.</param>
    /// <returns>The value represented by <paramref name="valueAccessor"/> expression.</returns>
    public T Observed<T>(Expression<Func<T>> valueAccessor)
    {
        ArgumentNullException.ThrowIfNull(valueAccessor);
        return this.Observed(MemberAccessor.Create(valueAccessor), false);
    }

    /// <summary>
    /// Observes the changes to the value and the collection represented by the <paramref
    /// name="valueAccessor"/> expression and returns its value.
    /// </summary>
    /// <typeparam name="T">Type of the value represented by <paramref name="valueAccessor"/>.</typeparam>
    /// <param name="valueAccessor">An expression representing the value.</param>
    /// <returns>The value represented by <paramref name="valueAccessor"/> expression.</returns>
    public T ObservedCollection<T>(Expression<Func<T>> valueAccessor)
    {
        ArgumentNullException.ThrowIfNull(valueAccessor);
        return this.Observed(MemberAccessor.Create(valueAccessor), true);
    }

    /// <summary>
    /// Observes the changes to the value represented by the <paramref name="valueAccessor"/>
    /// expression and returns an <see cref="IObservedBinding{T}"/> that getting or setting <see
    /// cref="IObservedBinding{T}.Value"/> will respectively get or set the value represented by the
    /// <paramref name="valueAccessor"/> expression.
    /// </summary>
    /// <typeparam name="T">Type of the value represented by <paramref name="valueAccessor"/>.</typeparam>
    /// <param name="valueAccessor">An expression representing the value.</param>
    /// <returns>An observed binding.</returns>
    public IObservedBinding<T> Binding<T>(Expression<Func<T>> valueAccessor)
    {
        ArgumentNullException.ThrowIfNull(valueAccessor);

        var descriptor = ObservedBindingDescriptor.Create<T>(ObservedBindingMode.TwoWay);

        return this.GetBinding(valueAccessor, descriptor);
    }

    /// <summary>
    /// Observes the changes to the value represented by the <paramref name="valueAccessor"/>
    /// expression and returns an <see cref="IObservedBinding{T}"/> that getting its <see
    /// cref="IObservedBinding{T}.Value"/> will return the value represented by the <paramref
    /// name="valueAccessor"/> converted to <typeparamref name="TTarget"/> by <paramref
    /// name="converter"/>, and setting it will use <paramref name="reverseConverter"/> to convert
    /// the set value to <typeparamref name="TSource"/> and set it to the value represented by the
    /// <paramref name="valueAccessor"/>.
    /// </summary>
    /// <typeparam name="TSource">Type of the source value.</typeparam>
    /// <typeparam name="TTarget">Type of the target value after conversion.</typeparam>
    /// <param name="valueAccessor">An expression representing the source value.</param>
    /// <param name="converter">
    /// A function to convert source values to target values.
    /// <para>
    /// Note: To improve the performance, avoid using lambda expressions as the converter, use
    /// instance or static methods instead.
    /// </para>
    /// </param>
    /// <param name="reverseConverter">
    /// A function to convert target values to source values.
    /// <para>
    /// Note: To improve the performance, avoid using lambda expressions as the converter, use
    /// instance or static methods instead.
    /// </para>
    /// </param>
    /// <returns>An observed binding.</returns>
    public IObservedBinding<TTarget> Binding<TSource, TTarget>(
        Expression<Func<TSource>> valueAccessor,
        Func<TSource, TTarget> converter,
        Func<TTarget, TSource> reverseConverter)
    {
        ArgumentNullException.ThrowIfNull(valueAccessor);
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(reverseConverter);

        var descriptor = ObservedBindingDescriptor.Create(converter, reverseConverter);

        return this.GetBinding(valueAccessor, descriptor);
    }

    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;

        foreach (var entry in this.entries.Values)
        {
            entry.Dispose();
        }
    }

    private ReactivityEntry<T> GetEntry<T>(MemberAccessor<T> valueAccessor)
    {
        ReactivityEntry<T> entry;

        if (this.entries.TryGetValue(valueAccessor, out var existing))
        {
            if (existing is not ReactivityEntry<T> typedExisting)
            {
                throw new InvalidOperationException("Unable to get entry. There is already an existing entry for the given accessor but has unmatching type.");
            }

            entry = typedExisting;
        }
        else
        {
            entry = new ReactivityEntry<T>(valueAccessor, this.StateHasChanged);
            this.entries[valueAccessor] = entry;
        }

        entry.Touch();

        return entry;
    }

    private IObservedBinding<TTarget> GetBinding<TSource, TTarget>(
        Expression<Func<TSource>> valueAccessor,
        IObservedBindingDescriptor<TSource, TTarget> bindingDescriptor)
    {
        this.EnsureNotDisposed();
        var memberAccessor = MemberAccessor.Create(valueAccessor);
        var entry = this.GetEntry(memberAccessor);
        return entry.GetBinding(bindingDescriptor);
    }

    private T Observed<T>(MemberAccessor<T> valueAccessor, bool observeCollectionChanges)
    {
        this.EnsureNotDisposed();

        var observedValue = this.GetEntry(valueAccessor).ObservedValue;

        if (observeCollectionChanges)
        {
            observedValue.RequestCollectionObserving();
        }

        return observedValue.Value;
    }

    private void StateHasChanged()
    {
        this.component.StateHasChanged();
    }

    private void EnsureNotDisposed()
    {
        if (this.isDisposed)
        {
            throw new ObjectDisposedException(nameof(ReactivityManager));
        }
    }
}