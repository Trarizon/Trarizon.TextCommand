using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal static class SyntaxHelper
{
    public static TypeSyntax GetNonWrappedDefaultParserTypeSyntax(IInputParameterData parameterData)
    {
        switch (parameterData.ParserInfo.ImplicitParameterKind) {
            case ImplicitParameterKind.Boolean:
                return SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.BooleanFlagParser_TypeName}");
            case ImplicitParameterKind.SpanParsable:
                return SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier($"{Constants.Global}::{Literals.ParsableParser_TypeName}"),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                            SyntaxFactory.IdentifierName(parameterData.ParsedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))));
            case ImplicitParameterKind.Enum:
                return SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier($"{Constants.Global}::{Literals.EnumParser_TypeName}"),
                    SyntaxFactory.TypeArgumentList(
                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                            SyntaxFactory.IdentifierName(parameterData.ParsedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))));
            default:
                throw new InvalidOperationException();
        }
    }

    public static (TypeSyntax, ExpressionSyntax) WrapParserTypeSyntax(TypeSyntax innerParserSyntax, ExpressionSyntax argExpressionSyntax, IInputParameterData parameterData)
    {
        var kind = parameterData.GetParserWrapperKind();

        if (kind is ParserWrapperKind.None)
            return (innerParserSyntax, argExpressionSyntax);

        ITypeSymbol? midType = null;

        if (kind.HasFlag(ParserWrapperKind.ImplicitConversion)) {
            midType = parameterData.ResultTypeSymbol.RemoveNullableAnnotation();
            innerParserSyntax = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier($"{Constants.Global}::{Literals.ConversionParser_TypeName}"),
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(new TypeSyntax[] {
                        SyntaxFactory.IdentifierName(parameterData.ParsedTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                        SyntaxFactory.IdentifierName(midType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                        innerParserSyntax,
                    })));
            argExpressionSyntax = SyntaxFactory.ObjectCreationExpression(
                innerParserSyntax,
                SyntaxProvider.ArgumentList(
                    argExpressionSyntax,
                    SyntaxFactory.SimpleLambdaExpression(
                        SyntaxFactory.Parameter(
                            SyntaxFactory.Identifier("x")),
                        block: null,
                        SyntaxFactory.IdentifierName("x"))),
                initializer: null);
        }

        if (kind.HasFlag(ParserWrapperKind.Nullable)) {
            innerParserSyntax = SyntaxFactory.GenericName(
                SyntaxFactory.Identifier($"{Constants.Global}::{Literals.NullableParser_TypeName}"),
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(new[] {
                        SyntaxFactory.IdentifierName((midType ?? parameterData.ParsedTypeSymbol).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                        innerParserSyntax
                    })));
            if (argExpressionSyntax is DefaultExpressionSyntax) {
                argExpressionSyntax = SyntaxFactory.DefaultExpression(innerParserSyntax);
            }
            else {
                argExpressionSyntax = SyntaxFactory.ObjectCreationExpression(
                    innerParserSyntax,
                    SyntaxProvider.ArgumentList(
                        argExpressionSyntax),
                    initializer: null);
            }
        }

        return (innerParserSyntax, argExpressionSyntax);
    }
}
