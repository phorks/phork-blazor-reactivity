using System;
using System.Linq.Expressions;
using Phork.Blazor.Bindings;

namespace Phork.Blazor;

public interface IReactivityManager : IDisposable
{
    /// <summary>
    /// Initializes the reactivity manager by passing its owning component.
    /// </summary>
    /// <param name="component">The owning component.</param>
    /// <exception cref="InvalidOperationException">Thrown when the reactivity manager is already initialized.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the reactivity manager is disposed.</exception>
    void Initialize(IReactiveComponent component);

    /// <summary>
    /// Observes the changes to the value represented by the <paramref name="valueAccessor"/>
    /// expression and returns its value.
    /// </summary>
    /// <typeparam name="T">Type of the value represented by <paramref name="valueAccessor"/>.</typeparam>
    /// <param name="valueAccessor">An expression representing the value.</param>
    /// <returns>The value represented by <paramref name="valueAccessor"/> expression.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the reactivity manager is not initialized.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the reactivity manager is disposed.</exception>
    T Observed<T>(Expression<Func<T>> valueAccessor);

    /// <summary>
    /// Observes the changes to the value and the collection represented by the <paramref
    /// name="valueAccessor"/> expression and returns its value.
    /// </summary>
    /// <typeparam name="T">Type of the value represented by <paramref name="valueAccessor"/>.</typeparam>
    /// <param name="valueAccessor">An expression representing the value.</param>
    /// <returns>The value represented by <paramref name="valueAccessor"/> expression.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the reactivity manager is not initialized.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the reactivity manager is disposed.</exception>
    T ObservedCollection<T>(Expression<Func<T>> valueAccessor);

    /// <summary>
    /// Observes the changes to the value represented by the <paramref name="valueAccessor"/>
    /// expression and returns an <see cref="IObservedBinding{T}"/> that getting or setting its <see
    /// cref="IObservedBinding{T}.Value"/> will respectively get or set the value represented by the
    /// <paramref name="valueAccessor"/> expression.
    /// </summary>
    /// <typeparam name="T">Type of the value represented by <paramref name="valueAccessor"/>.</typeparam>
    /// <param name="valueAccessor">An expression representing the value.</param>
    /// <returns>An observed binding.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the reactivity manager is not initialized.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the reactivity manager is disposed.</exception>
    IObservedBinding<T> Binding<T>(Expression<Func<T>> valueAccessor);

    /// <summary>
    /// Observes the changes to the value represented by the <paramref name="valueAccessor"/>
    /// expression and returns an <see cref="IObservedBinding{T}"/> that getting its <see
    /// cref="IObservedBinding{T}.Value"/> will return the value represented by the <paramref
    /// name="valueAccessor"/> converted to <typeparamref name="TTarget"/> by the <paramref
    /// name="converter"/>, and setting it will use the <paramref name="reverseConverter"/> to
    /// convert the set value to <typeparamref name="TSource"/> and set it to the value represented
    /// by the <paramref name="valueAccessor"/>.
    /// </summary>
    /// <typeparam name="TSource">Type of the source value.</typeparam>
    /// <typeparam name="TTarget">Type of the target value after conversion.</typeparam>
    /// <param name="valueAccessor">An expression representing the source value.</param>
    /// <param name="converter">A function to convert source values to target values.</param>
    /// <param name="reverseConverter">A function to convert target values to source values.</param>
    /// <returns>An observed binding.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the reactivity manager is not initialized.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the reactivity manager is disposed.</exception>
    IObservedBinding<TTarget> Binding<TSource, TTarget>(
        Expression<Func<TSource>> valueAccessor,
        Func<TSource, TTarget> converter,
        Func<TTarget, TSource> reverseConverter);

    /// <summary>
    /// Notifies the manager that a render cycle has been finished. The helps the manager get rid of
    /// inactive elements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the reactivity manager is not initialized.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the reactivity manager is disposed.</exception>
    void OnAfterRender();
}