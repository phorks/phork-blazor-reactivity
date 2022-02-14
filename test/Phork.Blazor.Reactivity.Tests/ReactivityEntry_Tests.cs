using System;
using System.Collections.ObjectModel;
using Phork.Blazor.Bindings;
using Phork.Blazor.Reactivity.Tests.Models;
using Phork.Expressions;
using Xunit;

namespace Phork.Blazor.Reactivity.Tests;

public class ReactivityEntry_Tests : IDisposable
{
    protected const string TEST_VALUE = "test";
    protected const string NEW_VALUE = "new";
    protected const string IRRELEVANT_VALUE = "irrelevant";
    protected const string RELEVANT_VALUE = "relevant";

    private readonly Action stateHasChanged;
    private readonly ReactivityEntry<string> entry;
    private readonly ReactivityEntry<ObservableCollection<string>> collectionEntry;

    private bool isStateHasChangedCalled = false;
    private FirstBindable bindable;

    public ReactivityEntry_Tests()
    {
        this.stateHasChanged = () => this.isStateHasChangedCalled = true;

        this.bindable = new FirstBindable
        {
            Second = new SecondBindable
            {
                Value = TEST_VALUE
            }
        };

        var accessor = MemberAccessor.Create(() => this.bindable.Second.Value);

        this.entry = new ReactivityEntry<string>(accessor, this.stateHasChanged);
        this.entry.Touch();

        var collectionAccessor = MemberAccessor.Create(() => this.bindable.Second.Values);

        this.collectionEntry = new ReactivityEntry<ObservableCollection<string>>(
            collectionAccessor,
            this.stateHasChanged);
        this.collectionEntry.Touch();
    }

