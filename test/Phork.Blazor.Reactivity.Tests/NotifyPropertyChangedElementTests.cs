using System;
using System.ComponentModel;
using Moq;
using Phork.Blazor.Lifecycle;
using Phork.Blazor.Reactivity.Tests.Models;
using Xunit;

namespace Phork.Blazor.Reactivity.Tests;

public class NotifyPropertyChangedElementTests : LifecycleElementTests
{
    private readonly NotifyPropertyChangedElement sut;
    private readonly InnerBindable bindable;

    private protected override LifecycleElement Element => this.sut;

    private readonly Mock<Action<INotifyPropertyChanged, string?>> callback;

    public NotifyPropertyChangedElementTests()
    {
        this.bindable = Values.CreateInnerBindable();

        this.callback = new();

        this.sut = new(this.bindable, this.callback.Object);
    }

    [Fact]
    public void NotifyCycleEnded_ShouldNotDisposeElement_WhenHasActiveProperties()
    {
        this.sut.Observe(nameof(InnerBindable.StringValue));

        this.sut.NotifyCycleEnded();

        Assert.False(this.sut.IsDisposed);
    }

    [Fact]
    public void NotifyCycleEneded_ShouldDisposeElement_WhenHasNoActiveProperties()
    {
        this.sut.Observe(nameof(InnerBindable.StringValue));

        this.sut.NotifyCycleEnded();

        this.sut.NotifyCycleEnded();

        Assert.True(this.sut.IsDisposed);
    }

    [Fact]
    public void NotifyCycleEneded_ShouldDisposeElement_WhenTouchedWithNoActiveProperties()
    {
        this.sut.Observe(nameof(InnerBindable.StringValue));

        this.sut.NotifyCycleEnded();

        this.sut.Touch();

        this.sut.NotifyCycleEnded();

        Assert.True(this.sut.IsDisposed);
    }

    [Fact]
    public void Observe_ShouldTouchElement()
    {
        this.sut.Observe(nameof(InnerBindable.StringValue));

        Assert.True(this.sut.IsActive);
    }

    [Fact]
    public void Observe_ShouldThrowArgumentNullException_WhenPropertyIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => this.sut.Observe(null!));
    }

    [Fact]
    public void Observe_ShouldThrowObjectDisposedException_WhenDisposed()
    {
        this.sut.Observe(nameof(InnerBindable.StringValue));

        this.sut.Dispose();

        Assert.Throws<ObjectDisposedException>(() => this.sut.Observe(nameof(InnerBindable.StringValue)));
    }

    [Fact]
    public void NotifyCycleEnded_ShouldThrowObjectDisposedException_WhenDisposed()
    {
        this.sut.NotifyCycleEnded();

        Assert.Throws<ObjectDisposedException>(() => this.sut.Observe(nameof(InnerBindable.StringValue)));
    }

    [Fact]
    public void PropertyChangedEvent_ShouldCallCallback_DuringCycle()
    {
        this.sut.Observe(nameof(InnerBindable.StringValue));

        this.bindable.StringValue = Values.NewValue;

        this.callback.Verify(x => x(this.bindable, nameof(InnerBindable.StringValue)), Times.Once);
    }

    [Fact]
    public void PropertyChangedEvent_ShouldCallCallback_AfterActiveCycleForObservedProperty()
    {
        this.sut.Observe(nameof(InnerBindable.StringValue));

        this.sut.NotifyCycleEnded();

        this.bindable.StringValue = Values.NewValue;

        this.callback.Verify(x => x(this.bindable, nameof(InnerBindable.StringValue)), Times.Once);
    }

    [Fact]
    public void PropertyChangedEvent_ShouldNotCallCallback_AfterInactiveCycleForObservedProperty()
    {
        this.sut.Observe(nameof(InnerBindable.StringValue));

        // Observe another property to stop the element from being disposed of after the second cycle
        this.sut.Observe(nameof(InnerBindable.NumberValue));

        this.sut.NotifyCycleEnded();

        // This ensures the element won't be disposed of due to the lack of active properties
        this.sut.Observe(nameof(InnerBindable.NumberValue));

        this.sut.NotifyCycleEnded();

        this.bindable.StringValue = Values.NewValue;

        this.callback.Verify(x => x(It.IsAny<INotifyPropertyChanged>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void PropertyChangedEvent_ShouldNotCallCallback_WhenRaisedForUnobservedProperty()
    {
        this.sut.Observe(nameof(InnerBindable.NumberValue));

        this.bindable.StringValue = Values.IrrelevantValue;

        this.callback.Verify(x => x(It.IsAny<INotifyPropertyChanged>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void PropertyChangedEvent_ShouldNotCallCallback_WhenDisposed()
    {
        this.sut.Observe(nameof(InnerBindable.StringValue));

        this.sut.Dispose();

        this.bindable.StringValue = Values.NewValue;

        this.callback.Verify(x => x(It.IsAny<INotifyPropertyChanged>(), It.IsAny<string>()), Times.Never);
    }
}