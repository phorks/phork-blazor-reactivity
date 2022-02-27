using System.Collections.ObjectModel;

namespace Phork.Blazor.Reactivity.Tests.Models;

public class InnerBindable : BindableBase
{
    private string? _stringValue;

    public string? StringValue
    {
        get => this._stringValue;
        set => this.SetProperty(ref this._stringValue, value);
    }

    private int _numberValue;

    public int NumberValue
    {
        get => this._numberValue;
        set => this.SetProperty(ref this._numberValue, value);
    }

    public ObservableCollection<string>? _collection = new();

    public ObservableCollection<string>? Collection
    {
        get => this._collection;
        set => this.SetProperty(ref this._collection, value);
    }
}