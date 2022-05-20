# Phork.Blazor.Reactivity

Phork.Blazor.Reactivity is an unopinionated Blazor state management library that utilizes [INotifyPropertyChanged](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.inotifypropertychanged?view=net-6.0) and [INotifyCollectionChanged](https://docs.microsoft.com/en-us/dotnet/api/system.collections.specialized.inotifycollectionchanged?view=net-6.0) .NET interfaces to automatically detect state changes in components.

Advantages of the library:

* You can use reactive one-way and two-way (in combination with the `@bind` directive) bindings that can make the component re-render if any `INotifyPropertyChanged` instance in the binding path raises `PropertyChanged` event with the appropriate property name.
* You can make your components react to `CollectionChanged` notifications of objects implementing  `INotifyCollectionChanged` interface.
* You can optionally use converters with two-way bindings if the binding source and target have different types and/or additional logic is required in your bindings.
* You don't need to worry about memory leaks and unnecessary re-renders as the library will take care of unsubscribing the events as soon as they get out of the render-tree.
* The library is unopinionated:

  * Usage of no MVx pattern is assumed, although it can greatly simplify implementing MVVM.
  * Even though the library provides a base class for reactive components, components not inheriting from the provided base class can still take advantage of the reactivity system.

If you are already familiar with these two interfaces, this document will guide you through the steps required to setup the library and make your components automatically react to the notifications published through the interfaces.

There are also other documents that you may find useful:

* [Phork.Blazor.Reactivity in Action](./docs/IN-ACTION.md): If you are new to these interfaces and/or you want to see the motivation behind the concepts of this library.
* [Phork.Blazor.Reactivity vs the Alternatives](./docs/COMPARISON.md): If you want to see how Phork.Blazor.Reactivity is different from the alternative libraries.

## Table of Contents

* [Phork.Blazor.Reactivity](#phorkblazorreactivity)
  * [Table of Contents](#table-of-contents)
  * [Getting Started](#getting-started)
    * [Install the NuGet Package](#install-the-nuget-package)
    * [Register Services](#register-services)
    * [Add the Namespace](#add-the-namespace)
    * [Make Your Component Reactive](#make-your-component-reactive)
    * [Use Reactivity](#use-reactivity)
      * [Observed Values](#observed-values)
      * [Observed Collections](#observed-collections)
      * [Observed Bindings](#observed-bindings)
      * [Configure Reactivity in Code Behind](#configure-reactivity-in-code-behind)
  * [Limitations and Considerations](#limitations-and-considerations)

## Getting Started

### Install the NuGet Package

Install the [NuGet package](https://www.nuget.org/packages/Phork.Blazor.Reactivity) or use the package manager console:

```powershell
Install-Package Phork.Blazor.Reactivity
```

### Register Services

Call `AddPhorkBlazorReactivity()` extension method on the  `IServiceCollection` of your host builder. This is usually done in _Program.cs_ located at the root of your Blazor project:

```csharp
using Phork.Blazor;

// ...

var builder = ...;

// ...

builder.Services.AddPhorkBlazorReactivity();
```

### Add the Namespace

Add the following line to _Imports.razor_ file located at the root of your Blazor project:

```csharp
namespace Phork.Blazor
```

### Make Your Component Reactive

In order to enable your components to take advantage of the library you must make them inherit from `ReactiveComponentBase`.

Insert the following line at the start of the Razor file of your component:

```csharp
@inherits ReactiveComponentBase
```

> **Note:** If your component does not have a Razor file, simply make it inherit from `Phork.Blazor.ReactiveComponentBase` instead of the default `ComponentBase`.

[](ignored) <!-- To get rid of MD028/no-blanks-blockquote -->

> **Note:** If your component has a direct base type other than the default `ComponentBase`, you can still take advantage of the library (as long as `ComponentBase` is still an indirect base type of the component). All you need to do, is to implement `IReactiveComponent` in your component as instructed [here](./docs/DETAILS.md#implementing-ireactivecomponent).

### Use Reactivity

#### Observed Values

When you intend to use a property of an object that implements `INotifyPropertyChanged` in your Razor file and at the same time make your component re-render when the property changes, you can use observed values.

Assuming `Person` is a parameter of the component and implements `INotifyPropertyChanged`, instead of doing:

```html
Name: @Person.Name
```

You can do the following in a reactive component:

```html
Name: @Observed(() => Person.Name)
```

This way, not only the `Observed` method returns the value of `Person.Name` but also it subscribes to `PropertyChanged` event of `Person` and makes the component re-render (through calling `StateHasChanged`) whenever it receives a change notification regarding the `Name` property of the `Person` object. (By checking `e.PropertyName == "Name"` in its `PropertyChanged` handler).

You can nest properties and let the observed value observe the changes to any of the intermediate properties to make your component re-render:

```html
Dog Name: @Observed(() => Person.Dog.Name)
```

This way, if the person changes its dog object or the name of its existing dog (assuming the dog class implements `INotifyPropertyChanged`) the component will re-render automatically.

Since the `Observed` method returns the value of the expression, it can be mixed with your code seamlessly:

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

Read more:

* [Detailed information of observed values](./docs/DETAILS.md#observed-values)

#### Observed Collections

You can use observed collections if you need your component to react to `CollectionChanged` events of values implementing `INotifyCollectionChanged`. Observed collections **do** react to property changes that happen in the property path leading to the collection in the same way as observed values do.

```html
@foreach(var skill in ObservedCollection(() => Person.Skills))
{
    <text>@Observed(() => skill.Name)</text>
}
```

By doing so, the `ObservedCollection` method will return the value of `Person.Skills` and will re-render the component when `Skills` property of `Person` object gets changes. In addition, if the `Skills` collection implements `INotifyCollectionChanged`, each `CollectionChanged` event it fires will cause the component to re-render.

Read more:

* [Detailed information of observed collections](./docs/DETAILS.md#observed-collections)

#### Observed Bindings

While observed values are enough for one-way binding scenarios, they cannot be used with the `@bind` syntax of Blazor. You can use observed bindings to create reactive two-way bindings:

```html
<input type="text" @bind="Binding(() => Person.Name).Value" />
```

Here, not only any external change (outside of the Blazor events) to `Person.Name` will make the component re-render to refresh the text-box, but also if the user changes the text inside the text-box, `Person.Name` will get changed accordingly.

Since unlike observed values, observed bindings **do not return the value of the expression directly**, they cannot be mixed with your custom logic as easily as observed values could. Converters can be used if you want a conversion to occur between the source and the target value:

```html
<input type="date" @bind-value="Binding(() => Person.Birthday, ConvertToDateTime, ConvertToDateOnly).Value" />

@code {
    // Person parameter (Person.Birthday is DateOnly)

    private DateTime ConvertToDateTime(DateOnly date)
    {
        return date.ToDateTime(TimeOnly.MinValue);
    }

    private DateOnly ConvertToDateOnly(DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }
}
```

The default `@bind-value` on date input in Blazor currently does not work with `DateOnly` struct. By using converters this way we can bind `DateOnly` values to date inputs two-way.

Or:

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

Here, if the value of `Person.Age` (the source value) gets changed externally, the text-box will show the new value plus five (as instructed by the `AddFive` converter method) and if the user edits the number inside the text-box (the target value), `Person.Age` will become the value inside the text-box minus five (as instructed by the `SubtractFive` reverse-converter method).

Read more:

* [Detailed information of observed bindings](./docs/DETAILS.md#observed-bindings)

#### Configure Reactivity in Code Behind

There might be situations where you need to create reactive bindings (observed values, observed collections, and rarely observed bindings) in code behind of your component, since the library has its own mechanisms to clean up inactive reactive bindings, it only expects reactive bindings to be created/consumed in a Razor file, or inside the `ConfigureBindings` method of your component.

The library will call `ConfigureBindings` method after the component renders and before the beginning of the clean-up process.

In a `ReactiveComponentBase` component:

```csharp
public partial class YourComponent : ReactiveComponentBase
{
    ...
    protected override void ConfigureBindings()
    {
        base.ConfigureBindings();

        Observed(() => Path.To.Property);
        ObservedCollection(() => Path.To.Collection);
    }
}
```

## Limitations and Considerations

Here is a list of known limitations and considerations of the library:

1. The `Observed`, the `ObservedCollection`, and the `Binding` methods accept an `Expression<Func<T>>` as the `valueAccessor`. Only a subset of all `Expression<Func<T>>`s are valid value accessors as explained [here](./docs/DETAILS.md#value-accessor).
2. Although the `Observed`, the `ObservedCollection`, and the `Binding` methods are generic methods, you **MUST NOT** explicitly specify their generic type parameters (e.g., as a means of up-casting the returned value). Doing so will cease the library's ability to reuse reactivity elements, and in some cases may lead to `InvalidOperationException`. Always let the `valueAccessor` (and converters in case of converted observed binding) define the generic type parameters.
3. To use converted observed bindings, the `converter` and the `reverseConverter` delegates need to be inverse functions of each other for the binding logic to work.
