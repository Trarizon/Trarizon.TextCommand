namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal interface INamedParameterData : IParameterData
{
    string? Alias { get; }
    string Name { get; }
}
