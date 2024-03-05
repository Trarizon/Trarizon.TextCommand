namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal interface INamedParameterData : IParameterData
{
    string? Alias { get; }
    string Name { get; }
}
