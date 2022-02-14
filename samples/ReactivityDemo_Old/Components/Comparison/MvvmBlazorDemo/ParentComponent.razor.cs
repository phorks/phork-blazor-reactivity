using Microsoft.AspNetCore.Components;
using ReactivityDemo.Models.Comparison.MvvmBlazorModels;
using System.Text;

namespace ReactivityDemo.Components.Comparison.MvvmBlazorDemo
{
    public partial class ParentComponent
    {
        private string log => this.logBuilder.ToString();
        private readonly StringBuilder logBuilder = new StringBuilder();

        [Parameter] public Person Person { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            this.logBuilder.AppendLine("> ParentComponent rendered");
        }
    }
}
