using System;
using Moq;
using Phork.Blazor.Reactivity.Tests.Models;
using Phork.Blazor.Services;
using Xunit;

namespace Phork.Blazor.Reactivity.Tests;

public class PropertyObserverTests : IDisposable
{
    private readonly PropertyObserver sut;

    private readonly Mock<EventHandler> callback = new();
    private readonly InnerBindable bindable;

    public PropertyObserverTests()
    {
        this.sut = new PropertyObserver();

        this.sut.ObservedPropertyChanged += this.callback.Object;

        this.bindable = Values.CreateInnerBindable();
    }

    [Fact]
    public void PropertyChangedEvent_ShouldCallCallback_ForObservedPropertyDuringCycle()
    {
        this.sut.Observe(this.bindable, nameof(InnerBindable.StringValue));

        this.bindable.StringValue = Values.NewValue;

        this.callback.Verify(x => x(this.sut, EventArgs.Empty), Times.Once);
    }

    [Fact]
    public void PropertyChangedEvent_ShouldCallCallback_ForEachObservedPropertyDuringCycle()
    {
        this.sut.Observe(this.bindable, nameof(InnerBindable.StringValue));
        this.sut.Observe(this.bindable, nameof(InnerBindable.NumberValue));

        this.bindable.StringValue = Values.NewValue;
        this.bindable.NumberValue = 1;

        this.callback.Verify(x => x(this.sut, EventArgs.Empty), Times.Exactly(2));
    }

    [Fact]
    public void PropertyChangedEvent_ShouldCallCallback_ForObservedPropertyAfterActiveCycle()
    {
        this.sut.Observe(this.bindable, nameof(InnerBindable.StringValue));

        this.sut.OnAfterRender();

        this.bindable.StringValue = Values.NewValue;

        this.callback.Verify(x => x(this.sut, EventArgs.Empty), Times.Once);
    }

    [Fact]
    public void PropertyChangedEvent_ShouldNotCallCallback_ForObservedPropertyAfterInactiveCycle()
    {
        this.sut.Observe(this.bindable, nameof(InnerBindable.StringValue));

        this.sut.OnAfterRender();

        this.sut.OnAfterRender();

        this.bindable.StringValue = Values.NewValue;

        this.callback.Verify(x => x(this.sut, EventArgs.Empty), Times.Never);
    }

    [Fact]
    public void PropertyChangedEvent_ShouldNotCallCallback_ForIrrelevantProperty()
    {
        this.sut.Observe(this.bindable, nameof(InnerBindable.NumberValue));

        this.bindable.StringValue = Values.IrrelevantValue;

        this.callback.Verify(x => x(this.sut, EventArgs.Empty), Times.Never);
    }

    public void Dispose()
    {
        this.sut.Dispose();
    }
}