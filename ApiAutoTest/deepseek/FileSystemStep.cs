// FileSystemStep.cs
using System.IO;
using System.Threading.Tasks;


namespace TestAutomationEngine.Core
{
    public class FileSystemStep : ComponentBase
    {
        public enum FileSystemAction { ReadText, WriteText, AppendText, Delete, ListFiles, CreateDirectory, DeleteDirectory }

        public FileSystemAction Action { get; set; }
        public string Path { get; set; } = string.Empty;
        public string? Content { get; set; }
        public string? TargetVariable { get; set; }

        public override string ComponentType => "FileSystem";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var resolvedPath = VariableResolver.Resolve(Path, context);
            var resolvedContent = Content != null ? VariableResolver.Resolve(Content, context) : null;

            switch (Action)
            {
                case FileSystemAction.ReadText:
                    var text = await File.ReadAllTextAsync(resolvedPath);
                    if (!string.IsNullOrEmpty(TargetVariable))
                        context.SetVariable(TargetVariable, text);
                    break;
                case FileSystemAction.WriteText:
                    await File.WriteAllTextAsync(resolvedPath, resolvedContent ?? string.Empty);
                    break;
                case FileSystemAction.AppendText:
                    await File.AppendAllTextAsync(resolvedPath, resolvedContent ?? string.Empty);
                    break;
                case FileSystemAction.Delete:
                    File.Delete(resolvedPath);
                    break;
                case FileSystemAction.ListFiles:
                    var files = Directory.GetFiles(resolvedPath);
                    if (!string.IsNullOrEmpty(TargetVariable))
                        context.SetVariable(TargetVariable, files);
                    break;
                case FileSystemAction.CreateDirectory:
                    Directory.CreateDirectory(resolvedPath);
                    break;
                case FileSystemAction.DeleteDirectory:
                    Directory.Delete(resolvedPath, true);
                    break;
            }
            return new ComponentResult { Component = this };
        }
    }
}