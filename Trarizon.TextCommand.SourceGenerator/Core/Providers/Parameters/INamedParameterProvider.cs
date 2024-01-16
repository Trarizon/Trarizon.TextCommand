namespace Trarizon.TextCommand.SourceGenerator.Core.Providers.Parameters;
internal interface INamedParameterProvider
{
    string? Alias { get; }
    string Name { get; }
}
