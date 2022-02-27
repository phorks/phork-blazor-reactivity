using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq.Expressions;
using Phork.Blazor.Expressions;
using Phork.Blazor.Services;

namespace Phork.Blazor.Lifecycle;

internal class MemberAccessorElement<T> : LifecycleElement, IMemberAccessorElement
{
    private readonly IPropertyObserver propertyObserver;

    private readonly ImmutableArray<Delegate> targetAccessors;
    private readonly ImmutableArray<string> memberNames;

    private readonly List<object>? targets;

    public MemberAccessor<T> Accessor { get; }

    public bool IsAccessible => this.targets == null || this.targets.Count == this.targetAccessors.Length;

    public MemberAccessorElement(
        MemberAccessor<T> accessor,
        IPropertyObserver propertyObserver)
    {
        ArgumentNullException.ThrowIfNull(accessor);
        ArgumentNullException.ThrowIfNull(propertyObserver);

        this.Accessor = accessor;
        this.propertyObserver = propertyObserver;

        if (accessor.Type == MemberAccessorType.Member
            && accessor.MemberExpressions is not null)
        {
            var targetAccessors = new List<Delegate>();
            var memberNames = new List<string>();

            foreach (var member in accessor.MemberExpressions)
            {
                var targetAccessor = Expression.Lambda(member.Expression!).Compile();
                targetAccessors.Add(targetAccessor);

                memberNames.Add(member.Member.Name);
            }

            this.targetAccessors = targetAccessors.ToImmutableArray();
            this.memberNames = memberNames.ToImmutableArray();
            this.targets = new(this.targetAccessors.Length);
        }
        else
        {
            this.targetAccessors = ImmutableArray<Delegate>.Empty;
            this.memberNames = ImmutableArray<string>.Empty;
        }
    }

    protected override void OnTouched()
    {
        base.OnTouched();

        if (this.targets == null)
        {
            return;
        }

        this.targets.Clear();

        for (int i = 0; i < this.targetAccessors.Length; i++)
        {
            var target = this.targetAccessors[i].DynamicInvoke();

            if (target is null)
            {
                break;
            }

            this.targets.Add(target);

            if (target is INotifyPropertyChanged notifier)
            {
                this.propertyObserver.Observe(notifier, this.memberNames[i]);
            }
        }
    }
}