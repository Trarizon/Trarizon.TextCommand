using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.ParameterDatas;
/// <param name="index">if &lt 0, means this value is after RestValues, thus always empty</param>
internal class ValueDataProvider(ValueParameterData data, ParameterProvider parameter) : IParameterDataProvider, IRequiredParameterDataProvider, IValueDataProvider
{
    public ValueParameterData Data { get; } = data;

    public ParameterProvider Parameter { get; } = parameter;

    IRequiredParameterData IRequiredParameterDataProvider.Data => Data;
    IParameterData IParameterDataProvider.Data => Data;
    IValueParameterData IValueDataProvider.Data => Data;

    public ProviderMethodInfoContext ProviderMethodInfo => new(
        Literals.ArgsProvider_GetValue_MethodIdentifier,
        [
            SyntaxProvider.LiteralInt32Expression(Data.IsUnreachable ? int.MaxValue : Data.Index),
            Parameter.ParserArgExpressionSyntax
        ]);

    public ExpressionSyntax GetResultValueAccessExpression()
        => SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName(Parameter.Argument_VarIdentifier()),
            SyntaxFactory.IdentifierName(Literals.ArgResult_Value_PropertyIdentifier));
}
