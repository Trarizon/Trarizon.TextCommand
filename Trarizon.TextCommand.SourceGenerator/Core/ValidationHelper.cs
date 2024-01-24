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

    public static bool IsCustomParser(SemanticModel semanticModel, ITypeSymbol parserType, ITypeSymbol parameterType,
        [NotNullWhen(true)] out ITypeSymbol? parsedType, out bool isFlag, out bool nullableClassTypeMayAssignToNotNullable)
    {
        foreach (var interfaceType in parserType.AllInterfaces) {
            switch (interfaceType.ToDisplayString(SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric)) {
                case Literals.IArgParser_TypeName:
                    if (interfaceType.TypeArguments is not [var typeArg])
                        break;
                    isFlag = false;
                    parsedType = typeArg;
                    return IsValidParser(semanticModel, typeArg, parameterType, out nullableClassTypeMayAssignToNotNullable);

                case Literals.IArgFlagParser_TypeName:
                    if (interfaceType.TypeArguments is not [var flagTypeArg])
                        break;
                    isFlag = true;
                    parsedType = flagTypeArg;
                    return IsValidParser(semanticModel, flagTypeArg, parameterType, out nullableClassTypeMayAssignToNotNullable);
            }
        }
        parsedType = null;
        isFlag = default;
        nullableClassTypeMayAssignToNotNullable = false;
        return false;
    }

    public static bool IsValidMethodParser(SemanticModel semanticModel, IMethodSymbol method, ITypeSymbol parameterType, CLParameterKind parameterKind,
        [NotNullWhen(true)] out ITypeSymbol? parsedType, out bool nullableClassTypeMayAssignToNotNullable)
    {
        switch (parameterKind) {
            case CLParameterKind.Flag:
                // Match delegate signature
                if (method is not { Parameters: [{ Type.SpecialType: SpecialType.System_Boolean }] })
                    break;
                parsedType = method.ReturnType;
                return IsValidParser(semanticModel, parsedType, parameterType, out nullableClassTypeMayAssignToNotNullable);
            case CLParameterKind.Option:
            case CLParameterKind.Value:
            case CLParameterKind.MultiValue:
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
                    break;
                }

                parsedType = resultParameterType;
                return IsValidParser(semanticModel, parsedType, parameterType, out nullableClassTypeMayAssignToNotNullable);
            default:
                throw new InvalidOperationException();
        }

        parsedType = null;
        nullableClassTypeMayAssignToNotNullable = default;
        return false;
    }

    private static bool IsValidParser(SemanticModel semanticModel, ITypeSymbol parsedType, ITypeSymbol parameterType, out bool nullableClassTypeMayAssignToNotNullable)
    {
        var conversion = semanticModel.Compilation.ClassifyCommonConversion(parsedType, parameterType);
        if (!conversion.IsImplicit) {
            nullableClassTypeMayAssignToNotNullable = default;
            return false;
        }
        else {
            nullableClassTypeMayAssignToNotNullable = conversion.IsIdentity &&
                parsedType.NullableAnnotation == NullableAnnotation.Annotated &&
                parameterType.NullableAnnotation != NullableAnnotation.Annotated;
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
