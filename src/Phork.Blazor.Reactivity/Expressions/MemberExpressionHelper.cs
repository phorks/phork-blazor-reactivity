using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Phork.Blazor.Expressions;

internal static class MemberExpressionHelper
{
    public static MemberExpression GetRoot(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var member = expression.Body as MemberExpression;

        while (member != null && member.Expression is MemberExpression parent)
        {
            member = parent;
        }

        if (member is null)
        {
            throw new ArgumentException("Unable to get the root of the expression. Given expression is not a valid member chain expression.", nameof(expression));
        }

        return member;
    }

    public static Type GetRootType(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var root = GetRoot(expression);

        if (root?.Expression is not ConstantExpression constant)
        {
            throw new ArgumentException("Unable to get the root type of the expression. Given expression is not a valid member chain expression.", nameof(expression));
        }

        return constant.Type;
    }

    public static object? GetRootObject(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var root = GetRoot(expression);

        if (root?.Expression is not ConstantExpression constant)
        {
            throw new ArgumentException("Unable to get the root object of the expression. Given expression is not a valid member chain expression.", nameof(expression));
        }

        return constant.Value;
    }

    /// <summary>
    /// If the expression is a chain of <see cref="MemberExpression"/>s, or if the expression
    /// is a <see cref="LambdaExpression"/> and its body is a chain of <see
    /// cref="MemberExpression"/>s it will return an array of <see cref="MemberExpression"/>s
    /// starting from the left-most one. Otherwise; <see langword="null"/> will be returned.
    /// </summary>
    /// <param name="expression">Accessor expression</param>
    public static ImmutableArray<MemberExpression> GetOrderedMembers(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        expression = (expression as LambdaExpression)?.Body ?? expression;

        var expressions = new Stack<MemberExpression>();

        var iterator = expression as MemberExpression;

        while (iterator != null)
        {
            expressions.Push(iterator);
            iterator = iterator.Expression as MemberExpression;
        }

        var result = expressions.ToImmutableArray();

        if (result[0].Expression is not ConstantExpression)
        {
            throw new ArgumentException("Expression is not a valid member access chain expression.", nameof(expression));
        }

        return result;
    }

    public static Expression<Func<T>> ReduceRootToConstant<T>(Expression<Func<T>> expression, out object? root)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var expressions = GetOrderedMembers(expression);

        if (expressions.Length <= 1)
        {
            throw new ArgumentException("Unable to reduce the root member of the expression. Given expression is not a valid member chain expression with at least two parts.", nameof(expression));
        }

        root = ExpressionHelper.Evaluate(expressions[0]);

        Expression newExpression = Expression.Constant(root, expressions[0].Type);

        for (int i = 1; i < expressions.Length; i++)
        {
            newExpression = Expression.MakeMemberAccess(newExpression, expressions[i].Member);
        }

        return Expression.Lambda<Func<T>>(newExpression);
    }
}