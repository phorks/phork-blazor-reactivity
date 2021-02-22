using Phork.Data;
using System;
using System.Collections.Specialized;

namespace Phork.Blazor.Values
{
    internal sealed class ObservedValue<T> : ObservedBase<T>,
        IValueReader<T>,
        IDisposable
    {
        public T Value
        {
            get => this.CurrentValue;
        }

        public ObservedValue(ReactivityEntry<T> entry) : base(entry)
        {
        }

        public override void OnValueChanged(T oldValue, T newValue)
        {
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= this.Value_CollectionChanged;
            }

            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += this.Value_CollectionChanged;
            }
        }

        private void Value_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Entry.StateHasChanged();
        }

        public void Dispose()
        {
            if (this.CurrentValue is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged -= this.Value_CollectionChanged;
            }
        }
    }
}
