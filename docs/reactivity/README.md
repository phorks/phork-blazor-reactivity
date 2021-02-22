# Phork.Blazor.Reactivity

_Phork.Blazor.Reactivity_ is a Blazor state management library. It helps you take advantage of C#'s `INotifyPropertyChanged` and `INotifyCollectionChanged` interfaces to automatically manage the state of your components.

By using this library:

* You can use reactive one-way and two-way (in combination with `@bind` directive) bindings that can make the component re-render if any `INotifyPropertyChanged` instance in the binding path raises `PropertyChanged` event.
* You can use nested properties as the binding path.
* If the binding source in a one-way binding implements `INotifyCollectionChanged`, its `CollectionChanged` event will make the component re-render.
* You can optionally use converters with bindings if the binding source and target have different types and/or additional logic is required in your binding.
* You don't need to worry about memory leaks and unnecessary re-renders as the library will take care unsubscribing the events as soon as they get out of the render-tree.

This is the official documentation of the library.

If you prefer to learn with examples and want to see the motivation behind the concepts of this library the following document can help you:

* [Phork.Blazor.Reactivity in Action](./REACTIVITY-IN-ACTION.md)

If you want to see how _Phork.Blazor.Reactivity_ is different from the existing alternatives read the following document:

* [Phork.Blazor.Reactivity vs the Alternatives](./ALTERNATIVES.md)

## Table of Contents

