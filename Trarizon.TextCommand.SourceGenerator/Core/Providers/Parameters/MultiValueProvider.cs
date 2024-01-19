using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.Providers.Parameters;
internal sealed class MultiValueProvider(ExecutorProvider executor, MultiValueParameterModel model, int index) : ParameterProvider(executor)
{
    protected override ICLParameterModel Model => model;

    protected override (string Identifier, ArgumentSyntax[] Arguments) GetProviderMethodInfo()
    {
        string literal;
        ExpressionSyntax expression;
        var throwFlagExpression = SyntaxFactory.Argument(
            model.Required
            ? SyntaxProvider.LiteralStringExpression($"{Literals.Prefix}{model.Parameter.Symbol.Name}")
            : SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));


        if (model.IsRest) {
            switch (model.CollectionType) {
                case MultiValueCollectionType.ReadOnlySpan:
                case MultiValueCollectionType.Span:
                case MultiValueCollectionType.Array:
                case MultiValueCollectionType.Enumerable:
                    literal = Literals.ArgsProvider_GetRestValues_MethodIdentifier;
                    break;
                case MultiValueCollectionType.List:
                    literal = Literals.ArgsProvider_GetRestValuesList_MethodIdentifier;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return (literal, [
                SyntaxFactory.Argument(SyntaxProvider.LiteralInt32Expression(index)),
                ParserArgumentSyntax,
                throwFlagExpression,
            ]);
        }
        else {
            switch (model.CollectionType) {
                case MultiValueCollectionType.ReadOnlySpan:
                case MultiValueCollectionType.Span:
                    var array = SyntaxFactory.ArrayType(
                        SyntaxFactory.IdentifierName(Model.ParsedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                        SyntaxFactory.SingletonList(
                            SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxProvider.LiteralInt32Expression(model.MaxCount)))));
                    literal = Literals.ArgsProvider_GetValues_MethodIdentifier;
                    expression = Model.ParsedTypeSymbol.IsUnmanagedType && model.MaxCount <= Literals.StackAllocThreshold
                        ? SyntaxFactory.StackAllocArrayCreationExpression(array)
                        : SyntaxFactory.ArrayCreationExpression(array);
                    break;
                case MultiValueCollectionType.Array:
                case MultiValueCollectionType.Enumerable:
                    literal = Literals.ArgsProvider_GetValuesArray_MethodIdentifier;
                    expression = SyntaxProvider.LiteralInt32Expression(model.MaxCount);
                    break;
                case MultiValueCollectionType.List:
                    literal = Literals.ArgsProvider_GetValuesList_MethodIdentifier;
                    expression = SyntaxProvider.LiteralInt32Expression(model.MaxCount);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return (literal, [
                SyntaxFactory.Argument(SyntaxProvider.LiteralInt32Expression(index)),
                SyntaxFactory.Argument(expression),
                ParserArgumentSyntax,
                throwFlagExpression,
            ]);
        }
    }
}