    public void Dispose()
    {
        this.entry.Dispose();
        this.collectionEntry.Dispose();

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ObservedValue_Is_Equal_To_Actual_Value()
    {
        var value = this.entry.ObservedValue;

        Assert.Equal(this.bindable.Second.Value, value.Value);
    }

    [Fact]
    public void ObservedValue_Change_Calls_Callback()
    {
        _ = this.entry.ObservedValue;

        this.bindable.Second.Value = NEW_VALUE;

        Assert.True(this.isStateHasChangedCalled);

        this.isStateHasChangedCalled = false;

        this.bindable.Second = new SecondBindable();

        Assert.True(this.isStateHasChangedCalled);
    }

    [Fact]
    public void ObservedValues_Are_Same_When_Active()
    {
        var value = this.entry.ObservedValue;

        this.entry.TryDispose();

        this.entry.Touch();

        var newValue = this.entry.ObservedValue;

        Assert.Same(newValue, value);
    }

    [Fact]
    public void ObservedValues_Are_Not_Same_When_Inactive()
    {
        var value = this.entry.ObservedValue;

        // The First render cycle is done and the value has been active
        this.entry.TryDispose();

        // The second render has started

        // The entry itself has been active in the second cycle (e.g. by GetBinding) so the entry
        //  is not disposed of
        this.entry.Touch();

        // The second render cycle is done and the value has not been active,
        //  therefore it should be disposed of
        this.entry.TryDispose();

        this.entry.Touch();

        var newValue = this.entry.ObservedValue;

        Assert.NotSame(newValue, value);
    }

    [Fact]
    public void ObservedValue_Is_Refreshed_On_Each_Render()
    {
        var value = this.entry.ObservedValue;

        var oldBindable = this.bindable;

        this.bindable = new FirstBindable()
        {
            Second = new SecondBindable()
            {
                Value = NEW_VALUE
            }
        };

        this.entry.TryDispose();

        this.entry.Touch();

        // The second render has started

        var newValue = this.entry.ObservedValue;

        Assert.Same(newValue, value);

        oldBindable.Second.Value = IRRELEVANT_VALUE;

        Assert.False(this.isStateHasChangedCalled);

        this.bindable.Second.Value = RELEVANT_VALUE;

        Assert.True(this.isStateHasChangedCalled);

        Assert.Equal(this.bindable.Second.Value, newValue.Value);
    }

    [Fact]
    public void Component_Is_Updated_When_ObservedValue_Collection_Is_Modified()
    {
        _ = this.collectionEntry.ObservedValue.Value;

        this.bindable.Second.Values.Add(NEW_VALUE);

        Assert.True(this.isStateHasChangedCalled);
    }

    [Fact]
    public void ObservedValue_Collection_Is_Refreshed_On_Each_Render()
    {
        var value = this.collectionEntry.ObservedValue;

        _ = value.Value;

        this.collectionEntry.TryDispose();

        this.collectionEntry.Touch();

        // The second render has started

        var oldBindable = this.bindable;

        this.bindable = new FirstBindable()
        {
            Second = new SecondBindable()
            {
                Values = new ObservableCollection<string>()
            }
        };

        var newValue = this.collectionEntry.ObservedValue;

        _ = value.Value;

        Assert.Same(newValue, value);

        oldBindable.Second.Values.Add(IRRELEVANT_VALUE);

        Assert.False(this.isStateHasChangedCalled);

        this.bindable.Second.Values.Add(NEW_VALUE);

        Assert.True(this.isStateHasChangedCalled);
    }

    [Fact]
    public void ObservedBinding_Value_Reading_Works()
    {
        var bindingDescriptor = ObservedBindingDescriptor.Create<string>(ObservedBindingMode.OneWay);
        var binding = this.entry.GetBinding(bindingDescriptor);

        var expectedValue = this.bindable.Second.Value;

        Assert.Equal(expectedValue, binding.Value);
    }

    [Fact]
    public void ObservedBinding_Value_Writing_Works()
    {
        var bindingDescriptor = ObservedBindingDescriptor.Create<string>(ObservedBindingMode.TwoWay);
        var binding = this.entry.GetBinding(bindingDescriptor);

        binding.Value = NEW_VALUE;

        var actualValue = this.bindable.Second.Value;

        Assert.Equal(NEW_VALUE, actualValue);
    }

    [Fact]
    public void ObservedBinding_Value_OneWay_Writing_Throws_Exception()
    {
        var bindingDescriptor = ObservedBindingDescriptor.Create<string>(ObservedBindingMode.OneWay);
        var binding = this.entry.GetBinding(bindingDescriptor);

        Assert.Throws<InvalidOperationException>(() => binding.Value = IRRELEVANT_VALUE);
    }

    [Fact]
    public void ObservedBinding_OneWay_Delegate_Converter_Works()
    {
        Func<string, int> converter = x => x.GetHashCode();

        var bindingDescriptor = ObservedBindingDescriptor.Create(
            converter);
        var binding = this.entry.GetBinding(bindingDescriptor);

        this.bindable.Second.Value = NEW_VALUE;

        var newValue = binding.Value;

        Assert.Equal(NEW_VALUE.GetHashCode(), newValue);
    }

    [Fact]
    public void ObservedBinding_TwoWay_Delegate_Converter_Works()
    {
        Func<string, int> converter = x =>
        {
            return x switch
            {
                TEST_VALUE => 0,
                NEW_VALUE => 1,
                _ => -1
            };
        };

        Func<int, string> reverseConverter = x =>
        {
            return x switch
            {
                0 => TEST_VALUE,
                1 => NEW_VALUE,
                _ => string.Empty
            };
        };

        var bindingDescriptor = ObservedBindingDescriptor.Create(
            converter,
            reverseConverter);
        var binding = this.entry.GetBinding(bindingDescriptor);

        this.bindable.Second.Value = NEW_VALUE;

        var newValue = binding.Value;

        Assert.Equal(1, newValue);

        binding.Value = 0;

        Assert.Equal(TEST_VALUE, this.bindable.Second.Value);
    }

    [Fact]
    public void ObservedBinding_Is_Same_For_Same_Descriptor()
    {
        Func<string, int> converter = x =>
        {
            return x switch
            {
                TEST_VALUE => 0,
                NEW_VALUE => 1,
                _ => -1
            };
        };

        Func<int, string> reverseConverter = x =>
        {
            return x switch
            {
                0 => TEST_VALUE,
                1 => NEW_VALUE,
                _ => string.Empty
            };
        };

        var bindingDescriptor1 = ObservedBindingDescriptor.Create(
            converter,
            reverseConverter);

        var binding1 = this.entry.GetBinding(bindingDescriptor1);

        var bindingDescriptor2 = ObservedBindingDescriptor.Create(
            converter,
            reverseConverter);
        var binding2 = this.entry.GetBinding(bindingDescriptor2);

        Assert.Same(binding1, binding2);
    }

    [Fact]
    public void ObservedBinding_Is_Disposed_When_Inactive()
    {
        static int converter(string x)
        {
            return x switch
            {
                TEST_VALUE => 0,
                NEW_VALUE => 1,
                _ => -1
            };
        }

        static string reverseConverter(int x)
        {
            return x switch
            {
                0 => TEST_VALUE,
                1 => NEW_VALUE,
                _ => string.Empty
            };
        }

        var bindingDescriptor = ObservedBindingDescriptor.Create<string, int>(
            converter,
            reverseConverter);

        var binding = this.entry.GetBinding(bindingDescriptor);

        this.entry.TryDispose();

        this.entry.Touch();

        this.entry.TryDispose();

        this.entry.Touch();

        var newBinding = this.entry.GetBinding(bindingDescriptor);

        Assert.NotSame(binding, newBinding);
    }
}