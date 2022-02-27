using Phork.Blazor.Reactivity.Tests.Models;

namespace Phork.Blazor.Reactivity.Tests;

internal static class Values
{
    public const string DefaultValue = "default";
    public const string NewValue = "new";
    public const string IrrelevantValue = "irrelevant";
    public const string RelevantValue = "relevant";

    public static RootBindable CreateRootBindable()
    {
        return new RootBindable
        {
            Inner = CreateInnerBindable()
        };
    }

    public static InnerBindable CreateInnerBindable()
    {
        return new InnerBindable
        {
            StringValue = DefaultValue,
            NumberValue = 0,
            Collection = new()
        };
    }
}