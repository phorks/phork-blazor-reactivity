namespace Phork.Blazor.Reactivity.Tests.Models;

public class FirstBindable : BindableBase
{
    private SecondBindable _second;

    public SecondBindable Second
    {
        get => this._second;
        set => this.SetProperty(ref this._second, value);
    }
}