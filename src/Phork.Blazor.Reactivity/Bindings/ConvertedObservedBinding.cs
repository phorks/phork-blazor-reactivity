using System;
using Phork.Blazor.Expressions;

namespace Phork.Blazor.Bindings;

internal class ConvertedObservedBinding<TSource, TTarget> : IObservedBinding<TTarget>
{
    private readonly MemberAccessor<TSource> accessor;
    private readonly Func<TSource, TTarget> converter;
    private readonly Func<TTarget, TSource> reverseConverter;

    /// <inheritdoc/>
    public TTarget Value
    {
        get
        {
            return this.converter(this.accessor.Value);
        }
        set
        {
            this.accessor.Value = this.reverseConverter(value);
        }
    }

    public ConvertedObservedBinding(
        MemberAccessor<TSource> accessor,
        Func<TSource, TTarget> converter,
        Func<TTarget, TSource> reverseConverter)
    {
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(reverseConverter);

        this.accessor = accessor;
        this.converter = converter;
        this.reverseConverter = reverseConverter;
    }
}