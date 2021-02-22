using Phork.Blazor.Lifecycle;
using System.Collections.Generic;

namespace Phork.Blazor
{
    internal abstract class ObservedBase<T> : RenderElement
    {
        protected ReactivityEntry<T> Entry { get; }

        private T _currentValue;
        protected T CurrentValue
        {
            get
            {
                this.UpdateValue();
                return this._currentValue;
            }
            set
            {
                this.Entry.MemberAccessor.Value = value;
            }
        }

        public ObservedBase(ReactivityEntry<T> entry)
        {
            Guard.ArgumentNotNull(entry, nameof(entry));

            this.Entry = entry;
        }

        protected void UpdateValue()
        {
            var newValue = this.Entry.MemberAccessor.Value;

            if (EqualityComparer<T>.Default.Equals(newValue, this._currentValue))
            {
                return;
            }

            var oldValue = this._currentValue;

            this._currentValue = newValue;

            this.OnValueChanged(oldValue, this._currentValue);
        }

        public abstract void OnValueChanged(T oldValue, T newValue);
    }
}
