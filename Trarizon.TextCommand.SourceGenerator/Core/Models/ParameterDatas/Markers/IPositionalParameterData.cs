namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
internal interface IPositionalParameterData : IInputParameterData
{
    int MaxCount { get; }
    int StartIndex { get; }
    bool IsRest { get; }
    bool IsUnreachable { get; }
}

internal interface IPositionalParameterDataMutable : IPositionalParameterData
{
    new int StartIndex { set; }
}
