using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
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

        switch (model.CollectionType) {
            case MultiValueCollectionType.ReadOnlySpan:
            case MultiValueCollectionType.Span:
                // For conditions cannot create Span, use T[] instead;
                if (!Model.ParsedTypeSymbol.IsUnmanagedType || model.MaxCount > Literals.StackAllocThreshold) {
                    goto case MultiValueCollectionType.Array;
                }

                literal = Literals.ArgsProvider_GetValues_MethodIdentifier;
                // stackalloc T[provider.GetAvailableArrayLength(start, length)]
                expression = SyntaxFactory.StackAllocArrayCreationExpression(
                    SyntaxFactory.ArrayType(
                        SyntaxFactory.IdentifierName(Model.ParsedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
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
                                                    SyntaxProvider.LiteralInt32Expression(model.MaxCount)),
                                            }))))))));
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
            SyntaxFactory.Argument(SyntaxProvider.LiteralStringExpression(model.Parameter.Symbol.Name)),
            SyntaxFactory.Argument(SyntaxProvider.LiteralBooleanExpression(model.Required))
        ]);
    }
}
