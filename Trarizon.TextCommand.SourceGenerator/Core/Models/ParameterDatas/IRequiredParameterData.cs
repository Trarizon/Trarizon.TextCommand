namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal interface IRequiredParameterData : IInputParameterData
{
    bool Required { get; }
}
