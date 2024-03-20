namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal interface IValueParameterData : IInputParameterData
{
    int MaxCount { get; }
    int Index { get; set; }
    bool IsRest { get; }
    bool IsUnreachable { get; }
}
