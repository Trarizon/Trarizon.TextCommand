using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities;


namespace Trarizon.TextCommand.SourceGenerator.Core.Providers;
internal abstract class ParameterProvider
{
    public ExecutorProvider Executor { get; }

    protected abstract ICLParameterModel Model { get; }

    public string Argument_VarIdentifier => $"__arg_{Model.Parameter.Symbol.Name}_{Executor.Model.Symbol.Name}";

    protected ParameterProvider(ExecutorProvider executor)
    {
        Executor = executor;
    }

    private TypeSyntax? _parserTypeSyntax;
    protected TypeSyntax ParserTypeSyntax
    {
        get {
            if (_parserTypeSyntax is null) {
                InitParserSyntaxes();
            }
            return _parserTypeSyntax;
        }
    }

    private ArgumentSyntax? _parserArgSyntax;
    protected ArgumentSyntax ParserArgumentSyntax
    {
        get {
            if (_parserArgSyntax is null) {
                InitParserSyntaxes();
            }
            return _parserArgSyntax;
        }
    }


    [MemberNotNull(nameof(_parserTypeSyntax), nameof(_parserArgSyntax))]
    private void InitParserSyntaxes()
    {
        if (Model.ParserInfo.TryGetLeft(out var prmKind)) {
            _parserTypeSyntax = SyntaxHelper.GetDefaultParserType(Model.ParsedTypeSymbol, prmKind);
            _parserArgSyntax = SyntaxProvider.DefaultArgument(ParserTypeSyntax);
        }
        else {
            var (type, member) = Model.ParserInfo.Right;
            _parserTypeSyntax = SyntaxFactory.IdentifierName(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            _parserArgSyntax = SyntaxFactory.Argument(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    member.IsStatic
                        ? SyntaxFactory.IdentifierName(Executor.Execution.Command.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                        : SyntaxFactory.ThisExpression(),
                    SyntaxFactory.IdentifierName(member.Name)));
        }
    }

    public LocalDeclarationStatementSyntax ArgumentLocalDeclaration()
    {
        var (identifier, args) = GetProviderMethodInfo();

        return SyntaxProvider.LocalVarSingleVariableDeclaration(
            Argument_VarIdentifier,
            SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(
                        Executor.ArgsProvider_VarIdentifer),
                    SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier(identifier),
                        SyntaxFactory.TypeArgumentList(
                            SyntaxFactory.SeparatedList(new[] {
                                Model.ParsedTypeSyntax,
                                ParserTypeSyntax,
                            })))),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(args))));
    }

    protected abstract (string Identifier, ArgumentSyntax[] Arguments) GetProviderMethodInfo();
}
