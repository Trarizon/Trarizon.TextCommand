using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
internal sealed class FlagDataProvider(FlagParameterData data, ParameterProvider parameter) : IParameterDataProvider, INamedParameterDataProvider
{
    public FlagParameterData Data { get; } = data;

    public ParameterProvider Parameter { get; } = parameter;

    INamedParameterData INamedParameterDataProvider.Data => Data;
    IParameterData IParameterDataProvider.Data => Data;

    public ProviderMethodInfoContext ProviderMethodInfo => new(
        Literals.ArgsProvider_GetFlag_MethodIdentifier,
        [
            SyntaxProvider.LiteralStringExpression(Data.Name),
            Parameter.ParserArgExpressionSyntax,
        ]);

    public ExpressionSyntax GetParameterSetDictValue()
        => SyntaxProvider.LiteralInt32Expression(0);

    public ExpressionSyntax GetResultValueAccessExpression()
        => SyntaxFactory.IdentifierName(Parameter.Argument_VarIdentifier());
}
