namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
internal interface IRequiredParameterData : IInputParameterData
{
    bool IsRequired { get; }
}
