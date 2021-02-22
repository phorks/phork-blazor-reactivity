using Microsoft.AspNetCore.Components;
using Phork.Blazor.Bindings;
using Phork.Data;
using System;
using System.Linq.Expressions;

namespace Phork.Blazor
{
    public abstract class ReactiveComponentBase : ComponentBase,
        IReactiveComponent,
        IDisposable
    {
        private readonly ReactivityManager reactivityManager;

        public ReactiveComponentBase()
        {
            this.reactivityManager = new ReactivityManager(this);
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            this.reactivityManager.OnAfterRender();
        }

        protected virtual void ConfigureBindings()
        {
        }

        protected T Observed<T>(Expression<Func<T>> valueAccessor)
        {
            return this.reactivityManager.Observed(valueAccessor);
        }

        protected IObservedBinding<T> Binding<T>(
            Expression<Func<T>> valueAccessor,
            ObservedBindingMode mode = ObservedBindingMode.TwoWay)
        {
            return this.reactivityManager.Binding(valueAccessor, mode);
        }

        protected IObservedBinding<TTarget> Binding<TSource, TTarget>(
            Expression<Func<TSource>> valueAccessor,
            Func<TSource, TTarget> converter)
        {
            return this.reactivityManager.Binding(valueAccessor, converter);
        }

        protected IObservedBinding<TTarget> Binding<TSource, TTarget>(
            Expression<Func<TSource>> valueAccessor,
            Func<TSource, TTarget> converter,
            Func<TTarget, TSource> reverseConverter)
        {
            return this.reactivityManager.Binding(valueAccessor, converter, reverseConverter);
        }

        protected IObservedBinding<TTarget> Binding<TSource, TTarget>(
            Expression<Func<TSource>> valueAccessor,
            IValueConverter<TSource, TTarget> converter,
            ObservedBindingMode mode)
        {
            return this.reactivityManager.Binding(valueAccessor, converter, mode);
        }

        public virtual void Dispose()
        {
            this.reactivityManager.Dispose();

            GC.SuppressFinalize(this);
        }

        void IReactiveComponent.StateHasChanged()
        {
            this.InvokeAsync(this.StateHasChanged);
        }

        void IReactiveComponent.ConfigureBindings()
        {
            this.ConfigureBindings();
        }
    }
}
