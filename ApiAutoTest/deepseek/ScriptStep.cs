// ScriptStep.cs
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace TestAutomationEngine.Core
{
    public class ScriptStep : ComponentBase
    {
        public string ScriptBody { get; set; } = string.Empty;

        public override string ComponentType => "Script";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var globals = new ScriptGlobals { Context = context };
            var script = CSharpScript.Create(ScriptBody, ScriptOptions.Default.WithImports("System"), typeof(ScriptGlobals));
            await script.RunAsync(globals, cancellationToken: context.CancellationToken);
            return new ComponentResult { Component = this };
        }
    }

    public class ScriptGlobals
    {
        public ExecutionContext Context { get; set; }
    }
}