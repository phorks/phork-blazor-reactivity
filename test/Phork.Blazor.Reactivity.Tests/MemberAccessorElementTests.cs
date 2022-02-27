using System;
using System.Linq.Expressions;
using Moq;
using Phork.Blazor.Expressions;
using Phork.Blazor.Lifecycle;
using Phork.Blazor.Reactivity.Tests.Models;
using Phork.Blazor.Services;
using Xunit;

namespace Phork.Blazor.Reactivity.Tests;

public class MemberAccessorElementTests
{
    private readonly Mock<IPropertyObserver> propertyChangedManager = new();
    private readonly RootBindable root;

    public MemberAccessorElementTests()
    {
        this.root = Values.CreateRootBindable();
    }

    [Fact]
    public void Touch_ShouldObserveProperties_WithMemberRoot()
    {
        var sut = this.CreateElement(() => this.root.Inner.StringValue);

        sut.Touch();

        this.propertyChangedManager.Verify(x => x.Observe(this.root, nameof(RootBindable.Inner)), Times.Once);

        this.propertyChangedManager.Verify(x => x.Observe(this.root.Inner, nameof(InnerBindable.StringValue)), Times.Once);

        this.propertyChangedManager.VerifyNoOtherCalls();
    }

    [Fact]
    public void Touch_ShouldObserveProperties_WithConstantRoot()
    {
        var root = Values.CreateRootBindable();

        var sut = this.CreateElement(() => root.Inner.StringValue);

        sut.Touch();

        this.propertyChangedManager.Verify(x => x.Observe(root, nameof(RootBindable.Inner)), Times.Once);
        this.propertyChangedManager.Verify(x => x.Observe(root.Inner, nameof(InnerBindable.StringValue)), Times.Once);
    }

    [Fact]
    public void Touch_ShouldUpdateObservedProperties_WithConstantRoot()
    {
        var sut = this.CreateElement(() => this.root.Inner.StringValue);

        sut.Touch();

        this.propertyChangedManager.Invocations.Clear();

        sut.NotifyCycleEnded();

        this.root.Inner = Values.CreateInnerBindable();

        sut.Touch();

        this.propertyChangedManager.Verify(x => x.Observe(this.root, nameof(RootBindable.Inner)), Times.Once);
        this.propertyChangedManager.Verify(x => x.Observe(this.root.Inner, nameof(InnerBindable.StringValue)), Times.Once);
        this.propertyChangedManager.VerifyNoOtherCalls();
    }

    [Fact]
    public void Touch_ShouldCallObserve_ForEachCallDuringCycle()
    {
        var sut = this.CreateElement(() => this.root.Inner.StringValue);

        sut.Touch();

        sut.Touch();

        this.propertyChangedManager.Verify(x => x.Observe(this.root, nameof(RootBindable.Inner)), Times.Exactly(2));
        this.propertyChangedManager.Verify(x => x.Observe(this.root.Inner, nameof(InnerBindable.StringValue)), Times.Exactly(2));
        this.propertyChangedManager.VerifyNoOtherCalls();
    }

    [Fact]
    public void IsAccessible_ShouldBeTrue_WhenNoPartIsNull()
    {
        var sut = this.CreateElement(() => this.root.Inner.StringValue);
        sut.Touch();

        Assert.True(sut.IsAccessible);
    }

    [Fact]
    public void IsAccessible_ShouldBeFalse_WhenSomePartIsNull()
    {
        this.root.Inner = null;

        var sut = this.CreateElement(() => this.root.Inner.StringValue);
        sut.Touch();

        Assert.False(sut.IsAccessible);
    }

    [Fact]
    public void IsAccessible_ShouldBeTrue_WhenExpressionIsConstant()
    {
        int value = 0;

        var sut = this.CreateElement(() => value);
        sut.Touch();

        Assert.True(sut.IsAccessible);
    }

    private MemberAccessorElement<T> CreateElement<T>(Expression<Func<T>> accessor)
    {
        return new MemberAccessorElement<T>(
            MemberAccessor.Create(accessor),
            this.propertyChangedManager.Object);
    }
}