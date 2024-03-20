using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
/// <param name="index">if &lt 0, means this value is after RestValues, thus always empty</param>
internal class ValueDataProvider(ValueParameterData data, InputParameterProvider parameter) : IInputParameterDataProvider, IRequiredParameterDataProvider, IValueDataProvider
{
    public ValueParameterData Data { get; } = data;

    public InputParameterProvider Parameter { get; } = parameter;

    IRequiredParameterData IRequiredParameterDataProvider.Data => Data;
    IInputParameterData IInputParameterDataProvider.Data => Data;
    IValueParameterData IValueDataProvider.Data => Data;
    IParameterData IParameterDataProvider.Data => Data;

    IParameterProvider IParameterDataProvider.Parameter => Parameter;

    public ProviderMethodInfoContext ProviderMethodInfo => new(
        Literals.ArgsProvider_GetValue_MethodIdentifier,
        [
            SyntaxProvider.LiteralInt32Expression(Data.IsUnreachable ? int.MaxValue : Data.Index),
            Parameter.ParserArgExpressionSyntax
        ]);


    public ExpressionSyntax ResultValueAccessExpression()
        => SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName(Parameter.Argument_VarIdentifier()),
            SyntaxFactory.IdentifierName(Literals.ArgResult_Value_PropertyIdentifier));
}
