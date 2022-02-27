namespace Phork.Blazor.Lifecycle;

internal interface IMemberAccessorElement : ILifecycleElement
{
    bool IsAccessible { get; }
}