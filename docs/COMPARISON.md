# Phork.Blazor.Reactivity vs the Alternatives

This document demonstrates how Phork.Blazor.Reactivity is different from the alternative libraries.

There are also other documents that you may find useful:

* [Getting Started](../README.md): Will guide you through the steps required to setup the library and make your components reactive.
* [Phork.Blazor.Reactivity in Action](./IN-ACTION.md): If you are new to these interfaces and/or you want to see the motivation behind the concepts of this library.

Before deciding to write Phork.Blazor.Reactivity I did some searching to find libraries that can bring `INotifyPropertyChanged` and `INotifyCollectionChanged` support to Blazor. I managed to find two particular libraries.

The first one was [ReactiveUI.Blazor](https://github.com/reactiveui/ReactiveUI/tree/main/src/ReactiveUI.Blazor). The great ReactiveUI's approach to add reactivity to Blazor is far from what I expected it to be. The fact that you can't make your component observe variables in the markup of your component is a deal-breaker. You have to replicate your markup logic once again in a handler to `ReactiveComponentBase<AppViewModel>.WhenActivated` to tell the library which variables it needs to watch in each render cycle. This was more than enough to make me ignore the library.

The second one was [MvvmBlazor](https://github.com/klemmchr/MvvmBlazor). It is a great library that helps you achieve MVVM in you Blazor applications. In addition, it comes with some functionalities that help you make your components reactive by taking advantage of `INotifyPropertyChanged` and `INotifyCollectionChanged`. The way it lets you do this is somehow similar to the approach I had in mind before writing Phork.Blazor.Reactivity. However, in terms of providing reactivity, this library falls short of my expectations. In this document we will see how Phork.Blazor.Reactivity compares to the reactivity solution offered by MvvmBlazor.

For the sake of demonstration we will define an problem and try to use each library to tackle it.

We will use the following models in the example.

```text
Person
{
    string Name,
    Dog Dog,
    ObservableCollection<Skill> Skills
}

Skill
{
    string Title
}

Dog
{
    string Name
}
```

Note that for the sake of simplicity, I didn't write the actual C# code for the models. Each model implements `INotifyPropertyChanged` and raises `PropertyChanged` in the setter of its properties.

Our `ParentComponent` code should look like this:

```html
<h3>Parent Component</h3>

Name: @(Person.Name)
<br />
Dog Name: @(Person.Dog.Name)

<ul>
    @foreach (var skill in Person.Skills)
    {
        <li @key="skill">Title: @(skill.Title)</li>
    }
</ul>

<ChildComponent Person="Person">

@code {
    [Parameter] public Person Person { get; set; }
}
```

In `ChildComponent` we are not going to use any library. It simply does some modifications to its `Person` parameter. We will use each library in `ParentComponent` to make it able to react to the modifications. The table below shows `ChildComponent`'s operations and the reaction we expect to see in `ParentComponent`:

| #   | Operation                                                          | Expected Reaction in `ParentComponent`                                     |
| --- | ------------------------------------------------------------------ | -------------------------------------------------------------------------- |
| 1   | Change `Person.Name`                                               | Re-render                                                                  |
| 2   | Add new skill to `Person.Skills`                                   | Re-render                                                                  |
| 3   | Change `Title` of `Person.Skills[0]` (if any)                      | Re-render                                                                  |
| 4   | Remove the last skill and store it in a field (if any)             | Re-render                                                                  |
| 5   | If a skill is stored in `Operation #4` change its `Title`          | Nothing - the removed skill is out of the render-tree of `ParentComponent` |
| 6   | Change `Person.Dog.Name`                                           | Re-render                                                                  |
| 7   | Store `Person.Dog` in a field and change `Person.Dog` to a new dog | Re-render                                                                  |
| 8   | Change `Name` of the stored dog in `Operation #7`                  | Nothing - the old dog is out of the render-tree of `ParentComponent`       |

## Phork.Blazor.Reactivity

With Phork.Blazor.Reactivity We can modify `ParentComponent` as follows to make it reactive:

```html
@inherits ReactiveComponentBase

<h3>Parent Component</h3>

Name: @Observed(() => Person.Name)
<br />
Dog Name: @Observed(() => Person.Dog.Name)

<ul>
    @foreach (var skill in ObservedCollection(() => Person.Skills))
    {
        <li @key="skill">Title: @Observed(() => skill.Title)</li>
    }
</ul>

<ChildComponent Person="Person" />
```

## MvvmBlazor (v2.0.0)

You can't implement `INotifyPropertyChanged` directly in your models and use them in MvvmBlazor. In order for your models to be used with MvvmBlazor, you should make them inherit from `MvvmBlazor.ViewModel.ViewModelBase` (which implements `INotifyPropertyChanged`). This is not desirable as it makes your models dependent on MvvmBlazor. MvvmBlazor is supposed to be used in the _presentation layer_, using this in your _data layer_ makes it platform-dependent!

Nevertheless, if you change your models to inherit from `ViewModelBase`, you can write `ParentComponent` as follows to make it reactive:

```html
@inherits MvvmComponentBase

<h3>Parent Component</h3>

Name: @Bind(Person, x => x.Name)
<br />
Dog Name: @Bind(Person.Dog, x => x.Name)

<ul>
    @foreach (var skill in Bind(Person, x => x.Skills))
    {
        <li @key="skill">Title: @Bind(skill, x => x.Title)</li>
    }
</ul>

<ChildComponent Person="Person" />
```

## ChildComponent Operation Results

You can use the demo application to see the results: [Phork.Blazor.Reactivity](https://phorks.github.io/phork-blazor-reactivity/reactivity-demo/comparison/phork-blazor-reactivity), [MvvmBlazor](https://phorks.github.io/phork-blazor-reactivity/reactivity-demo/comparison/mvvmblazor).

The table below compares the reaction of `ParentComponent` in Phork.Blazor.Reactivity and MvvmBlazor to the operations performed by `ChildComponent`:

| #   | Operation                                                          | Expected Reaction | Phork.Blazor.Reactivity      | MvvmBlazor                   |
| --- | ------------------------------------------------------------------ | ----------------- | ---------------------------- | ---------------------------- |
| 1   | Change `Person.Name`                                               | Re-render         | :heavy_check_mark: Re-render | :heavy_check_mark: Re-render |
| 2   | Add new skill to `Person.Skills`                                   | Re-render         | :heavy_check_mark: Re-render | :heavy_check_mark: Re-render |
| 3   | Change `Title` of `Person.Skills[0]` (if any)                      | Re-render         | :heavy_check_mark: Re-render | :heavy_check_mark: Re-render |
| 4   | Remove the last skill and store it in a field (if any)             | Re-render         | :heavy_check_mark: Re-render | :heavy_check_mark: Re-render |
| 5   | If a skill is stored in `Operation #4` change its `Title`          | Nothing           | :heavy_check_mark: Nothing   | :x: Re-render                |
| 6   | Change `Person.Dog.Name`                                           | Re-render         | :heavy_check_mark: Re-render | :heavy_check_mark: Re-render |
| 7   | Store `Person.Dog` in a field and change `Person.Dog` to a new dog | Re-render         | :heavy_check_mark: Re-render | :x: Nothing                  |
| 8   | Change `Name` of the stored dog in `Operation #7`                  | Nothing           | :heavy_check_mark: Nothing   | :x: Re-render                |

## Conclusion

In conclusion, you can use the table below to see the differences between the two libraries.

| Title                                                                    | Phork.Reactivity.Blazor                                                                        | MvvmBlazor                                                                                                                                                                                                   |
| ------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Observing first level properties                                         | `Observed(() => item.Member)`                                                                  | `Bind(item, x => x.Member)`                                                                                                                                                                                  |
| Observing nested properties                                              | `Observed(() => item.Member.NestedMember)`                                                     | :x: Not supported - you can do `Bind(item.Member, x => x.NestedMember)`, but this way, if `item.Member` gets changed it won't be observed                                                                     |
| Stop observing a property when it gets out of the render-tree            | Supported                                                                                      | :x: Not supported                                                                                                                                                                                            |
| Observing `INotifyCollectionChanged`                                     | `ObservedCollection(() => item.Collection)`                                                              | `Bind(item, () => item.Collection)`                                                                                                                                                                          |
| Binding child component's parameters one-way                             | `Parameter="Observed(() => item.Member)"`                                                 | `Parameter="Bind(item, x => x.Member)"` - however, if you do this and `item.Member` implements `INotifyCollectionChanged` the parent component will re-render every time `CollectionChanged` event is raised |
| Binding child component's parameters two-way                             | `@bind-Parameter="Binding(() => item.Member).Value"`                                        | :x: Not supported                                                                                                                                                                                            |
| Using inside a component that directly inherits from `ComponentBase`     | Make the component inherit from `ReactiveComponentBase`                                        | Make the component inherit from `MvvmComponentBase` or `MvvmComponentBase<T>`                                                                                                                                |
| Using inside a component that its direct ancestor is not `ComponentBase` | Implement `IReactiveComponent`                                                                 | :x: Not supported                                                                                                                                                                                            |
| Types that can be used to create reactivity                              | Any item in `() => item.Member.NestedMember` can optionally implement `INotifyPropertyChanged` | Root of the expression must inherit from `ViewModelBase`, nested members are ignored                                                                                                                         |
| Defining reactivity in code behind                                        |  Define them inside the `ConfigureBindings()` method                                                               | :x: Not supported - you can use the `Bind` method anywhere in the code behind of the component but doing so will observe the property until the end of the component's life no matter what                                                                                                                                                                                          |
