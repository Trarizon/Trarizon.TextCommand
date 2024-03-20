using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
internal interface IRequiredParameterDataProvider : IInputParameterDataProvider
{
    new IRequiredParameterData Data { get; }
}
