using Phork.Blazor.Bindings;
using Phork.Blazor.Reactivity.Tests.Models;
using Phork.Data;
using System;
using System.Collections.ObjectModel;
using Xunit;

namespace Phork.Blazor.Reactivity.Tests
{
    public class ReactivityEntry_Tests : IDisposable
    {
        protected const string TEST_VALUE = "test";
        protected const string NEW_VALUE = "new";
        protected const string IRRELEVANT_VALUE = "irrelevant";

        private protected readonly Action stateHasChanged;
        private protected ReactivityEntry<string> entry;
        private protected readonly ReactivityEntry<ObservableCollection<string>> collectionEntry;

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
        }

        public void Dispose()
        {
            this.entry.Dispose();
            this.collectionEntry.Dispose();
        }

        [Fact]
        public void ObservedValue_Is_Equal_To_The_Actual_Value()
        {
            var value = this.entry.GetValue();

            Assert.Equal(this.bindable.Second.Value, value.Value);
        }

        [Fact]
        public void ObservedValue_Change_Calls_Callback()
        {
            var value = this.entry.GetValue();

            this.bindable.Second.Value = NEW_VALUE;

            Assert.True(this.isStateHasChangedCalled);

            this.isStateHasChangedCalled = false;

            this.bindable.Second = new SecondBindable();

            Assert.True(this.isStateHasChangedCalled);
        }

        [Fact]
        public void ObservedValues_Are_The_Same_When_Active()
        {
            var value = this.entry.GetValue();

            this.entry.TryCleanUp();

            var newValue = this.entry.GetValue();

            Assert.Same(newValue, value);
        }

        [Fact]
        public void ObservedValues_Are_Not_The_Same_When_Inactive()
        {
            var value = this.entry.GetValue();

            // The First render cycle is done and the value has been active
            this.entry.TryCleanUp();

            // The second render has started

            // The entry itself has been active in the second cycle (e.g. by GetBinding) so the entry
            //  is not cleaned up
            this.entry.Touch();

            // The second render cycle is done and the value has not been active,
            //  therefore it should be cleaned up
            this.entry.TryCleanUp();

            var newValue = this.entry.GetValue();

            Assert.NotSame(newValue, value);
        }

        [Fact]
        public void ObservedValue_Is_Refreshed_On_Each_Render()
        {
            var value = this.entry.GetValue();

            this.entry.TryCleanUp();

            // The second render has started

            var oldBindable = this.bindable;

            this.bindable = new FirstBindable()
            {
                Second = new SecondBindable()
                {
                    Value = TEST_VALUE
                }
            };

            var newValue = this.entry.GetValue();

            Assert.Same(newValue, value);

            oldBindable.Second.Value = IRRELEVANT_VALUE;

            Assert.False(this.isStateHasChangedCalled);

            this.bindable.Second.Value = NEW_VALUE;

            Assert.True(this.isStateHasChangedCalled);

            Assert.Equal(this.bindable.Second.Value, newValue.Value);
        }

        [Fact]
        public void Component_Is_Updated_When_The_ObservedValue_Collection_Is_Modified()
        {
            _ = this.collectionEntry.GetValue().Value;

            this.bindable.Second.Values.Add(NEW_VALUE);

            Assert.True(this.isStateHasChangedCalled);
        }

        [Fact]
        public void ObservedValue_Collection_Is_Refreshed_On_Each_Render()
        {
            var value = this.collectionEntry.GetValue();

            _ = value.Value;

            this.collectionEntry.TryCleanUp();

            // The second render has started

            var oldBindable = this.bindable;

            this.bindable = new FirstBindable()
            {
                Second = new SecondBindable()
                {
                    Values = new ObservableCollection<string>()
                }
            };

            var newValue = this.collectionEntry.GetValue();

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
        public void ObservedBinding_Value_OneWay_Writing_Does_Not_Update_Value()
        {
            var bindingDescriptor = ObservedBindingDescriptor.Create<string>(ObservedBindingMode.OneWay);
            var binding = this.entry.GetBinding(bindingDescriptor);

            binding.Value = IRRELEVANT_VALUE;

            var actualValue = this.bindable.Second.Value;

            Assert.Equal(TEST_VALUE, actualValue);
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
        public void ObservedBinding_Is_The_Same_For_Same_Descriptor()
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
        public void ObservedBinding_Is_Cleaned_Up_When_Inactive()
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

            this.entry.TryCleanUp();

            this.entry.Touch();

            this.entry.TryCleanUp();

            var newBinding = this.entry.GetBinding(bindingDescriptor);

            Assert.NotSame(binding, newBinding);
        }
    }
}