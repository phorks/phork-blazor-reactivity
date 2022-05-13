# Detailed Information of Phorks.Blazor.Reactivity

This document contains some detailed information about the concepts of the library.

There are also other documents that you may find useful:

* [Getting Started](../README.md): Will guide you through the steps required to setup the library and use reactivity in your components.
* [Phork.Blazor.Reactivity in Action](./IN-ACTION.md): If you are new to `INotifyPropertyChanged` and `INotifyCollectionChanged` interfaces and/or you want to see the motivation behind the concepts of this library.
* [Phork.Blazor.Reactivity vs the Alternatives](./COMPARISON.md): If you want to see how Phork.Blazor.Reactivity is different from the alternative libraries.

## Table of Contents

* [Implementing IReactiveComponent](#implementing-ireactivecomponent)
* [Value Accessor](#value-accessor)
* [Observed Values](#observed-values)
* [Observed Collection](#observed-collections)
* [Observed Bindings](#observed-bindings)

## Implementing IReactiveComponent

If your component has a direct base type other than the default `ComponentBase` and you are not able to make the direct base type inherit from `ReactiveComponentBase` you can still use reactivity in your component by implementing `IReactiveComponent`. (Provided that `ComponentBase` is still an indirect base type of the component.)

Modify _YourComponent.razor.cs_ this way (if there is no cs file you can still add these functionalities in the Razor file):

```csharp
using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Phork.Blazor;
using Phork.Blazor.Bindings;
// your usings...

public partial class YourComponent : NonReactiveComponentBase, IReactiveComponent, IDisposable
{
    [Inject]
    protected IReactivityManager ReactivityManager { get; private set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        ReactivityManager.Initialize(this);

        // your OnInitialized logic (if any)...
    }

    // your code...

    public void Dispose() // Optionally implement dispose pattern
    {
        // your Dispose logic (if any)...

        ReactivityManager.Dispose();
        GC.SuppressFinalize(this);
    }

    protected virtual void ConfigureBindings()
    {
    }

    /// <inheritdoc cref="IReactivityManager.Observed{T}(Expression{Func{T}})"/>
    protected T Observed<T>(Expression<Func<T>> valueAccessor)
    {
        return ReactivityManager.Observed(valueAccessor);
    }

    /// <inheritdoc cref="IReactivityManager.ObservedCollection{T}(Expression{Func{T}})"/>
    protected T ObservedCollection<T>(Expression<Func<T>> valueAccessor)
    {
        return ReactivityManager.ObservedCollection(valueAccessor);
    }

    /// <inheritdoc cref="IReactivityManager.Binding{T}(Expression{Func{T}})" />
    protected IObservedBinding<T> Binding<T>(Expression<Func<T>> valueAccessor)
    {
        return ReactivityManager.Binding(valueAccessor);
    }

    /// <inheritdoc cref="IReactivityManager.Binding{TSource, TTarget}(Expression{Func{TSource}}, Func{TSource, TTarget}, Func{TTarget, TSource})"/>
    protected IObservedBinding<TTarget> Binding<TSource, TTarget>(
        Expression<Func<TSource>> valueAccessor,
        Func<TSource, TTarget> converter,
        Func<TTarget, TSource> reverseConverter)
    {
        return ReactivityManager.Binding(valueAccessor, converter, reverseConverter);
    }

    void IReactiveComponent.StateHasChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    void IReactiveComponent.ConfigureBindings()
    {
        ConfigureBindings();
    }
}
```

## Value Accessor

A value accessor of type `T` is essentially an `Expression<Func<T>>` conforming to some restrictions. `Expression<Func<T>>` type forces the expression to be a lambda expression returning `T`. However not all lambda expressions with the return type of `T` are valid value accessors. In order for a lambda expression to be a valid value accessor, the body of the lambda expression has to either be a variable access expression (an expression like `() => item` where `item` is a variable) or a chain of object member access expressions. In other words only expressions like `variable` and `root.member1.member2.⋯.member{n}`
 are valid where in the first case `variable` is a variable and in the second case `root` has to be a variable and `member1` is a member (property of field) of `root` and for each i > 1, `member{i}` is a member of `member{i-1}`. If the expression is not a valid value accessor, using it as a value accessor argument throws an `ArgumentException`.

## Observed Values

An observed value can be created by calling `Observed<T>(Expression<Func<T>>)` method on a reactive component.

`Observed` method accepts only one parameter of type `Expression<Func<T>>`. This expression has to be a [value accessor](#value-accessor).

### Returned Value of Observed Values

When you use a `Observed` method to create an observed value with `() => Path.To.Property` value accessor, the returned value of the method will be the value of `Path.To.Property`.

### Behavior of Observed Values

When an observed value is created with `() => root.member1.member2.⋯.member{n}` value accessor, the library will scan the body of the lambda expression. If `root` implements `INotifyPropertyChanged`, its `PropertyChanged` event will be subscribed to. If the event gets raised and `e.PropertyName` equals `member1` the `ReactivityManager` will call its reactive component's `IReactiveComponent.StateHasChanged` method. For each i < n, the same thing will happen to `item{i}` in the body of the value accessor except the condition that will trigger `IReactiveComponent.StateHasChanged` will be `e.PropertyName` being equal to `item{i+1}`.

> **Note**: Creation of observed values requires dynamic compiling of lambda expressions, and doing so may turn expensive, so the library does a good job in caching created observed values while getting rid of unnecessary ones as soon as possible to avoid redundant `StateHasChanged` calls and potential memory leaks. A reactive component calls `ReactivityManager`'s `Notify` method after each rendering and this helps `ReactivityManager` clean up the observed values that were not used in that render cycle (e.g. an observed value that is inside the body of an if statement that has a false condition based on the current state of the component will not make the component re-render when it gets changed because `ReactivityManager` will consider this observed value inactive and will get rid of it).

### Use Cases of Observed Values

Since the `Observed` method, when used with `() => Path.To.Property` value accessor, directly returns the value of `Path.To.Property`, you can use `Observed(() => Path.To.Property)` anywhere inside your Razor file that using `Path.To.Property` is valid, with two exceptions.

1. When the returned value is supposed to be a collection and there is a chance for the collection to implement `INotifyCollectionChanged` and if so you want your component to re-render in case it notifies any changes via its `CollectionChanged` event. In this case you need to use [Observed Collections](#observed-collections).
2. When you intend to use `Path.To.Property` as the left-hand side of an assignment and at the same time making the component react to its property changes (this is what happens under the hood when you bind a parameter two-way using the `@bind` directive). In this case use [Observed Bindings](#observed-bindings) instead.

Example:

```html
Dog Age Estimate: @(DateTime.Now.Year - Observed(() => Person.Dog.Birthday).Year)

-----

<DogComponent Dog="Observed(() => Person.Dog)">

-----

@if (IsPalindrome(Observed(() => Person.Name)))
{
    <text>Congrats!</text>
}

-----

@if (IsPalindrome(Observed(() => Person.Name)))
{
    var dog = Observed(() => Person.Dog);
    if(IsPalindrome(Observed(() => dog.Name))) // or IsPalindrome(Observed(() => Person.Dog.Name))
    {
        <text>Nested Congrats!</text>
    }
}

----- Code -----

@code {
    // Person parameter

    private string IsPalindrome(string text)
    {
        // using System.Linq
        return text.ToLower().SequenceEqual(text.ToLower().Reverse());
    }
}
```

## Observed Collections

An observed collection can be created by calling `ObservedCollection<T>(Expression<Func<T>>)` method on a reactive component.

`ObservedCollection<T>` method accepts only one parameter of type `Expression<Func<T>>`. This expression has to be a [value accessor](#value-accessor).

### Returned Value of Observed Collections

When you use a `ObservedCollection` method to create an observed collection with `() => Path.To.Collection` value accessor, the returned value of the method will be the value of `Path.To.Collection`.

### Behavior of Observed Collections

Observed collections have the exact [behavior](#behavior-of-observed-values) of observed values in that they will make the component re-render whenever they detect a property change in the value accessor. On top of that, they are aware of `INotifyCollectionChanged` interface. If the returned value by the value accessor implements `INotifyCollectionChanged` the observed collection will make the component re-render every time the collection raises a `CollectionChanged` event handler.

### Use Cases of Observed Collection

Observed collections can be used when the returned value of a value accessor is supposed to implement `INotifyCollectionChanged` and you want your component re-render whenever it publishes its `CollectionChanged` event in addition to the `PropertyChanged` event notifications raised by the objects present in the path of the value accessor leading to the collection. This happens most of the time in `foreach` statements.

Example:

```html
@foreach(var skill in ObservedCollection(() => Person.Skills))
{
    <text>@Observed(skill.Name)</text>
}
```

## Observed Bindings

To be able to use observed values with `@bind` directive in your components you can use observed bindings.

> **Why not observed values?**
>
> If you try to use observed values with the `@bind` directive like this:
>
> ```html
> <ChildComponent @bind-Name="Observed(() => Person.Name)">
> ```
>
> The Razor code generator will try to add `__value => Observed(() => Person.Name) = __value` lambda as a handler to `NameChanged` event callback of the child component. Which will cause a `Compiler Error CS0131` since the left-hand side of the assignment (`Observed(() => Person.Name)`) is of course not assignable!

There are two types of observed bindings, direct bindings and converted bindings.

Observed bindings can be created by the overloads of `Binding` method on a reactive component. There are two overloads. One for direct bindings and one for converted bindings. Both of the overloads accept a [value accessor](#value-accessor) as the first parameter.

### Returned Value of Observed Bindings

Both overloads of `Binding` method, when used with `() => Path.To.Property` value accessor, return an `IObservedBinding<T>` instance where `T` will be the type of the target parameter. `IObservedBinding<T>` has a `Value` property of type `T` that is both settable and gettable.

Regardless of which overload you use to create the observed binding, you need to use `Binding(...).Value` as the value of the target parameter.

Example:

```html
<ChildComponent @bind-TargetParameter="Binding(...).Value">
```

### Observed Binding Methods

#### 1. Direct Observed Binding

A direct binding can be used when the target parameter and the value represented by the value accessor share the same type, and no extra conversion logic is required. You can use the following overload of `Binding` method to create a direct binding:

```csharp
IObservedBinding<T> Binding<T>(
    Expression<Func<T>> valueAccessor)
```

Example:

```html
<ChildComponent @bind-SomeStringParameter="Binding(() => Path.To.StringProperty).Value">
```

> **Note**: Once again note that we used `Binding(...).Value` as the value of the parameter not `Binding(...)`.

#### 2. Converted Observed Binding

If the target parameter has a different type than the type of the value represented by the value accessor and/or you need to apply custom conversion logic, you need to use converted bindings. The following overload of `Binding` method creates a converted binding:

```csharp
IObservedBinding<TTarget> Binding<TSource, TTarget>(
    Expression<Func<TSource>> valueAccessor,
    Func<TSource, TTarget> converter,
    Func<TTarget, TSource> reverseConverter)
```

Here:

* `TSource` is the type of value represented by the `valueAcessor`.
* `TTarget` is the type of the target parameter.
* `converter` is a `Func<TSource, TTarget>` delegate that will be used to convert the value of type `TSource` represented by the value accessor to a value of type `TTarget` that must be assigned to the target parameter.
* `reverseConverter` is a `Func<TTarget, TSource>` delegate that will be used to convert the values provided by `{TargetParameter}Changed` event to a value that can be assigned to the property represented by the value accessor.

Example:

```html
<ChildComponent @bind-SomeIntParameter="Binding(() => Path.To.String.Property, ConverterMethod, ReverseConverterMethod).Value">

@code {
    int ConverterMethod(string value)
    {
        // your conversion logic...
    }

    string ReverseConverterMethod(int value)
    {
        // your conversion logic...
    }
}
```

> :warning: **Warning:** The `converter` and the `reverseConverter` delegates must be the inverse functions of each other for the binding logic to work.

### Behavior of Observed Bindings

An observed binding shares [the same behavior](#behavior-of-observed-values) as observed values in that it will make the component re-render whenever it detects a property change in the value accessor.

If we create a binding like this (using any of the overloads of the `Binding` method):

```html
<ChildComponent @bind-SomeParameter="Binding(() => Path.To.Property, ...).Value">
```

When the `ChildComponent` raises `SomeParameterChanged` event, the new value will be set to `Path.To.Property`. Obviously if `Path.To.Property` is not settable (e.g., it represents a property without a setter or a `readonly` field) you will receive an `InvalidOperationException`.

> **Note:** Observed bindings are state-less. It means that when converters are used in a binding, every time the component renders, the `converter` will be called to convert the source value, even if the source value has not been changed since the last render. And every time the target value gets changed, the `reverseConverter` will be used to convert the target value, even if the target value is the converted value of the current source value.
