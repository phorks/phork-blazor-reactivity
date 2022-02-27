namespace Phork.Blazor.Reactivity.Tests.Models;

public class RootBindable : BindableBase
{
    private InnerBindable? _inner;

    public InnerBindable? Inner
    {
        get => this._inner;
        set => this.SetProperty(ref this._inner, value);
    }
}