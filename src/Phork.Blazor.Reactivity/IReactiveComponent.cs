namespace Phork.Blazor
{
    public interface IReactiveComponent
    {
        void ConfigureBindings();
        void StateHasChanged();
    }
}
