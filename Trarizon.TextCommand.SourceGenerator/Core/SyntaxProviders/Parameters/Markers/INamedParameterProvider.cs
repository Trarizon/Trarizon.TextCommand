using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters.Markers;
internal interface INamedParameterProvider : IInputParameterProvider
{
    new INamedParameterData Data { get; }

    ExpressionSyntax ParsingContextArgDictValueExpr();
}
