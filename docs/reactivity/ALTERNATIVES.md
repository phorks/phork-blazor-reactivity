# Phork.Blazor.Reactivity vs the Alternatives

Before deciding to write _Phork.Blazor.Reactivity_ I did some searching to find libraries that can bring `INotifyProeprtyChanged` and `INotifyCollectionChanged` support to Blazor. I managed to find two particular libraries.

The first one was [ReactiveUI.Blazor](https://github.com/reactiveui/ReactiveUI/tree/main/src/ReactiveUI.Blazor). The great _ReactiveUI_'s approach to add reactivity to Blazor is far from what I expected it to be. The fact that you can't make your component observe variables in the markup of your component is a dealbreaker. You have to replicate your markup logic once again in a handler to `ReactiveComponentBase<AppViewModel>.WhenActivated` to tell the library which variables it needs to watch in each render cycle. This was more than enough to make me ignore the library.

The second one was [MvvmBlazor](https://github.com/klemmchr/MvvmBlazor), its approach is what I had in my mind but after some testing I realized that it can't satisfy my expectations. In this document I will compare _Phork.Blazor.Reactivity_ to _MvvmBlazor_.

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

With _Phork.Blazor.Reactivity_ We can modify `ParentComponent` as follows to make it reactive:

```html
@inherits ReactiveComponentBase

<h3>Parent Component</h3>

Name: @Observed(() => Person.Name)
<br />
Dog Name: @Observed(() => Person.Dog.Name)

<ul>
    @foreach (var skill in Observed(() => Person.Skills))
    {
        <li @key="skill">Title: @Observed(() => skill.Title)</li>
    }
</ul>

<ReactivityChild Person="Person" />
```

## MvvmBlazor

You can't implement `INotifyPropertyChanged` directly in your models and use them in _MvvmBlazor_. In order for our models to be used with _MvvmBlazor_, we should make them inherit from `MvvmBlazor.ViewModel.ViewModelBase` (which implements `INotifyPropertyChanged`). This is not desirable as it makes our models dependent on `MvvmBlazor`. However, _MvvmBlazor_ is supposed to be used in _presentation layer_, using this in our _data layer_ makes it platform-dependent!

Nevertheless, if we change our models to inherit from `ViewModelBase`, we can write `ParentComponent` as follows to make it reactive:

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

The table below compares the reaction of `ParentComponent` in _Phork.Blazor.Reactivity_ and _MvvmBlazor_ to the operations performed by `ChildComponent`:

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
| Observing nested properties                                              | `Observed(() => item.Member.NestedMember)`                                                     | :x: Not supported - you can do `Bind(item.Member, x => x.NestedMember)`, but this way if item.Member gets changed it won't be observed                                                                       |
| Stop observing a property when it gets out of the render-tree            | Supported                                                                                      | :x: Not supported                                                                                                                                                                                            |
| Observing `INotifyCollectionChanged`                                     | `Observed(() => item.Collection)`                                                              | `Bind(item, () => item.Collection)`                                                                                                                                                                          |
| Binding child component's parameters one-way                             | `Parameter="Binding(() => item.Member).Value"`                                                 | `Parameter="Bind(item, x => x.Member)"` - however, if you do this and `item.Member` implements `INotifyCollectionChanged` the parent component will re-render every time `CollectionChanged` event is raised |
| Binding child component's parameters two-way                             | `@bind-Parameter="Binding(() => item.Parameter).Value"`                                        | :x: Not supported                                                                                                                                                                                            |
| Using inside a component that directly inherits from `ComponentBase`     | Make the component inherit from `ReactiveComponentBase`                                        | Make the component inherit from `MvvmComponentBase` or `MvvmComponentBase<T>`                                                                                                                                |
| Using inside a component that its direct ancestor is not `ComponentBase` | Implement `IReactiveComponent`                                                                 | :x: Not supported                                                                                                                                                                                            |
| Types that can be used to create reactivity                              | Any item in `() => item.Member.NestedMember` can optionally implement `INotifyPropertyChanged` | Root of the expression must inherit from `ViewModelBase`, nested members are ignored                                                                                                                         |
| Adding observed values in code                                           | Use `ConfigureBindings()` method                                                               | :x: Not supported                                                                                                                                                                                            |
