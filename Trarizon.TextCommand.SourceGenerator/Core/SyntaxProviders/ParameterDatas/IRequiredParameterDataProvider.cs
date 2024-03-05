using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
internal interface IRequiredParameterDataProvider : IParameterDataProvider
{
    new IRequiredParameterData Data { get; }
}
