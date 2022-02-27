using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Phork.Blazor.Expressions;

internal abstract class MemberAccessor : IEquatable<MemberAccessor>
{
    internal LambdaExpression LambdaExpression { get; }

    public ImmutableArray<MemberExpression>? MemberExpressions { get; private protected set; }

    public MemberAccessorType Type { get; protected set; }
    public object? Target { get; protected set; }
    public Type? TargetType { get; protected set; }
    public bool IsReadOnly { get; protected set; }

    internal MemberAccessor(LambdaExpression lambdaExpression)
    {
        ArgumentNullException.ThrowIfNull(lambdaExpression);

        this.LambdaExpression = lambdaExpression;
    }

    public virtual bool Equals(MemberAccessor? other)
    {
        return ReferenceEquals(this.Target, other?.Target);
    }

    public override bool Equals(object? obj)
    {
        return obj is MemberAccessor accessor && this.Equals(accessor);
    }

    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this.Target);
    }

    public static MemberAccessor<T> Create<T>(Expression<Func<T>> accessor)
    {
        return new MemberAccessor<T>(accessor, true);
    }

    public static bool operator ==(MemberAccessor? lhs, MemberAccessor? rhs)
    {
        if (lhs is null)
        {
            return rhs is null;
        }

        return lhs.Equals(rhs);
    }

    public static bool operator !=(MemberAccessor? lhs, MemberAccessor? rhs)
        => !(lhs == rhs);
}

internal class MemberAccessor<T> : MemberAccessor
{
    private readonly Lazy<Func<T>> valueGetter;
    private readonly Lazy<Action<T>> valueSetter;

    public Expression<Func<T>> Expression { get; }

    public T Value
    {
        get => this.valueGetter.Value.Invoke();
        set => this.valueSetter.Value.Invoke(value);
    }

    internal MemberAccessor(Expression<Func<T>> accessor, bool reduceCompilerGeneratedParts = true)
        : base(accessor)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        this.valueGetter = new Lazy<Func<T>>(() =>
        {
            if (this.Expression is null)
            {
                throw new InvalidOperationException($"Unable to create '{nameof(this.valueGetter)}'. '{nameof(this.Expression)}' is not assigned.");
            }

            return this.Expression.Compile();
        });

        this.valueSetter = new Lazy<Action<T>>(() =>
        {
            if (this.IsReadOnly)
            {
                throw new InvalidOperationException($"Unable to set the value of '{this.Expression}'. The target is read-only.");
            }

            if (this.Expression is null)
            {
                throw new InvalidOperationException($"Unable to create '{nameof(this.valueGetter)}'. '{nameof(this.Expression)}' is not assigned.");
            }

            var valueParameter = System.Linq.Expressions.Expression.Parameter(typeof(T));
            return System.Linq.Expressions.Expression
                        .Lambda<Action<T?>>(
                            System.Linq.Expressions.Expression.Assign(this.Expression.Body, valueParameter),
                            valueParameter)
                        .Compile();
        });

        if (accessor.Body is MemberExpression memberBody)
        {
            var members = MemberExpressionHelper.GetOrderedMembers(memberBody);

            if (members.Length == 0)
            {
                throw new ArgumentException($"Unable to create {typeof(MemberAccessor).Name}. Given argument is not a valid accessor expression.", nameof(accessor));
            }

            if (members[0].Expression is not ConstantExpression constant)
            {
                throw new ArgumentException($"Unable to create {typeof(MemberAccessor).Name}. An accessor expression must have a constant target.", nameof(accessor));
            }

            this.IsReadOnly = !ExpressionHelper.IsWritable(members.Last());
            this.Type = MemberAccessorType.Member;
            this.Expression = accessor;

            int i;
            for (i = 0; i < members.Length; i++)
            {
                if (ShouldReduce(constant, reduceCompilerGeneratedParts))
                {
                    constant = System.Linq.Expressions.Expression.Constant(ExpressionHelper.Evaluate(members[i]));
                }
                else
                {
                    break;
                }
            }

            // The expression is reducible
            if (constant != members[0].Expression)
            {
                var reducedMembers = new List<MemberExpression>();

                Expression temp = constant;

                for (; i < members.Length; i++)
                {
                    var member = System.Linq.Expressions.Expression.MakeMemberAccess(temp, members[i].Member);
                    reducedMembers.Add(member);
                    temp = member;
                }

                if (temp is ConstantExpression)
                {
                    this.Type = MemberAccessorType.Constant;
                    this.IsReadOnly = true;
                }
                else
                {
                    this.MemberExpressions = reducedMembers.ToImmutableArray();
                }

                this.Expression = System.Linq.Expressions.Expression.Lambda<Func<T>>(temp);
            }
            else
            {
                this.MemberExpressions = members;
            }

            this.Target = constant.Value;
            this.TargetType = constant.Type;
        }
        else if (accessor.Body is ConstantExpression constantBody)
        {
            this.Type = MemberAccessorType.Constant;
            this.IsReadOnly = true;
            this.Target = constantBody.Value;
            this.TargetType = constantBody.Type;
            this.Expression = accessor;
        }
        else
        {
            throw new ArgumentException("Unable to create member-accessor. Given accessor is not a valid member-accessor expression.");
        }
    }

    public override bool Equals(MemberAccessor? other)
    {
        return base.Equals(other)
            && other is MemberAccessor<T> typedOther
            && this.Expression.ToString() == typedOther.Expression.ToString();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), this.Expression.ToString());
    }

    private static bool ShouldReduce(
        ConstantExpression constant,
        bool reduceCompilerGeneratedParts)
    {
        if (reduceCompilerGeneratedParts && constant.Type.IsDefined(typeof(CompilerGeneratedAttribute)))
        {
            return true;
        }

        return false;
    }
}