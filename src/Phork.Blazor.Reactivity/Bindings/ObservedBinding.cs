using System;
using System.Collections.Generic;

namespace Phork.Blazor.Bindings
{
    internal sealed class ObservedBinding<TSource, TTarget> : ObservedBase<TSource>,
        IObservedBinding<TTarget>,
        IObservedBinding
    {
        private TTarget _value;
        public TTarget Value
        {
            get
            {
                this.UpdateValue();
                return this._value;
            }
            set
            {
                if (this.Descriptor.Mode != ObservedBindingMode.TwoWay)
                {
                    return;
                }

                if (EqualityComparer<TTarget>.Default.Equals(value, this._value))
                {
                    return;
                }

                this._value = value;

                var convertedValue = this.Descriptor.Converter.ConvertBack(value);

                if (EqualityComparer<TSource>.Default.Equals(convertedValue, this.CurrentValue))
                {
                    return;
                }

                this.CurrentValue = convertedValue;
            }
        }

        public IObservedBindingDescriptor<TSource, TTarget> Descriptor { get; }
        IObservedBindingDescriptor IObservedBinding.Descriptor => this.Descriptor;


        public ObservedBinding(
            ReactivityEntry<TSource> entry,
            IObservedBindingDescriptor<TSource, TTarget> bindingDescriptor)
            : base(entry)
        {
            Guard.ArgumentNotNull(bindingDescriptor, nameof(bindingDescriptor));

            if (bindingDescriptor.Mode == ObservedBindingMode.TwoWay
                && entry.MemberAccessor.IsReadOnly)
            {
                throw new InvalidOperationException($"Unable to create observed binding. A two-way binding cannot be created with a read-only source. Try using {nameof(ObservedBindingMode)}.{nameof(ObservedBindingMode.OneWay)} instead.");
            }

            this.Descriptor = bindingDescriptor;
        }

        public override void OnValueChanged(TSource oldValue, TSource newValue)
        {
            var convertedValue = this.Descriptor.Converter.Convert(newValue);

            this._value = convertedValue;
        }
    }
}
