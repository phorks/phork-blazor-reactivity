using Phork.Blazor.Bindings;
using Phork.Data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Phork.Blazor
{
    public sealed class ReactivityManager : IDisposable
    {
        private bool isDisposed = false;

        private readonly Dictionary<MemberAccessor, IReactivityEntry> entries
            = new Dictionary<MemberAccessor, IReactivityEntry>();

        private readonly IReactiveComponent component;

        public ReactivityManager(IReactiveComponent component)
        {
            Guard.ArgumentNotNull(component, nameof(component));

            this.component = component;
        }

        private ReactivityEntry<T> GetEntry<T>(MemberAccessor<T> accessor)
        {
            ReactivityEntry<T> entry;

            if (this.entries.TryGetValue(accessor, out var existing))
            {
                entry = existing as ReactivityEntry<T>;
            }
            else
            {
                entry = new ReactivityEntry<T>(accessor, this.StateHasChanged);
                this.entries[accessor] = entry;
            }

            entry.Touch();

            return entry;
        }

        private IObservedBinding<TTarget> GetBinding<TSource, TTarget>(
            Expression<Func<TSource>> accessorExpression,
            IObservedBindingDescriptor<TSource, TTarget> bindingDescriptor)
        {
            this.EnsureNotDisposed();
            var memberAccessor = MemberAccessor.Create(accessorExpression);
            var entry = this.GetEntry(memberAccessor);
            return entry.GetBinding(bindingDescriptor);
        }

        private T Observed<T>(MemberAccessor<T> accessor)
        {
            this.EnsureNotDisposed();
            return this.GetEntry(accessor).GetValue().Value;
        }

        public T Observed<T>(Expression<Func<T>> accessor)
        {
            Guard.ArgumentNotNull(accessor, nameof(accessor));
            return this.Observed(MemberAccessor.Create(accessor));
        }



        public void OnAfterRender()
        {
            this.EnsureNotDisposed();

            this.component.ConfigureBindings();

            List<MemberAccessor> inactiveEntries = new List<MemberAccessor>();
            foreach (var entry in this.entries)
            {
                if (entry.Value.TryCleanUp())
                {
                    inactiveEntries.Add(entry.Key);
                }
            }

            foreach (var item in inactiveEntries)
            {
                this.entries.Remove(item);
            }
        }

        private void StateHasChanged()
        {
            this.component.StateHasChanged();
        }

        private void EnsureNotDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(typeof(ReactivityManager).FullName);
            }
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            foreach (var entry in this.entries.Values)
            {
                (entry as IDisposable).Dispose();
            }
        }

        #region Binding Creation Overloads
        public IObservedBinding<T> Binding<T>(
            Expression<Func<T>> accessor,
            ObservedBindingMode mode = ObservedBindingMode.TwoWay)
        {
            Guard.ArgumentNotNull(accessor, nameof(accessor));

            var descriptor = ObservedBindingDescriptor.Create<T>(mode);

            return this.GetBinding(accessor, descriptor);
        }

        public IObservedBinding<TTarget> Binding<TSource, TTarget>(
            Expression<Func<TSource>> accessor,
            Func<TSource, TTarget> converter)
        {
            Guard.ArgumentNotNull(accessor, nameof(accessor));
            Guard.ArgumentNotNull(converter, nameof(converter));

            var descriptor = ObservedBindingDescriptor.Create(converter);

            return this.GetBinding(accessor, descriptor);
        }

        public IObservedBinding<TTarget> Binding<TSource, TTarget>(
            Expression<Func<TSource>> accessor,
            Func<TSource, TTarget> converter,
            Func<TTarget, TSource> reverseConverter)
        {
            Guard.ArgumentNotNull(accessor, nameof(accessor));
            Guard.ArgumentNotNull(converter, nameof(converter));
            Guard.ArgumentNotNull(reverseConverter, nameof(reverseConverter));

            var descriptor = ObservedBindingDescriptor.Create(converter, reverseConverter);

            return this.GetBinding(accessor, descriptor);
        }

        public IObservedBinding<TTarget> Binding<TSource, TTarget>(
            Expression<Func<TSource>> accessor,
            IValueConverter<TSource, TTarget> converter,
            ObservedBindingMode mode)
        {
            Guard.ArgumentNotNull(accessor, nameof(accessor));
            Guard.ArgumentNotNull(converter, nameof(converter));

            var descriptor = ObservedBindingDescriptor.Create(converter, mode);

            return this.GetBinding(accessor, descriptor);
        }
        #endregion
    }
}
