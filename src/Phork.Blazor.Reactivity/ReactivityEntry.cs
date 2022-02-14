using System;
using System.Collections.Generic;
using Phork.Blazor.Bindings;
using Phork.Blazor.Lifecycle;
using Phork.Blazor.Values;
using Phork.Data;
using Phork.Expressions;

namespace Phork.Blazor;

internal sealed class ReactivityEntry<T> : RenderElement, IReactivityEntry
{
    private readonly Dictionary<IObservedBindingDescriptor, IObservedBinding> bindings = new();

    public Action StateHasChanged { get; }

    private ObservedValue<T>? _observedValue;

    public ObservedValue<T> ObservedValue
    {
        get
        {
            this.EnsureNotDisposed();

            if (this._observedValue == null)
            {
                this._observedValue = new ObservedValue<T>(this);
            }

            this._observedValue.Touch();

            return this._observedValue;
        }
    }

    public MemberAccessor<T> MemberAccessor { get; }
    public ObservedProperty<T>? ObservedProperty { get; }

    public bool IsPropertyAccessible
        => this.ObservedProperty == null || this.ObservedProperty.IsAccessible;

    public ReactivityEntry(MemberAccessor<T> accessor, Action stateHasChanged)
    {
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(stateHasChanged);

        this.MemberAccessor = accessor;
        this.StateHasChanged = stateHasChanged;

        if (accessor.Type != MemberAccessorType.Constant)
        {
            this.ObservedProperty = Data.ObservedProperty.Create(accessor, this.StateHasChanged);
        }
    }

    //public T? GetObservedValue()
    //{
    //    var observedValue = this.ObservedValue;

    //    T? value;
    //    if (this.IsPropertyAccessible)
    //    {
    //        value = observedValue.Value;
    //    }
    //    else
    //    {
    //        observedValue.ClearValue();
    //        value = default;
    //    }

    //    return value;
    //}

    //public T GetObservedValue(Func<T> fallbackValue)
    //{
    //    ArgumentNullException.ThrowIfNull(fallbackValue);

    //    var observedValue = this.GetOrCreateObservedValue();

    //    T value;
    //    if (this.IsPropertyAccessible)
    //    {
    //        value = observedValue.Value;
    //    }
    //    else
    //    {
    //        observedValue.ClearValue();
    //        value = fallbackValue();
    //    }

    //    return value;
    //}

    public IObservedBinding<TTarget> GetBinding<TTarget>(
        IObservedBindingDescriptor<T, TTarget> bindingDescriptor)
    {
        ArgumentNullException.ThrowIfNull(bindingDescriptor);

        this.EnsureNotDisposed();

        IObservedBinding<TTarget> binding;

        if (this.bindings.TryGetValue(bindingDescriptor, out var existingBinding))
        {
            if (existingBinding is not IObservedBinding<TTarget> existingTypedBinding)
            {
                throw new InvalidOperationException("Unable to get binding. An existing binding conforming to the given descriptor has unmatching target type.");
            }

            binding = existingTypedBinding;
        }
        else
        {
            binding = new ObservedBinding<T, TTarget>(this, bindingDescriptor);
            this.bindings[bindingDescriptor] = binding;
        }

        binding.Touch();
        return binding;
    }

    /// <inheritdoc/>
    public override bool TryDispose()
    {
        if (base.TryDispose())
        {
            // The entry is already disposed of by the base class. No need to take any action.
            return true;
        }

        if (this._observedValue?.TryDispose() == true)
        {
            this._observedValue = null;
        }

        var inactiveBindings = new List<IObservedBindingDescriptor>();
        foreach (var binding in this.bindings)
        {
            if (binding.Value.TryDispose())
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

    protected override void OnActivated(bool firstActivation)
    {
        base.OnActivated(firstActivation);

        if (!firstActivation)
        {
            this.ObservedProperty?.TryRefreshSubscriptions();
        }
    }

    public override void Dispose()
    {
        this.ObservedProperty?.Dispose();

        this._observedValue?.Dispose();
        this._observedValue = null;

        foreach (var binding in this.bindings.Values)
        {
            binding.Dispose();
        }

        this.bindings.Clear();

        base.Dispose();
    }
}