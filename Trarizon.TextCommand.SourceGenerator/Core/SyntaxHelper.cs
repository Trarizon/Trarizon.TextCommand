﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal static class SyntaxHelper
{
    public static TypeSyntax GetDefaultParserType(ITypeSymbol type, ImplicitParameterKind parameterKind)
    {
        switch (parameterKind) {
            case ImplicitParameterKind.Boolean:
                return SyntaxFactory.IdentifierName($"{Constants.Global}::{Literals.BooleanFlagParser_TypeName}");
            case ImplicitParameterKind.SpanParsable:
                return ParsableParserTypeSyntax(
                    SyntaxFactory.IdentifierName(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
            case ImplicitParameterKind.Enum:
                return EnumParserTypeSyntax(
                    SyntaxFactory.IdentifierName(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
            case ImplicitParameterKind.NullableSpanParsable:
                var underlyingType = ((INamedTypeSymbol)type).TypeArguments[0];
                var parsed = SyntaxFactory.IdentifierName(underlyingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                var parser = ParsableParserTypeSyntax(parsed);
                return NullableParserTypeSyntax(parsed, parser);
            case ImplicitParameterKind.NullableEnum:
                underlyingType = ((INamedTypeSymbol)type).TypeArguments[0];
                parsed = SyntaxFactory.IdentifierName(underlyingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                parser = EnumParserTypeSyntax(parsed);
                return NullableParserTypeSyntax(parsed, parser);
            default:
                throw new InvalidOperationException();
        }

        static TypeSyntax EnumParserTypeSyntax(TypeSyntax parsedType)
            => SyntaxFactory.GenericName(
                SyntaxFactory.Identifier($"{Constants.Global}::{Literals.EnumParser_TypeName}"),
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(parsedType)));

        static TypeSyntax NullableParserTypeSyntax(TypeSyntax parsedType, TypeSyntax parserType)
            => SyntaxFactory.GenericName(
                SyntaxFactory.Identifier($"{Constants.Global}::{Literals.NullableParser_TypeName}"),
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList(new[] { parsedType, parserType })));

        static TypeSyntax ParsableParserTypeSyntax(TypeSyntax parsedType)
            => SyntaxFactory.GenericName(
                SyntaxFactory.Identifier($"{Constants.Global}::{Literals.ParsableParser_TypeName}"),
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(parsedType)));
    }
}
