using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
internal static class SymbolExtensions
{
    public static bool MatchDisplayString(this ISymbol? symbol, string displayString, SymbolDisplayFormat? symbolDisplayFormat = null)
        => symbol?.ToDisplayString(symbolDisplayFormat) == displayString;

    public static bool MatchGenericType(this INamedTypeSymbol type, string displayStringWithoutGeneric, int typeParameterLength)
    {
        if (type.TypeParameters.Length != typeParameterLength)
            return false;

        if (type.MatchDisplayString(displayStringWithoutGeneric, SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric))
            return true;
        return false;
    }

    public static bool IsNullableValueType(this ITypeSymbol type, [NotNullWhen(true)] out ITypeSymbol? underlyingType)
    {
        if (type is {
            IsValueType: true,
            NullableAnnotation: NullableAnnotation.Annotated,
        }) {
            underlyingType = type.RemoveNullableAnnotation();
            return true;
        }
        underlyingType = default;
        return false;
    }

    public static ITypeSymbol RemoveNullableAnnotation(this ITypeSymbol type)
    {
        if (type.NullableAnnotation is not NullableAnnotation.Annotated)
            return type;
        if (type.IsValueType)
            return ((INamedTypeSymbol)type).TypeArguments[0]; // Value type cannot use .WithNullableAnnotation
        return type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
    }

    public static bool IsImplicitAssignableTo(this ITypeSymbol type, ITypeSymbol targetType, SemanticModel semanticModel, out bool nullableAnnotationConflict)
    {
        var conversion = semanticModel.Compilation.ClassifyCommonConversion(type, targetType);
        if (!conversion.IsImplicit) {
            nullableAnnotationConflict = default;
            return false;
        }

        // int = int
        // int? = int
        // A? = int; 
        // A = int;
        // A? = int?

        nullableAnnotationConflict = conversion.IsIdentity &&
            type.NullableAnnotation is NullableAnnotation.Annotated &&
            targetType.NullableAnnotation is NullableAnnotation.NotAnnotated;
        return true;
    }

    public static bool IsMayBeDefault(this ITypeSymbol type)
    {
        return type is not {
            IsValueType: false,
            NullableAnnotation: NullableAnnotation.NotAnnotated,
        };
    }

    public static bool IsRefKindCompatiblyPassTo(this IParameterSymbol fromParameter, IParameterSymbol toParameter)
    {
        return (fromParameter.RefKind, toParameter.RefKind) switch {
            (RefKind.In, RefKind.In or RefKind.None or RefKind.RefReadOnlyParameter) => true,
            (RefKind.None, RefKind.None or RefKind.Out or RefKind.Ref or RefKind.RefReadOnlyParameter or RefKind.In) => true,
            (RefKind.Out, RefKind.Out) => true,
            (RefKind.Ref, RefKind.Ref or RefKind.RefReadOnlyParameter or RefKind.In or RefKind.None or RefKind.Out) => true,
            (RefKind.RefReadOnlyParameter, RefKind.RefReadOnlyParameter or RefKind.In or RefKind.None) => true,
            _ => false,
        };
    }
}
