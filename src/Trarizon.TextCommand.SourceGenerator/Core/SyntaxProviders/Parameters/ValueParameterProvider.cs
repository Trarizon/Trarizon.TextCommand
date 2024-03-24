using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters.Markers;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;
internal sealed class ValueParameterProvider : IInputParameterProvider, IRequiredParameterProvider, IPositionalParameterProvider
{
    private readonly InputParameterProviderComponent _helper;

    public ValueParameterData Data { get; }

    public ExecutorProvider Executor { get; }

    IParameterData IParameterProvider.Data => Data;
    IInputParameterData IInputParameterProvider.Data => Data;
    IRequiredParameterData IRequiredParameterProvider.Data => Data;
    IPositionalParameterData IPositionalParameterProvider.Data => Data;

    public ValueParameterProvider(ValueParameterData data, ExecutorProvider executor)
    {
        Data = data;
        Executor = executor;
        _helper = new(this);
    }

    public IEnumerable<StatementSyntax> CaseBodyLocalStatements()
    {
        yield return _helper.StdLocalVarDeclaration(
            Literals.ArgsProvider_GetValue_MethodIdentifier,
            new[] {
                SyntaxProvider.LiteralInt32Expression(Data.IsUnreachable ? int.MaxValue : Data.StartIndex),
                _helper.ParserArgExprSyntax,
            }.Select(SyntaxFactory.Argument));

        if (!Data.IsUnreachable) {
            yield return _helper.StdErrorHandingStatement(Data.IsRequired);
        }
    }

    public ArgumentSyntax ExecutorArgAccess_ArgumentSyntax()
        => SyntaxFactory.Argument(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(_helper.L_ExecutorArgument_VarIdentifier()),
                SyntaxFactory.IdentifierName(Literals.ArgResult_Value_PropertyIdentifier)));
}
