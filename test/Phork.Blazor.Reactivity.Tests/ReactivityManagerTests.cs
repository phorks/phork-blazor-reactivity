using System;
using Moq;
using Phork.Blazor.Reactivity.Tests.Models;
using Phork.Blazor.Services;
using Xunit;

namespace Phork.Blazor.Reactivity.Tests;

public class ReactivityManagerTests : IDisposable
{
    private readonly Mock<IPropertyObserver> propertyOberver = new();
    private readonly Mock<ICollectionObserver> collectionObserver = new();
    private readonly Mock<IReactiveComponent> component = new();

    private readonly ReactivityManager sut;

    private readonly RootBindable bindable;

    public ReactivityManagerTests()
    {
        this.sut = new(this.propertyOberver.Object, this.collectionObserver.Object);

        this.bindable = Values.CreateRootBindable();
    }

    [Fact]
    public void Observed_ShouldReturnValue()
    {
        this.Initialize();
        var value = this.sut.Observed(() => this.bindable.Inner.StringValue);

        Assert.Equal(this.bindable.Inner.StringValue, value);
    }

    [Fact]
    public void Observed_ShouldObserveProperties()
    {
        this.Initialize();

        this.sut.Observed(() => this.bindable.Inner.StringValue);

        this.propertyOberver.Verify(x => x.Observe(this.bindable, nameof(RootBindable.Inner)), Times.Once);
        this.propertyOberver.Verify(x => x.Observe(this.bindable.Inner, nameof(InnerBindable.StringValue)), Times.Once);
    }

    [Fact]
    public void ObservedCollection_ShouldReturnValue()
    {
        this.Initialize();

        var collection = this.sut.ObservedCollection(() => this.bindable.Inner.Collection);

        Assert.Equal(this.bindable.Inner.Collection, collection);
    }

    [Fact]
    public void ObservedCollection_ShouldObserveProperties()
    {
        this.Initialize();

        this.sut.Observed(() => this.bindable.Inner.Collection);

        this.propertyOberver.Verify(x => x.Observe(this.bindable, nameof(RootBindable.Inner)), Times.Once);
        this.propertyOberver.Verify(x => x.Observe(this.bindable.Inner, nameof(InnerBindable.Collection)), Times.Once);
    }

    [Fact]
    public void ObservedCollection_ShouldObserveCollection()
    {
        this.Initialize();

        this.sut.ObservedCollection(() => this.bindable.Inner.Collection);

        this.collectionObserver.Verify(x => x.Observe(this.bindable.Inner.Collection), Times.Once);
    }

    [Fact]
    public void Binding_ShouldGetValue()
    {
        this.Initialize();

        var binding = this.sut.Binding(() => this.bindable.Inner.StringValue);

        Assert.Equal(this.bindable.Inner.StringValue, binding.Value);
    }

    [Fact]
    public void Binding_ShouldSetValue()
    {
        this.Initialize();

        var binding = this.sut.Binding(() => this.bindable.Inner.StringValue);

        binding.Value = Values.NewValue;

        Assert.Equal(this.bindable.Inner.StringValue, Values.NewValue);
    }

    [Fact]
    public void Binding_ShouldObserveProperties()
    {
        this.Initialize();

        this.sut.Binding(() => this.bindable.Inner.StringValue);

        this.propertyOberver.Verify(x => x.Observe(this.bindable, nameof(RootBindable.Inner)), Times.Once);
        this.propertyOberver.Verify(x => x.Observe(this.bindable.Inner, nameof(InnerBindable.StringValue)), Times.Once);
    }

    [Fact]
    public void ConvertedBinding_ShouldCallConverterOnGet()
    {
        this.Initialize();

        var converter = new Mock<Func<string, int>>();
        var reverseConverter = new Mock<Func<int, string>>();

        var binding = this.sut.Binding(() => this.bindable.Inner.StringValue, converter.Object, reverseConverter.Object);

        _ = binding.Value;

        converter.Verify(x => x(this.bindable.Inner.StringValue), Times.Once);
        reverseConverter.VerifyNoOtherCalls();
    }

    [Fact]
    public void ConvertedBinding_ShouldCallConverterOnSet()
    {
        this.Initialize();

        var converter = new Mock<Func<string, int>>();
        var reverseConverter = new Mock<Func<int, string>>();

        var binding = this.sut.Binding(() => this.bindable.Inner.StringValue, converter.Object, reverseConverter.Object);

        int newValue = 100;

        binding.Value = newValue;

        reverseConverter.Verify(x => x(newValue), Times.Once);
        converter.VerifyNoOtherCalls();
    }

    [Fact]
    public void ConvertedBinding_ShouldObserveProperties()
    {
        this.Initialize();

        this.sut.Binding(() => this.bindable.Inner.StringValue, x => x, x => x);

        this.propertyOberver.Verify(x => x.Observe(this.bindable, nameof(RootBindable.Inner)), Times.Once);
        this.propertyOberver.Verify(x => x.Observe(this.bindable.Inner, nameof(InnerBindable.StringValue)), Times.Once);
    }

    [Fact]
    public void PropertyObserverObservedPropertyChangedEvent_ShouldNotifyComponent()
    {
        this.Initialize();

        this.propertyOberver.Raise(x => x.ObservedPropertyChanged += null, EventArgs.Empty);

        this.component.Verify(x => x.StateHasChanged(), Times.Once);
    }

