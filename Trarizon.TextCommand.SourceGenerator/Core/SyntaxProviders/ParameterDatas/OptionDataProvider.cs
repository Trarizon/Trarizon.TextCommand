using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
internal sealed class OptionDataProvider(OptionParameterData data, InputParameterProvider parameter) : IInputParameterDataProvider, INamedParameterDataProvider,IRequiredParameterDataProvider
{
    public OptionParameterData Data { get; } = data;

    public InputParameterProvider Parameter { get; } = parameter;

    INamedParameterData INamedParameterDataProvider.Data => Data;
    IInputParameterData IInputParameterDataProvider.Data => Data;
    IRequiredParameterData IRequiredParameterDataProvider.Data => Data;
    IParameterData IParameterDataProvider.Data => Data;

    IParameterProvider IParameterDataProvider.Parameter => Parameter;

    public ProviderMethodInfoContext ProviderMethodInfo => new(
        Literals.ArgsProvider_GetOption_MethodIdentifier,
        [
            SyntaxProvider.LiteralStringExpression(Data.Name),
            Parameter.ParserArgExpressionSyntax
        ]);

    public ExpressionSyntax GetParameterSetDictValue()
        => SyntaxProvider.LiteralInt32Expression(1);

    public ExpressionSyntax ResultValueAccessExpression()
        => SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName(Parameter.Argument_VarIdentifier()),
            SyntaxFactory.IdentifierName(Literals.ArgResult_Value_PropertyIdentifier));
}
