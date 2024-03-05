using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
internal interface IParameterDataProvider
{
    IParameterData Data { get; }
    ParameterProvider Parameter { get; }

    ProviderMethodInfoContext ProviderMethodInfo { get; }

    ExpressionSyntax GetResultValueAccessExpression();
}
