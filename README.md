# Phork.Blazor.Reactivity

_Phork.Blazor.Reactivity_ is a Blazor state management framework named after the Vue.js Reactivity System. It helps you take advantage of C#'s `INotifyPropertyChanged` and `INotifyCollectionChanged` interfaces to automatically manage the state of your components.

Reactivity is how the application detects the changes in the state of a component that require re-rendering. According to [Microsoft docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/rendering?view=aspnetcore-5.0), Blazor components have their own reactivity conventions. A component re-renders if:

1. Its set of parameters is updated by the parent component.
2. A cascading value is updated.
3. An event on the component is raised or an external EventCallback that the component is subscribed to is invoked.
4. `StateHasChanged` method of the component is explicitly called.

In simple scenarios, the first 3 conventions are enough to handle state changes. But the need to explicitly calling `StateHasChanged` raises as the models and the relationships between components get complicated. _Phork.Blazor.Reactivity_ helps to reduce the need to `StateHasChanged`. It does so by introducing two new concepts, observed values and observed bindings, that take advantage of `INotifyPropertyChanged` and `INotifyCollectionChanged` interfaces.

## Observed Values
In this section we will define a scenario that the first 3 Blazor reactivity conventions are not enough to handle the situation, and then we will demonstrate how using _Observed Values_ can simplify our lives.

Let's assume that we have the following models:

```csharp
class Person
{
    public string Name { get; set;}
    public ICollection<Person> Skills { get; }
        = new List<Person>();
}

class PersonSkill
{
    public string Title { get; }
    public bool IsEnabled { get; set; }

    public PersonSkill(string title)
    {
        this.Title = title;
    }
}
```

We want to create a business card generator component that accepts a Person parameter. It has two responsibilities, it should let the user edit the person information and it should generate the business card as the user edits the information. Fortunately for us, we already have a fancy PersonEditor component that accepts a Person parameter and does the job of letting the user edit the information. So we only have to focus on the business card generation.

