using System;
using Moq;
using Phork.Blazor.Reactivity.Tests.Models;
using Xunit;

namespace Phork.Blazor.Reactivity.Tests;

public class ReactivityManager_Tests : IDisposable
{
    private readonly Mock<IReactiveComponent> component;
    private readonly ReactivityManager reactivityManager;

    private readonly FirstBindable bindable;

    public ReactivityManager_Tests()
    {
        this.component = new Mock<IReactiveComponent>();
        this.component.Setup(x => x.StateHasChanged()).Verifiable();

        this.reactivityManager = new ReactivityManager(this.component.Object);

        this.bindable = new FirstBindable()
        {
            Second = new SecondBindable()
            {
                Value = "test"
            }
        };
    }

    public void Dispose()
    {
        this.reactivityManager.Dispose();

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ObservedValue_Is_Equal_To_Actual_Value()
    {
        var value = this.reactivityManager.Observed(() => this.bindable.Second.Value);

        Assert.Equal(this.bindable.Second.Value, value);
    }

    [Fact]
    public void Component_Is_Updated_When_ObservedValue_Is_Changed()
    {
        var value = this.reactivityManager.Observed(() => this.bindable.Second.Value);

        Assert.Equal(this.bindable.Second.Value, value);

        this.bindable.Second.Value = "new";

        this.component.Verify();
    }
}