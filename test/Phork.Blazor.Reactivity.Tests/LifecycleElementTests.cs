using System;
using Phork.Blazor.Lifecycle;
using Xunit;

namespace Phork.Blazor.Reactivity.Tests;

public abstract class LifecycleElementTests : IDisposable
{
    private protected abstract ILifecycleElement Element { get; }

    [Fact]
    public void Touch_ShouldMakeElementActive()
    {
        this.Element.Touch();

        Assert.True(this.Element.IsActive);
    }

    [Fact]
    public void Touch_ShouldThrowObjectDisposedException_WhenDisposed()
    {
        this.Element.Dispose();

        Assert.Throws<ObjectDisposedException>(() => this.Element.Touch());
    }

    [Fact]
    public void NotifyCycleEneded_ShouldThrowObjectDisposedException_WhenDisposed()
    {
        this.Element.Dispose();

        Assert.Throws<ObjectDisposedException>(() => this.Element.Touch());
    }

    [Fact]
    public void NotifyCycleEneded_ShouldDisposeElement_WhenNotTouched()
    {
        this.Element.NotifyCycleEnded();

        Assert.True(this.Element.IsDisposed);
    }

    [Fact]
    public void Dispose_ShouldMakeElementDisposed()
    {
        this.Element.Dispose();

        Assert.True(this.Element.IsDisposed);
    }

    public virtual void Dispose()
    {
        this.Element.Dispose();
    }
}