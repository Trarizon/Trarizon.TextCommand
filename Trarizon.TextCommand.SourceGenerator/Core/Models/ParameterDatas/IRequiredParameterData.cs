namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal interface IRequiredParameterData : IParameterData
{
    bool Required { get; }
}
