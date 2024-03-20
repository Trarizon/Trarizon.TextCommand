namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal interface INamedParameterData : IInputParameterData
{
    string? Alias { get; }
    string Name { get; }
}
