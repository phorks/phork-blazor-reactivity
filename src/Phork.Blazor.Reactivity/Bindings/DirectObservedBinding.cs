using System;
using Phork.Blazor.Expressions;

namespace Phork.Blazor.Bindings;

internal class DirectObservedBinding<T> : IObservedBinding<T>
{
    private readonly MemberAccessor<T> accessor;

    /// <inheritdoc/>
    public T Value
    {
        get => this.accessor.Value;
        set => this.accessor.Value = value;
    }

    public DirectObservedBinding(MemberAccessor<T> accessor)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        this.accessor = accessor;
    }
}