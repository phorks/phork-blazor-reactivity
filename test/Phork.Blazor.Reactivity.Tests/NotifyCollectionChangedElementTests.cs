using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Moq;
using Phork.Blazor.Lifecycle;
using Xunit;

namespace Phork.Blazor.Reactivity.Tests;

public class NotifyCollectionChangedElementTests : LifecycleElementTests
{
    private readonly NotifyCollectionChangedElement sut;
    private readonly ObservableCollection<string> collection;

    private readonly Mock<Action<INotifyCollectionChanged, NotifyCollectionChangedEventArgs>> callback = new();

    private protected override LifecycleElement Element => this.sut;

    public NotifyCollectionChangedElementTests()
    {
        this.collection = new();

        this.sut = new(this.collection, this.callback.Object);
    }

    [Fact]
    public void CollectionChangedEvent_ShouldCallCallback_WhenActive()
    {
        this.sut.Touch();
        this.collection.Add(Values.NewValue);

        this.callback.Verify(x => x(this.collection, It.IsAny<NotifyCollectionChangedEventArgs>()), Times.Once());
    }

    [Fact]
    public void CollectionChangedEvent_ShouldNotCallCallback_WhenDisposed()
    {
        this.sut.Dispose();
        this.collection.Add(Values.NewValue);

        this.callback.Verify(x => x(It.IsAny<INotifyCollectionChanged>(), It.IsAny<NotifyCollectionChangedEventArgs>()), Times.Never());
    }

    [Fact]
    public void Touch_ShouldMakeElementSurvive()
    {
        this.sut.Touch();
        this.sut.NotifyCycleEnded();

        Assert.False(this.sut.IsDisposed);
    }
}