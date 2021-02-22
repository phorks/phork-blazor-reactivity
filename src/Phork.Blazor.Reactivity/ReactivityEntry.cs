using Phork.Blazor.Bindings;
using Phork.Blazor.Lifecycle;
using Phork.Blazor.Values;
using Phork.Data;
using System;
using System.Collections.Generic;

namespace Phork.Blazor
{
    internal sealed class ReactivityEntry<T> : RenderElement, IReactivityEntry, IDisposable
    {
        private bool isDisposed;

        public Action StateHasChanged { get; }

        private ObservedValue<T> value;

        private Dictionary<IObservedBindingDescriptor, IObservedBinding> bindings
            = new Dictionary<IObservedBindingDescriptor, IObservedBinding>();

        public MemberAccessor<T> MemberAccessor { get; }
        public ObservedProperty<T> ObservedProperty { get; }


        public ReactivityEntry(MemberAccessor<T> accessor, Action stateHasChanged)
        {
            Guard.ArgumentNotNull(accessor, nameof(accessor));
            Guard.ArgumentNotNull(stateHasChanged, nameof(stateHasChanged));

            this.MemberAccessor = accessor;
            this.StateHasChanged = stateHasChanged;

            if (accessor.Type != MemberAccessorType.Constant)
            {
                this.ObservedProperty = Data.ObservedProperty.Create(accessor, this.StateHasChanged);
            }
        }

        public ObservedValue<T> GetValue()
        {
            this.EnsureNotDisposed();

            if (this.value == null)
            {
                this.value = new ObservedValue<T>(this);
            }

            this.value.Touch();

            return this.value;
        }

        public IObservedBinding<TTarget> GetBinding<TTarget>(
            IObservedBindingDescriptor<T, TTarget> bindingDescriptor)
        {
            Guard.ArgumentNotNull(bindingDescriptor, nameof(bindingDescriptor));

            this.EnsureNotDisposed();

            IObservedBinding<TTarget> binding;

            if (this.bindings.TryGetValue(bindingDescriptor, out var existingBinding))
            {
                binding = existingBinding as IObservedBinding<TTarget>;
            }
            else
            {
                binding = new ObservedBinding<T, TTarget>(this, bindingDescriptor);
                this.bindings[bindingDescriptor] = binding;
            }

            binding.Touch();
            return binding;
        }

        public override bool TryCleanUp()
        {
            if (base.TryCleanUp())
            {
                return true;
            }

            if (this.value?.TryCleanUp() == true)
            {
                this.value = null;
            }

            var inactiveBindings = new List<IObservedBindingDescriptor>();
            foreach (var binding in this.bindings)
            {
                if (binding.Value.TryCleanUp())
                {
                    inactiveBindings.Add(binding.Key);
                }
            }

            foreach (var key in inactiveBindings)
            {
                this.bindings.Remove(key);
            }

            return false;
        }

        protected override void Activate(bool firstActivation)
        {
            base.Activate(firstActivation);

            if (!firstActivation)
            {
                this.ObservedProperty?.TryRefreshSubscriptions();
            }
        }

        private void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(typeof(ReactivityEntry<T>).Name);
            }
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            this.ObservedProperty?.Dispose();

            this.value?.Dispose();
            this.value = null;

            foreach (var binding in this.bindings.Values)
            {
                (binding as IDisposable)?.Dispose();
            }

            this.bindings.Clear();

            this.isDisposed = true;
        }
    }
}
