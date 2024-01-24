using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
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

    private TypeSyntax? _parsedTypeSyntax;
    protected TypeSyntax ParsedTypeSyntax
        => _parsedTypeSyntax ??= SyntaxFactory.IdentifierName(Model.ParsedTypeSymbol.ToDisplayString(SymbolDisplayFormats.FullQualifiedFormatIncludeNullableRefTypeModifier));

    [MemberNotNull(nameof(_parserTypeSyntax), nameof(_parserArgSyntax))]
    private void InitParserSyntaxes()
    {
        // Implicit 
        if (Model.ParserInfo.TryGetLeft(out var prmKind, out var parser)) {
            _parserTypeSyntax = SyntaxHelper.GetDefaultParserType(Model.ParsedTypeSymbol, prmKind);
            _parserArgSyntax = SyntaxProvider.DefaultArgument(ParserTypeSyntax);
        }
        // Parser field or property
        else if (parser.TryGetLeft(out var memberParser, out var methodParser)) {
            var (type, member) = memberParser;
            _parserTypeSyntax = SyntaxFactory.IdentifierName(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            _parserArgSyntax = SyntaxFactory.Argument(
                SyntaxProvider.SiblingMemberAccessExpression(member));
        }
        // parsing method
        else {
            var identifier = Model.Parameter.ParameterKind == CLParameterKind.Flag
                ? $"{Constants.Global}::{Literals.DelegateFlagParser_TypeName}"
                : $"{Constants.Global}::{Literals.DelegateParser_TypeName}";
            _parserTypeSyntax =
                SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier(identifier),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            ParsedTypeSyntax)));
            _parserArgSyntax =
                SyntaxFactory.Argument(
                    SyntaxFactory.ObjectCreationExpression(
                        _parserTypeSyntax,
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxProvider.SiblingMemberAccessExpression(methodParser)))),
                    default));
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
                                ParsedTypeSyntax,
                                ParserTypeSyntax,
                            })))),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(args))));
    }

    protected abstract (string Identifier, ArgumentSyntax[] Arguments) GetProviderMethodInfo();
}
