using Phork.Blazor.Expressions;
using Phork.Blazor.Reactivity.Tests.Models;
using Xunit;

namespace Phork.Blazor.Reactivity.Tests;

public class MemberAccessorTests
{
    private RootBindable Bindable { get; set; }

    public MemberAccessorTests()
    {
        this.Bindable = Values.CreateRootBindable();
    }

    [Fact]
    public void Target_ShouldBeRootObject_WhenRootIsObject()
    {
        var accessor = MemberAccessor.Create(() => this.Bindable.Inner.StringValue);

        Assert.Same(this, accessor.Target);
    }

    [Fact]
    public void Target_ShouldBeScopedVariable_WhenRootIsScopedVariable()
    {
        var bindable = Values.CreateRootBindable();

        var accessor = MemberAccessor.Create(() => bindable.Inner.StringValue);

        Assert.Same(bindable, accessor.Target);
    }
}