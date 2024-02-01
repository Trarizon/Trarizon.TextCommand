using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.Providers.Parameters;
internal sealed class MultiValueProvider(ExecutorProvider executor, MultiValueParameterData data, int index) : ParameterProvider(executor)
{
    protected override ParameterModel Model => data.Model;

    protected override (string Identifier, ArgumentSyntax[] Arguments) GetProviderMethodInfo()
    {
        string literal;
        ExpressionSyntax expression;

        switch (data.CollectionType) {
            case MultiValueCollectionType.ReadOnlySpan:
            case MultiValueCollectionType.Span:
                // For conditions cannot create Span, use T[] instead;
                if (!data.ParsedTypeSymbol.IsUnmanagedType || data.MaxCount > Literals.StackAllocThreshold) {
                    goto case MultiValueCollectionType.Array;
                }

                literal = Literals.ArgsProvider_GetValues_MethodIdentifier;
                // stackalloc T[provider.GetAvailableArrayLength(start, length)]
                expression = SyntaxFactory.StackAllocArrayCreationExpression(
                    SyntaxFactory.ArrayType(
                        SyntaxFactory.IdentifierName(data.ParsedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                        SyntaxFactory.SingletonList(
                            SyntaxFactory.ArrayRankSpecifier(
                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                                    SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.IdentifierName(
                                                Executor.ArgsProvider_VarIdentifer),
                                            SyntaxFactory.IdentifierName(
                                                Literals.ArgsProvider_GetAvailableArrayLength_MethodIdentifier)),
                                        SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SeparatedList<ArgumentSyntax>(new[] {
                                                SyntaxFactory.Argument(
                                                    SyntaxProvider.LiteralInt32Expression(index)),
                                                SyntaxFactory.Argument(
                                                    SyntaxProvider.LiteralInt32Expression(data.MaxCount)),
                                            }))))))));
                break;
            case MultiValueCollectionType.Array:
            case MultiValueCollectionType.Enumerable:
                literal = Literals.ArgsProvider_GetValuesArray_MethodIdentifier;
                expression = SyntaxProvider.LiteralInt32Expression(data.MaxCount);
                break;
            case MultiValueCollectionType.List:
                literal = Literals.ArgsProvider_GetValuesList_MethodIdentifier;
                expression = SyntaxProvider.LiteralInt32Expression(data.MaxCount);
                break;
            default:
                throw new InvalidOperationException();
        }
        return (literal, [
            SyntaxFactory.Argument(SyntaxProvider.LiteralInt32Expression(index)),
            SyntaxFactory.Argument(expression),
            ParserArgumentSyntax,
            SyntaxFactory.Argument(SyntaxProvider.LiteralStringExpression(Model.Symbol.Name)),
            SyntaxFactory.Argument(SyntaxProvider.LiteralBooleanExpression(data.Required))
        ]);
    }
}
