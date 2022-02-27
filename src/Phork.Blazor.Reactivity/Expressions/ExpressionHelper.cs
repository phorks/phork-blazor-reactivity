using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Phork.Blazor.Expressions;

internal static class ExpressionHelper
{
    public static object? Evaluate(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var lambda = expression as LambdaExpression ?? Expression.Lambda(expression);

        return lambda.Compile().DynamicInvoke();
    }

    public static bool IsReadable(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        bool isReadable = false;

        if (expression is IndexExpression index)
        {
            isReadable = index.Indexer == null || index.Indexer.CanRead;
        }
        else if (expression is MemberExpression member)
        {
            if (member.Member is PropertyInfo property)
            {
                isReadable = property.CanRead;
            }
            else if (member.Member is FieldInfo)
            {
                isReadable = true;
            }
        }

        return isReadable;
    }

    public static bool IsWritable(Expression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        bool isWritable = false;

        if (expression is IndexExpression index)
        {
            isWritable = index.Indexer == null || index.Indexer.CanWrite;
        }
        else if (expression is MemberExpression member)
        {
            switch (member.Member.MemberType)
            {
                case MemberTypes.Property:
                    var property = member.Member as PropertyInfo;
                    isWritable = property!.CanWrite;
                    break;

                case MemberTypes.Field:
                    var field = member.Member as FieldInfo;
                    isWritable = !field!.IsInitOnly && !field!.IsLiteral;
                    break;
            }
        }
        else if (expression is ParameterExpression)
        {
            isWritable = true;
        }

        return isWritable;
    }
}