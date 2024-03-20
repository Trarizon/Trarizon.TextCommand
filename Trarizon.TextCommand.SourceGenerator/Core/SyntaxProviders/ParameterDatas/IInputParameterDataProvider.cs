using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
internal interface IInputParameterDataProvider : IParameterDataProvider
{
    new IInputParameterData Data { get; }
    new InputParameterProvider Parameter { get; }

    ProviderMethodInfoContext ProviderMethodInfo { get; }
}