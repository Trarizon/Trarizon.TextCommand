using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal static class ValidationHelper
{
    public static InputParameterType ValidateInputParameterType(ITypeSymbol type)
    {
        if (type.SpecialType is SpecialType.System_String)
            return InputParameterType.String;

        else if (type is IArrayTypeSymbol {
            ElementType.SpecialType: SpecialType.System_String
        }) {
            return InputParameterType.Array;
        }

        else if (type is INamedTypeSymbol {
            TypeArguments: [{ SpecialType: SpecialType.System_String }]
        } namedType) {
            return namedType.ToDisplayString(SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric) switch {
                Constants.ReadOnlySpan_TypeName => InputParameterType.Span,
                Constants.Span_TypeName => InputParameterType.Span,
                Constants.List_TypeName => InputParameterType.List,
                _ => InputParameterType.Unknown,
            };
        }

        return InputParameterType.Unknown;
    }

    public static ImplicitParameterKind ValidateImplicitParameterKind(ITypeSymbol type)
    {
        // Boolean
        if (type.SpecialType is SpecialType.System_Boolean) {
            return ImplicitParameterKind.Boolean;
        }

        // Nullable mark
        bool isNullable = type.IsValueType && type.NullableAnnotation == NullableAnnotation.Annotated;
        type = isNullable
            ? ((INamedTypeSymbol)type).TypeArguments[0]
            : type;

        // ISpanParsable
        if (type.AllInterfaces.Any(static interfaceType
            => interfaceType.ToDisplayString(SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric) == Constants.ISpanParsable_TypeName
            && interfaceType.TypeArguments.Length == 1)
        ) {
            return isNullable
                ? ImplicitParameterKind.NullableSpanParsable
                : ImplicitParameterKind.SpanParsable;
        }

        // Enum
        if (type.TypeKind == TypeKind.Enum) {
            return isNullable
                ? ImplicitParameterKind.NullableEnum
                : ImplicitParameterKind.Enum;
        }

        // Invalid
        return ImplicitParameterKind.Invalid;
    }

    public static MultiValueCollectionType ValidateMultiValueCollectionType(ITypeSymbol type, out ITypeSymbol elementType)
    {
        if (type is IArrayTypeSymbol arrayType) {
            elementType = arrayType.ElementType;
            return MultiValueCollectionType.Array;
        }

        if (type is INamedTypeSymbol { TypeArguments: [var typeArg] } namedType) {
            elementType = typeArg;
            return namedType.ToDisplayString(SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric) switch {
                Constants.ReadOnlySpan_TypeName => MultiValueCollectionType.ReadOnlySpan,
                Constants.Span_TypeName => MultiValueCollectionType.Span,
                Constants.List_TypeName => MultiValueCollectionType.List,
                Constants.IEnumerable_TypeName => MultiValueCollectionType.Enumerable,
                _ => MultiValueCollectionType.Invalid
            };
        }

        elementType = null!;
        return MultiValueCollectionType.Invalid;
    }

    public static bool IsCustomParser(SemanticModel semanticModel, ITypeSymbol parserType, ITypeSymbol assignedType, bool isFlag,
        [NotNullWhen(true)] out ITypeSymbol? parsedType, out bool nullableClassTypeMayAssignToNotNullable)
    {
        var displayString = isFlag ? Literals.IArgFlagParser_TypeName : Literals.IArgParser_TypeName;

        foreach (var interfaceType in parserType.AllInterfaces) {
            if (interfaceType.ToDisplayString(SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric) == displayString &&
                interfaceType.TypeArguments is [var typeArg]
            ) {
                parsedType = typeArg;
                return IsParsedTypeMatched(semanticModel, parsedType, assignedType, out nullableClassTypeMayAssignToNotNullable);
            }
        }
        parsedType = null;
        nullableClassTypeMayAssignToNotNullable = false;
        return false;
    }

    public static bool IsValidMethodParser(SemanticModel semanticModel, IMethodSymbol method, ITypeSymbol assignedType, bool isFlag,
        [NotNullWhen(true)] out ITypeSymbol? parsedType, out bool nullableClassTypeMayAssignToNotNullable)
    {
        if (isFlag) {
            // Match delegate signature
            if (method is not { Parameters: [{ Type.SpecialType: SpecialType.System_Boolean }] })
                goto End;
            parsedType = method.ReturnType;
            return IsParsedTypeMatched(semanticModel, parsedType, assignedType, out nullableClassTypeMayAssignToNotNullable);
        }
        else {
            if (method is not {
                ReturnType.SpecialType: SpecialType.System_Boolean,
                Parameters:
                [
                    { Type: var spanParameterType },
                    {
                        RefKind: RefKind.Out,
                        Type: var resultParameterType,
                    }
                ]
            } || spanParameterType.ToDisplayString() != Constants.ReadOnlySpan_Char_TypeName) {
                goto End;
            }

            parsedType = resultParameterType;
            return IsParsedTypeMatched(semanticModel, parsedType, assignedType, out nullableClassTypeMayAssignToNotNullable);
        }

    End:
        parsedType = null;
        nullableClassTypeMayAssignToNotNullable = default;
        return false;
    }

    private static bool IsParsedTypeMatched(SemanticModel semanticModel, ITypeSymbol parsedType, ITypeSymbol assignedType, out bool nullableClassTypeMayAssignToNotNullable)
    {
        var conversion = semanticModel.Compilation.ClassifyCommonConversion(parsedType, assignedType);
        if (!conversion.IsImplicit) {
            nullableClassTypeMayAssignToNotNullable = default;
            return false;
        }
        else {
            nullableClassTypeMayAssignToNotNullable = conversion.IsIdentity &&
                parsedType.NullableAnnotation == NullableAnnotation.Annotated &&
                assignedType.NullableAnnotation != NullableAnnotation.Annotated;
            return true;
        }
    }

    public static bool IsValidCommandPrefix(string commandPrefix)
    {
        if (commandPrefix[0] == '-')
            return false;

        foreach (var c in commandPrefix) {
            if (char.IsWhiteSpace(c))
                return false;
        }

        return true;
    }

    public static bool IsCanBeDefault(this ITypeSymbol type)
    {
        return type is not {
            IsValueType: false,
            NullableAnnotation: NullableAnnotation.NotAnnotated,
        };
    }
}
