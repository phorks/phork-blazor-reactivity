using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Components;

namespace Phork.Blazor.Helpers;

internal static class ComponentBaseHelper
{
    private const string RenderFragmentFieldName = "_renderFragment";

    private static Func<ComponentBase, RenderFragment?> RenderFragmentGetter { get; }
    private static Action<ComponentBase, RenderFragment> RenderFragmentSetter { get; }

    static ComponentBaseHelper()
    {
        const string renderFragmentFieldName = "_renderFragment";

        var renderFragmentField = typeof(ComponentBase)
            .GetField(
                renderFragmentFieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);

        if (renderFragmentField?.FieldType != typeof(RenderFragment))
        {
            ThrowImplementationChanged();
        }

        RenderFragmentGetter = CreateRenderFragmentGetter(renderFragmentField);
        RenderFragmentSetter = CreateRenderFragmentSetter(renderFragmentField);
    }

    public static RenderFragment GetRenderFragment(ComponentBase componentBase)
    {
        ArgumentNullException.ThrowIfNull(componentBase);

        var renderFragment = RenderFragmentGetter(componentBase);

        if (renderFragment is null)
        {
            ThrowImplementationChanged();
        }

        return renderFragment;
    }

    public static void SetRenderFragment(ComponentBase componentBase, RenderFragment renderFragment)
    {
        ArgumentNullException.ThrowIfNull(componentBase);
        ArgumentNullException.ThrowIfNull(renderFragment);

        RenderFragmentSetter(componentBase, renderFragment);
    }

    [DoesNotReturn]
    private static void ThrowImplementationChanged()
    {
        throw new InvalidOperationException($"Unable to get '{RenderFragmentFieldName}' value. This is probably caused by some changes in the implementation of '{nameof(ComponentBase)}'.");
    }

    private static Func<ComponentBase, RenderFragment?> CreateRenderFragmentGetter(FieldInfo field)
    {
        var componentParameter = Expression.Parameter(typeof(ComponentBase));

        var fieldExpression = Expression.Field(componentParameter, field);


        var expression = Expression.Lambda<Func<ComponentBase, RenderFragment?>>(
            fieldExpression,
            componentParameter);

        return expression.Compile();
    }

    private static Action<ComponentBase, RenderFragment> CreateRenderFragmentSetter(FieldInfo field)
    {
        DynamicMethod dynamicMethod = new DynamicMethod(
            "set_" + field.Name,
            typeof(void),
            new[] { typeof(ComponentBase), typeof(RenderFragment) },
            typeof(ComponentBaseHelper).Module,
            true);

        var il = dynamicMethod.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, field);
        il.Emit(OpCodes.Ret);

        return dynamicMethod.CreateDelegate<Action<ComponentBase, RenderFragment>>();
    }
}