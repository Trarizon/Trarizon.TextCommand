using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
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

    public static ImplicitCLParameterKind ValidateImplicitParameterKind(ITypeSymbol type)
    {
        // Boolean
        if (type.SpecialType is SpecialType.System_Boolean) {
            return ImplicitCLParameterKind.Boolean;
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
                ? ImplicitCLParameterKind.NullableSpanParsable
                : ImplicitCLParameterKind.SpanParsable;
        }

        // Enum
        if (type.TypeKind == TypeKind.Enum) {
            return isNullable
                ? ImplicitCLParameterKind.NullableEnum
                : ImplicitCLParameterKind.Enum;
        }

        // Invalid
        return ImplicitCLParameterKind.Invalid;
    }

    public static MultiValueCollectionType ValidateMultiValueCollectionType(ITypeSymbol type, out ITypeSymbol elementType, out Func<TypeSyntax, TypeSyntax> elementSyntaxGetter)
    {
        if (type is IArrayTypeSymbol arrayType) {
            elementType = arrayType.ElementType;
            elementSyntaxGetter = static array => ((ArrayTypeSyntax)array).ElementType;
            return MultiValueCollectionType.Array;
        }

        if (type is INamedTypeSymbol { TypeArguments: [var typeArg] } namedType) {
            elementType = typeArg;
            elementSyntaxGetter = static generic => ((GenericNameSyntax)generic).TypeArgumentList.Arguments[0];
            return namedType.ToDisplayString(SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric) switch {
                Constants.ReadOnlySpan_TypeName => MultiValueCollectionType.ReadOnlySpan,
                Constants.Span_TypeName => MultiValueCollectionType.Span,
                Constants.List_TypeName => MultiValueCollectionType.List,
                Constants.IEnumerable_TypeName => MultiValueCollectionType.Enumerable,
                _ => MultiValueCollectionType.Invalid
            };
        }

        elementType = null!;
        elementSyntaxGetter = null!;
        return MultiValueCollectionType.Invalid;
    }

    public static bool IsCustomParser(ITypeSymbol parserType, ITypeSymbol argType, out bool isFlag)
    {
        foreach (var interfaceType in parserType.AllInterfaces) {
            switch (interfaceType.ToDisplayString(SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric)) {
                case Literals.IArgParser_TypeName:
                    if (interfaceType.TypeArguments is [var typeArg] &&
                        (typeArg.IsValueType
                            ? SymbolEqualityComparer.IncludeNullability.Equals(typeArg, argType)
                            : SymbolEqualityComparer.Default.Equals(typeArg, argType))
                    ) {
                        isFlag = false;
                        return true;
                    }
                    break;
                case Literals.IArgFlagParser_TypeName:
                    if (interfaceType.TypeArguments is [var flagTypeArg] &&
                        (flagTypeArg.IsValueType
                            ? SymbolEqualityComparer.IncludeNullability.Equals(flagTypeArg, argType)
                            : SymbolEqualityComparer.Default.Equals(flagTypeArg, argType))
                    ) {
                        isFlag = true;
                        return true;
                    }
                    break;
            }
        }
        isFlag = default;
        return false;
    }
}