    [Fact]
    public void CollectionObserverObservedCollectionChangedEvent_ShouldNotifyComponent()
    {
        this.Initialize();

        this.collectionObserver.Raise(x => x.ObservedCollectionChanged += null, EventArgs.Empty);

        this.component.Verify(x => x.StateHasChanged(), Times.Once);
    }

    [Fact]
    public void OnAfterRender_ShouldCallComponentConfigureBindings()
    {
        this.Initialize();

        this.sut.OnAfterRender();

        this.component.Verify(x => x.ConfigureBindings());
    }

    [Fact]
    public void OnAfterRender_ShouldCallPropertyObserverOnAfterRender()
    {
        this.Initialize();

        this.sut.OnAfterRender();

        this.propertyOberver.Verify(x => x.OnAfterRender());
    }

    [Fact]
    public void OnAfterRender_ShouldCallCollectionObserverOnAfterRender()
    {
        this.Initialize();

        this.sut.OnAfterRender();

        this.collectionObserver.Verify(x => x.OnAfterRender());
    }

    [Fact]
    public void OnAfterRender_ShouldOnlyObserveActiveProperties()
    {
        this.Initialize();

        this.sut.Observed(() => this.bindable.Inner.StringValue);
        this.sut.Observed(() => this.bindable.Inner.NumberValue);

        this.sut.OnAfterRender();

        this.propertyOberver.Invocations.Clear();

        this.sut.Observed(() => this.bindable.Inner.StringValue);

        this.sut.OnAfterRender();

        this.propertyOberver.Verify(x => x.Observe(this.bindable, nameof(RootBindable.Inner)), Times.Once);
        this.propertyOberver.Verify(x => x.Observe(this.bindable.Inner, nameof(InnerBindable.StringValue)), Times.Once);
        this.propertyOberver.Verify(x => x.Observe(this.bindable.Inner, nameof(InnerBindable.NumberValue)), Times.Never);
    }

    [Fact]
    public void Dispose_ShouldDisposePropertyObserver()

    {
        this.Initialize();

        this.sut.Dispose();

        this.propertyOberver.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldDisposeCollectionObserver()
    {
        this.Initialize();

        this.sut.Dispose();

        this.collectionObserver.Verify(x => x.Dispose(), Times.Once);
    }

    #region Not Initialized Exception Tests

    [Fact]
    public void Observed_ShouldThrowInvalidOperationException_WhenNotInitialized()
    {
        Assert.Throws<InvalidOperationException>(() => this.sut.Observed(() => this.bindable.Inner.StringValue));
    }

    [Fact]
    public void ObservedCollection_ShouldThrowInvalidOperationException_WhenNotInitialized()
    {
        Assert.Throws<InvalidOperationException>(() => this.sut.ObservedCollection(() => this.bindable.Inner.Collection));
    }

    [Fact]
    public void Binding_ShouldThrowInvalidOperationException_WhenNotInitialized()
    {
        Assert.Throws<InvalidOperationException>(() => this.sut.Binding(() => this.bindable.Inner.StringValue));
    }

    [Fact]
    public void ConvertedBinding_ShouldThrowInvalidOperationException_WhenNotInitialized()
    {
        Assert.Throws<InvalidOperationException>(() => this.sut.Binding(() => this.bindable.Inner.StringValue, x => x, x => x));
    }

    [Fact]
    public void OnAfterRender_ShouldThrowInvalidOperationException_WhenNotInitialized()
    {
        Assert.Throws<InvalidOperationException>(() => this.sut.OnAfterRender());
    }

    #endregion Not Initialized Exception Tests

    #region Disposed Exception Tests

    [Fact]
    public void Initialize_ShouldThrowObjectDisposedException_WhenDisposed()
    {
        this.sut.Dispose();

        Assert.Throws<ObjectDisposedException>(() => this.sut.Initialize(this.component.Object));
    }

    [Fact]
    public void Observed_ShouldThrowObjectDisposedException_WhenDisposed()
    {
        this.sut.Dispose();

        Assert.Throws<ObjectDisposedException>(() => this.sut.Observed(() => this.bindable.Inner.StringValue));
    }

    [Fact]
    public void ObservedCollection_ShouldThrowObjectDisposedException_WhenDisposed()
    {
        this.sut.Dispose();

        Assert.Throws<ObjectDisposedException>(() => this.sut.ObservedCollection(() => this.bindable.Inner.Collection));
    }

    [Fact]
    public void Binding_ShouldThrowObjectDisposedException_WhenDisposed()
    {
        this.sut.Dispose();

        Assert.Throws<ObjectDisposedException>(() => this.sut.Binding(() => this.bindable.Inner.StringValue));
    }

    [Fact]
    public void ConvertedBinding_ShouldThrowObjectDisposedException_WhenDisposed()
    {
        this.sut.Dispose();

        Assert.Throws<ObjectDisposedException>(() => this.sut.Binding(() => this.bindable.Inner.StringValue, x => x, x => x));
    }

    [Fact]
    public void OnAfterRender_ShouldThrowObjectDisposedException_WhenDisposed()
    {
        this.sut.Dispose();

        Assert.Throws<ObjectDisposedException>(() => this.sut.OnAfterRender());
    }

    #endregion Disposed Exception Tests

    public void Dispose()
    {
        this.sut.Dispose();
    }

    private void Initialize()
    {
        this.sut.Initialize(this.component.Object);
    }
}