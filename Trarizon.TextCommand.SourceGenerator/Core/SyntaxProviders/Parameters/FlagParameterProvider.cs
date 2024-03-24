using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
using Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters.Markers;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.SyntaxProviders.Parameters;
internal sealed class FlagParameterProvider : IInputParameterProvider, IFlagParameterProvider, INamedParameterProvider
{
    private readonly InputParameterProviderComponent _helper;

    public FlagParameterData Data { get; }

    public ExecutorProvider Executor { get; }

    IParameterData IParameterProvider.Data => Data;
    IInputParameterData IInputParameterProvider.Data => Data;
    INamedParameterData INamedParameterProvider.Data => Data;

    public FlagParameterProvider(FlagParameterData data, ExecutorProvider executor)
    {
        Data = data;
        Executor = executor;
        _helper = new(this);
    }


    public IEnumerable<StatementSyntax> CaseBodyLocalStatements()
    {
        return _helper.StdLocalVarDeclaration(
            Literals.ArgsProvider_GetFlag_MethodIdentifier,
            new[] {
                SyntaxProvider.LiteralStringExpression(Data.Name),
                _helper.ParserArgExprSyntax,
            }.Select(SyntaxFactory.Argument))
            .Collect();
    }

    public ArgumentSyntax ExecutorArgAccess_ArgumentSyntax()
        => SyntaxFactory.Argument(
            SyntaxFactory.IdentifierName(_helper.L_ExecutorArgument_VarIdentifier()));

    public ExpressionSyntax ParsingContextArgDictValueExpr()
        => SyntaxProvider.LiteralInt32Expression(0);
}
