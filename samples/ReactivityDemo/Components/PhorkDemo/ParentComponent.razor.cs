using System.Text;
using Microsoft.AspNetCore.Components;
using ReactivityDemo.Models.Comparison.PhorkModels;

namespace ReactivityDemo.Components.Comparison.PhorkDemo;

public partial class ParentComponent
{
    private string log => this.logBuilder.ToString();
    private readonly StringBuilder logBuilder = new();

    [Parameter] public Person Person { get; set; }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        this.logBuilder.AppendLine("> ParentComponent rendered");
    }
}