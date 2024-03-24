namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
internal interface INamedParameterData : IInputParameterData
{
    string? Alias { get; }
    string Name { get; }
}
