using System;
using System.Collections.ObjectModel;
using Moq;
using Phork.Blazor.Services;
using Xunit;

namespace Phork.Blazor.Reactivity.Tests;

public class CollectionObserverTests : IDisposable
{
    private readonly CollectionObserver sut;

    private readonly Mock<EventHandler> callback = new();
    private readonly ObservableCollection<string> collection;

    public CollectionObserverTests()
    {
        this.sut = new();

        this.sut.ObservedCollectionChanged += this.callback.Object;

        this.collection = new();
    }

    [Fact]
    public void CollectionChangedEvent_ShouldCallCallback_ForObservedCollectionDuringCycle()
    {
        this.sut.Observe(this.collection);

        this.collection.Add(Values.NewValue);

        this.callback.Verify(x => x(this.sut, EventArgs.Empty), Times.Once);
    }

    [Fact]
    public void CollectionChangedEvent_ShouldCallCallback_ForEachObservedCollectionDuringCycle()
    {
        var otherCollection = new ObservableCollection<int>();

        this.sut.Observe(this.collection);
        this.sut.Observe(otherCollection);

        this.collection.Add(Values.NewValue);
        otherCollection.Add(1);

        this.callback.Verify(x => x(this.sut, EventArgs.Empty), Times.Exactly(2));
    }

    [Fact]
    public void CollectionChangedEvent_ShouldCallCallback_ForObservedCollectionAfterActiveCycle()
    {
        this.sut.Observe(this.collection);

        this.sut.OnAfterRender();

        this.collection.Add(Values.NewValue);

        this.callback.Verify(x => x(this.sut, EventArgs.Empty), Times.Once);
    }

    [Fact]
    public void CollectionChangedEvent_ShouldNotCallCallback_ForObservedCollectionAfterInactiveCycle()
    {
        this.sut.Observe(this.collection);

        this.sut.OnAfterRender();

        this.sut.OnAfterRender();

        this.collection.Add(Values.NewValue);

        this.callback.Verify(x => x(this.sut, EventArgs.Empty), Times.Never);
    }

    public void Dispose()
    {
        this.sut.Dispose();
    }
}