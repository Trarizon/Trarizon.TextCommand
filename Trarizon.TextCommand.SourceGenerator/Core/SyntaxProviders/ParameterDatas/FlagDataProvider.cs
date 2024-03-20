using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
internal sealed class FlagDataProvider(FlagParameterData data, InputParameterProvider parameter) : IInputParameterDataProvider, INamedParameterDataProvider
{
    public FlagParameterData Data { get; } = data;

    public InputParameterProvider Parameter { get; } = parameter;

    IInputParameterData IInputParameterDataProvider.Data => Data;
    INamedParameterData INamedParameterDataProvider.Data => Data;
    IParameterData IParameterDataProvider.Data => Data;

    IParameterProvider IParameterDataProvider.Parameter => Parameter;

    public ProviderMethodInfoContext ProviderMethodInfo => new(
        Literals.ArgsProvider_GetFlag_MethodIdentifier,
        [
            SyntaxProvider.LiteralStringExpression(Data.Name),
            Parameter.ParserArgExpressionSyntax,
        ]);


    public ExpressionSyntax GetParameterSetDictValue()
        => SyntaxProvider.LiteralInt32Expression(0);

    public ExpressionSyntax ResultValueAccessExpression()
        => SyntaxFactory.IdentifierName(Parameter. Argument_VarIdentifier());
}
