# Phork.Blazor.Reactivity

_Phork.Blazor.Reactivity_ is a Blazor state management library. It helps take advantage of .NET's `INotifyPropertyChanged` and `INotifyCollectionChanged` interfaces to automatically manage the state of your components.

By using this library:

* You can use reactive one-way and two-way (in combination with `@bind` directive) bindings that can make the component re-render if any `INotifyPropertyChanged` instance in the binding path raises `PropertyChanged` event.
* You can use nested properties in the binding path.
* You can additionally make your components react to `CollectionChanged` notifications of `INotifyCollectionChanged` interface.
* You can optionally use converters with bindings if the binding source and target have different types and/or additional logic is required in your bindings.
* You don't need to worry about memory leaks and unnecessary re-renders as the library will take care of unsubscribing the events as soon as they get out of the render-tree.

This is the official documentation of the library.

If you prefer to learn with examples and want to see the motivation behind the concepts of this library, the following document can help you:

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

In order to enable your components to take advantage of the library you should make them inherit from `ReactiveComponentBase`.

Insert the following line at the start of the Razor file of your component:

```csharp
@inherits ReactiveComponentBase
```

> **Note:** If your component does not have a Razor file, simply make it inherit from `Phork.Blazor.ReactiveComponentBase` instead of the default `ComponentBase`.

[](ignored) <!-- To get rid of MD028/no-blanks-blockquote -->

> **Note:** If your component has a direct base type other than the default `ComponentBase`, you can still take advantage of the library. All you need to do is to implement `IReactiveComponent` in your component. [This](.) document will guide you through the steps.

### Use reactivity

#### Observed Values

When you intend to use a property of an object that implements `INotifyPropertyChanged` in your Razor file and at the same time make your component re-render when the property changes, you can use _observed values_.

Assuming `Person` is a parameter of the component and implements `INotifyPropertyChanged`, instead of doing:

```html
Name: @Person.Name
```

You can do the following in a reactive component:

```html
Name: @Observed(() => Person.Name)
```

This way not only the `Observed` method returns the value of `Person.Name` but also it subscribes to `PropertyChanged` event of `Person` and makes the component re-render (through calling `StateHasChanged`) whenever it receives a change notification regarding the `Name` property of the `Person` object. (By checking `e.PropertyName == "Name"` in its `PropertyChanged` handler).

You can nest properties and let the _observed value_ observe the changes to any of the intermediate properties to make your component re-render:

```html
Dog Name: @Observed(() => Person.Dog.Name)
```

This way if the person changes its dog object or the name of its existing dog (assuming the dog class implements `INotifyPropertyChanged`) the component will re-render automatically.

Since the `Observed` method returns the value of the expression, it can be mixed with your code seamlessly:

```html
Dog Age Estimate: @(DateTime.Now.Year - Observed(() => Person.Dog.Birthday).Year)

-----

@if (IsPalindrome(Observed(() => Person.Name)))
{
    <text>Congrats!</text>
}

-----

@if (IsPalindrome(Observed(() => Person.Name)))
{
    var dog = Observed(() => Person.Dog);
    if(IsPalindrome(Observed(() => dog.Name)))
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

#### Observed Collections

You can use _observed collections_ if you need your component to react to `CollectionChanged` events of values implementing `INotifyCollectionChanged`. _Observed collections_ **do** react to property changes that happen in the property path leading to the collection in the same way as _observed values_ do.

```html
@foreach(var skill in ObservedCollection(() => Person.Skills))
{
    <text>@Observed(skill.Name)</text>
}
```

By doing so, the `ObservedCollection` method will return the value of `Person.Skills` and will re-render the component when `Skills` property of `Person` object gets changes. In addition, if the `Skills` collection implements `INotifyCollectionChanged`, each `CollectionChanged` event it fires will cause the component to re-render.

#### Observed Bindings

While _observed values_ are enough for one-way binding scenarios, they cannot be used with the `@bind` syntax of Blazor. You can use _observed bindings_ to create reactive two-way bindings:

```html
<input type="text" @bind="Binding(() => Person.Name).Value" />
```

Here, not only any external change (outside of the Blazor events) to `Person.Name` will make the component re-render to refresh the text-box, but also if the user changes the text inside the text-box, `Person.Name` will get changed accordingly.

Since unlike _observed values_, _observed bindings_ **do not return the value of the expression directly**, they cannot be mixed with your logic as easily as _observed values_ could. Converters can be used if you want a conversion occur between the source and the target value:

```html
<input type="text" @bind="Binding(() => Person.Age, AddFive, SubtractFive).Value" @bind:event="oninput" />

@code {
    // Person parameter

    private int AddFive(int number)
    {
        return number + 5;
    }

    private int SubtractFive(int number)
    {
        return number - 5;
    }
}
```

This way if the value of `Person.Age` (the source value) gets changed externally, the text-box will show the new value plus five (as instructed by the `AddFive` converter method) and if the user edits the number inside the text-box (the target value), `Person.Age` will become the value inside the text-box minus five (as instructed by the `SubtractFive` reverse-converter method).

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

There is another overload of `Binding` method that accepts a `Phork.Data.IValueConverter<TSource, TTarget>` object as the converter. This interface has two methods `TTarget Convert(TSource value)` and `TSource ConvertBack(TTarget value)` that will be used to convert values. The behavior of the bindings created by this overload is the same as the bindings that the two previous overloads create.
