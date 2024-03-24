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
internal sealed class OptionParameterProvider : IInputParameterProvider, INamedParameterProvider, IRequiredParameterProvider
{
    private readonly InputParameterProviderComponent _helper;

    public OptionParameterData Data { get; }

    public ExecutorProvider Executor { get; }

    IInputParameterData IInputParameterProvider.Data => Data;
    IParameterData IParameterProvider.Data => Data;
    INamedParameterData INamedParameterProvider.Data => Data;
    IRequiredParameterData IRequiredParameterProvider.Data => Data;

    public OptionParameterProvider(OptionParameterData data,ExecutorProvider executor)
    {
        Data = data;
        Executor = executor;
        _helper = new(this);
    }

    public IEnumerable<StatementSyntax> CaseBodyLocalStatements()
    {
        yield return _helper.StdLocalVarDeclaration(
            Literals.ArgsProvider_GetOption_MethodIdentifier,
            new[] {
                SyntaxProvider.LiteralStringExpression(Data.Name),
                _helper.ParserArgExprSyntax,
            }.Select(SyntaxFactory.Argument));

        yield return _helper.StdErrorHandingStatement(Data.IsRequired);
    }

    public ArgumentSyntax ExecutorArgAccess_ArgumentSyntax()
        => SyntaxFactory.Argument(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(_helper.L_ExecutorArgument_VarIdentifier()),
                SyntaxFactory.IdentifierName(Literals.ArgResult_Value_PropertyIdentifier)));

    public ExpressionSyntax ParsingContextArgDictValueExpr()
        => SyntaxProvider.LiteralInt32Expression(1);
}