* [Getting Started](#getting-started)
* [Observed Values](#observed-values)
* [Observed Bindings](#observed-bindings)

## Getting Started

### Install the NuGet Package

Install the [NuGet package](https://www.nuget.org/packages/Phork.Blazor.Reactivity) or use the package manager console:

```powershell
Install-Package Phork.Blazor.Reactivity
```

### Add the Namespace

Add the following line to __Imports.razor_ file at the root of your Blazor project:

```csharp
namespace Phork.Blazor
```

### Make Your Component Reactive

#### If the Component Is Derived Directly from `ComponentBase`

If you are going to use reactivity in a component that directly inherits from `ComponentBase` you must make the component inherit from `ReactiveComponentBase`.

In Razor, insert the following line at the start of you file:

```csharp
@inherits ReactiveComponentBase
```

If your component does not have a .razor file, you can simply make your component class inherit from `ReactiveComponentBase` in C#:

```csharp
public class YourComponent : Phork.Blazor.ReactiveComponentBase
```

#### If the Component Has a Different Direct Base Type

If your component has a direct base type other than ComponentBase and you are not able to make the base type inherit from `ReactiveComponentBase` you can still use reactivity in your component by implementing `IReactiveComponent`.

Modify _YourComponent.razor.cs_ this way (if there is no cs file you can still add these functionalities in the Razor file):

```csharp
public partial class YourComponent : NonReactiveComponentBase, IReactiveComponentBase, IDisposable
{
    protected ReactivityManager ReactivityManager { get; }

    public YourComponent()
    {
        this.reactivityManager = new ReactivityManager(this);
        // your constructor code...
    }

    // your code...

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        // your OnAfterRender logic (if any)...

        this.reactivityManager.CleanUp();
    }

    public virtual void Dispose()
    {
        this.reactivityManager.Dispose();

        // your Dispose logic (if any)...
    }

    void IReactiveComponent.StateHasChanged()
    {
        this.InvokeAsync(this.StateHasChanged());
    }

    void IReactiveComponent.ConfigureBindings()
    {
    }
}
```

This way you have to use `ReactivityManager.Observed(...)` and `ReactivityManager.Binding(...)` in your Razor file. If this annoys you, you can copy any of the public methods of `ReactivityManager` in your component class and direct the call with given arguments to the respective method in `ReactivityManager`.

## Observed Values

An _observed value_ can be created by calling `Observed<T>(Expression<Func<T>>)` method on a `ReactivityManager` or a component inheriting from `ReactiveComponentBase` -which directs the call to an internal `ReactivityManager`.

`Observed<T>` method has only one parameter of type `Expression<Func<T>>`. This expression has to be a _value accessor_ as defined below.

### Value Accessor

A _value accessor_ of type `T` is essentially an `Expression<Func<T>>` conforming to some restrictions. `Expression<Func<T>>` type forces the expression to be a lambda expression returning `T`. However not all lambda expressions with the return type of `T` are valid _value accessors_. In order for a lambda expression to be a valid _value accessor_, the body of the lambda expression has to either be a constant expression (an expression like `() => item` where item is a variable) or a chain of object member access expressions. In other words only expressions like `variable` and `root.member1.member2.⋯.member{n}`
 are valid where in the second case `member1` is a member (property of field) of `root` and for each i > 1, `member{i}` is a member of `member{i-1}`. If the expression is not a valid _value accessor_, using it as a _value accessor_ argument throws an `ArgumentException`.

### Returned Value of Observed Values

 When you use a `Observed` method to create an observed value with `() => Path.To.Property` _value accessor_, the returned value of the method will be the value of `Path.To.Property`.

### Behavior of Observed Values

 When an _observed value_ is created with a `() => root.member1.member2.⋯.member{n}` _value accessor_, the `ReactivityManager` will scan the body of the lambda expression. If `root` implements `INotifyPropertyChanged`, its `PropertyChanged` event will be subscribed to. If the event gets raised and `e.PropertyName` equals `member1` the `ReactivityManager` will call its reactive component's `IReactiveComponent.StateHasChanged` method. For each i < n, the same thing will happen to `item{i}` in the body of the _value accessor_ except the condition that will trigger `IReactiveComponent.StateHasChanged` will be `e.PropertyName` being equal to `item{i+1}`. In addition, if the value returned by `root.member1.member2.⋯.member{n}` implements `INotifyCollectionChanged`, its `CollectionChanged` event will automatically be subscribed to, and the `ReactivityManager`'s reactive component will receive a `IReactiveComponent.StateHasChanged` call each time the event is raised.

 Since under the hood, the creation of _observed values_ requires dynamic compiling of lambda expressions, and doing so may turn expensive, `ReactivityManager` does a good job in caching created _observed values_ while getting rid of unnecessary ones as soon as possible to avoid redundant `StateHasChanged` calls and potential memory leaks. A reactive component calls `ReactivityManager`'s `CleanUp` method after each rendering and this helps `ReactivityManager` clean up the _observed values_ that were not used in that rendering cycle (e.g. an _observed value_ that is inside the body of an if statement that has a false condition based on the current state of the component will not make the component re-render when it gets changed because `ReactivityManager` will consider this _observed value_ inactive and will clean it up).

### Use Cases

Since the `Observed` method, when used with a `() => Path.To.Property` _value accessor_, returns the value of `Path.To.Property`, you can use `Observed(() => Path.To.Property)` anywhere inside your razor file that using `Path.To.Property` is valid. However, when you intend to set a child component's parameter -either using a `@bind` directive or directly- [_observed bindings_](#observed-bindings) are preferred over _observed values_.

Example:

```html
@inherits ReactiveComponentBase

Name: @Observed(() => Person.Name)

@if(ShouldShowItems(Observed(() => Person.AccountType)))
{
    foreach(var item in Observed(() => Person.Items))
    {
        <div>@Observed(() => item.Name)</div>
    }
}

@code {
    Person person;

    bool ShouldShowItems(PersonAccountType accountType)
    {
        // your logic...
    }
}
```

### Usage in Code

There might be situations where you need to create _observed values_ in code, since `ReactivityManager` has its own mechanisms to clean up inactive _observed values_, it only expects _observed values_ to be created/consumed in a razor file, or inside the `ConfigureBindings` method of your component.

If your component inherits from `ReactiveComponentBase`, you have to override the protected `ConfigureBindings` method and if you have explicitly implemented `IReactiveComponent`, you can create additional _observed values_ inside your `IReactiveComponent.ConfigureBindings` implementation.

This method will be called after the component renders and before the `ReactivityManager`'s clean-up process.

In a `ReactiveComponentBase` component:

```csharp
public partial class YourComponent : ReactiveComponentBase
{
    ...
    protected override void ConfigureBindings()
    {
        base.ConfigureBindings();

        this.Observed(() => Path.To.Property);
    }
}
```

Or in a `IReactiveComponent` component:

```csharp
public partial class YourComponent : NonReactiveComponentBase, IReactiveComponent
{
    ...
    void IReactiveComponent.ConfigureBindings()
    {
        this.ReactivityManager.Observed(() => Path.To.Property);
    }
}
```

## Observed Bindings

Up to this point you know how using _observed values_ can help you make your component reactive. However, using _observed values_ is not desirable when you want to achieve reactivity while setting a child component's parameters. There are two reasons behind this:

1. If the value returned by the _observed value_ is an _INotifyCollectionChanged_, the change notifications of the collection will make the consuming component re-render. This is useful when you want to consume the collection directly in your component (like in a `foreach` statement). But when your component only needs to pass the collection to a child component through its parameter, it's the child component with the collection parameter's responsibility to decide if it needs to re-render when its collection parameter changes, not the parent's.

2. If you use _observed values_ with `@bind` syntax like this:

    ```html
    <ChildComponent @bind-Name="Observed(() => Name)">
    ```

    The razor code generator will try to add `__value => Observed(() => Name) = __value` lambda as a handler to `NameChanged` event callback of the child component. Which will cause a `Compiler Error CS0131` since the left-hand side of the assignment (`Observed(() => Name)`) is not assignable!

In both of these cases you have to use _observed bindings_.

In order to create an _observed binding_, you have to use one of the overloads of `Binding` method on a `ReactivityManager` or a component inheriting from `ReactiveComponentBase` -which directs the call to an internal `ReactivityManager`.

All overloads of `Binding` method need a _value accessor_ of type `Expression<Func<T>>` as the first argument. The _value accessor_ has to conform to the restrictions explained [here](#value-accessor).

### Returned Value of Observed Bindings

All overloads of `Binding` method, when used with `() => Path.To.Property` _value accessor_, return an `IObservedBinding<T>` instance where `T` will be the type of the target parameter. `IObservedBinding<T>` has a `Value` property of type `T` that is both settable and gettable.

Regardless of which overload you use to create the _observed binding_, you need to use `Binding(...).Value` as the value of the target parameter.

### Behavior of Observed Bindings

An _observed binding_ shares the same [behavior](./OBSERVED-VALUES.md#behavior) as _observed values_ in that it will make the component of `ReactivityManager` re-render (by calling `IReactiveComponent.StateHasChanged()`) whenever it detects a property change in the _value accessor_. But unlike _observed values_, `CollectionChanged` notifications will be ignored by _observed bindings_.

Each _observed binding_ is either one-way or two-way. The difference is only apparent when the binding is used to provide the value to a `@bind` directive.

Let's assume that we create a binding like this:

```html
<ChildComponent @bind-SomeParameter="Binding(() => Path.To.Property, ...).Value">
```

Now, when `ChildComponent` raises `SomeParameterChanged` event, if the binding is two-way the new value will be set to `Path.To.Property`. But if the binding is one-way, the new value will be ignored. Obviously if you create a two-way binding and `Path.To.Property` is not settable -for example it represents a property without a setter or a `readonly` field- you will receive an `InvalidOperationException`. In the following sections you will see how you can make your bindings one-way or two-way.

### Binding Methods

#### 1. Direct Binding

A direct binding can be used when the target parameter and the value represented by the _value accessor_ share the same type, and no extra conversion logic is required. You can use the following overload of `Binding` method to create a direct binding:

```csharp
IObservedBinding<T> Binding<T>(
    Expression<Func<T>> accessor,
    ObservedBindingMode mode = ObservedBindingMode.TwoWay)
```

You can pass `mode` argument to indicate the mode of the binding (one-way or two-way). The mode is two-way by default.

Example:

```html
@inherits ReactiveComponentBase
...
<ChildComponent SomeStringParameter="Binding(() => Path.To.String.Property).Value">
```

> **Note**: Once again note that we used `Binding(...).Value` as the value of the parameter not `Binding(...)`.

#### 2. Binding with Delegate Converters

If the target parameter has a different type than the type of the value represented by the _value accessor_, it makes no sense to use direct bindings. You need to use converters in these cases. You can use delegates as converters or you can use `IValueConverter<TSource, TTarget>`.
> **Note**: If converters are used in a binding, the converter will not be called every time the component renders. The binding will cache the value of the _value accessor_ and the converted value. When the component re-renders the binding will only use your converters when the value provided by the _value accessor_ is different than the cached one, otherwise the cached converted value will be used.

There are two overloads you can use to create bindings with delegate converters.

The following overload creates a one-way binding and needs a `Func<TSource, TTarget>` that will be used to convert the value of the _value accessor_ to a value of that can be assigned to the target parameter.

```csharp
IObservedBinding<TTarget> Binding<TSource, TTarget>(
    Expression<Func<TSource>> accessor,
    Func<TSource, TTarget> converter)
```

Using this overload as the value of a `@bind` directive does not make sense since the binding created by this method is always one-way.

Example:

```html
@inherits ReactiveComponentBase
...
<ChildComponent SomeIntParameter="Binding(() => Path.To.String.Property, ConverterMethod).Value">

@code {
    int ConverterMethod(string value)
    {
        // your conversion logic...
    }
}
```

There is another overload that can create a two-way binding that requires an additional converter delegate to be used to convert the values provided by `{Parameter}Changed` event to a value that can be assigned to the property represented by the _value accessor_:

```csharp
IObservedBinding<TTarget> Binding<TSource, TTarget>(
    Expression<Func<TSource>> accessor,
    Func<TSource, TTarget> converter,
    Func<TTarget, TSource> reverseConverter)
```

If you don't use the binding created by this overload as the value of a `@bind` directive, the binding will act exactly as if it was created by the previous overload (i.e. `reverseConverter` will be ignored).

Example:

```html
@inherits ReactiveComponentBase
...
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

> :warning: **Warning:** As the converter parameters in both overloads has `Func` type, it may seem reasonable to use lambda expressions as their values, however, doing so will disable the `ReactivityManager`'s ability to cache bindings. Always write your conversion logic in instance methods and pass those instance methods as converter arguments.

There is another overload to `Binding` method that accepts a `Phork.Data.IValueConverter<TSource, TTarget>` object as the converter. This interface has two methods `TTarget Convert(TSource value)` and `TSource ConvertBack(TTarget value)` that will be used to convert values. The behavior of the bindings created by this overload is the same as the bindings that the two previous overloads create.
