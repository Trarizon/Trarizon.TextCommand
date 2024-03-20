using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal static class ValidationHelper
{
    /// <summary>
    /// Is struct or nullable class
    /// </summary>
    public static bool IsCanBeDefault(this ITypeSymbol type)
    {
        return type is not {
            IsValueType: false,
            NullableAnnotation: NullableAnnotation.NotAnnotated,
        };
    }

    public static bool IsValidCommandPrefix(string commandPrefix)
    {
        if (!commandPrefix.TryAt(0, out var c))
            return false;
        if (c == '-')
            return false;
        if (commandPrefix.Any(char.IsWhiteSpace))
            return false;

        return true;
    }

    public static MultiValueCollectionType ValidateMultiCollectionType(ITypeSymbol type, out ITypeSymbol elementType)
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
                _ => MultiValueCollectionType.Invalid,
            };
        }

        elementType = null!;
        return MultiValueCollectionType.Invalid;
    }

    public static bool IsValidParserType(SemanticModel semanticModel, ITypeSymbol parserType, ITypeSymbol assignedType, bool isFlag,
        [NotNullWhen(true)] out ITypeSymbol? parsedType, out bool nullableClassTypeMayAssignToNotNullable)
    {
        var displayString = isFlag ? Literals.IArgFlagParser_TypeName : Literals.IArgParser_TypeName;

        foreach (var type in parserType.AllInterfaces) {
            if (type.MatchDisplayString(displayString, SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric)
                && type.TypeArguments is [var typeArg]
            ) {
                parsedType = typeArg;
                return IsTypeImplicitAssignable(semanticModel, parsedType, assignedType, out nullableClassTypeMayAssignToNotNullable);
            }
        }

        parsedType = default;
        nullableClassTypeMayAssignToNotNullable = default;
        return false;
    }

    public static bool IsValidMethodParser(SemanticModel semanticModel, IMethodSymbol method, ITypeSymbol assignedType, bool isFlag,
        [NotNullWhen(true)] out ITypeSymbol? parsedType, out MethodParserInputKind inputKind, out bool nullableClassTypeMayAssignToNotNullable)
    {
        if (isFlag) {
            // Match delegate signature
            if (method is { Parameters: [{ Type.SpecialType: SpecialType.System_Boolean }] }) {
                parsedType = method.ReturnType;
                inputKind = MethodParserInputKind.Invalid;
                return IsTypeImplicitAssignable(semanticModel, parsedType, assignedType, out nullableClassTypeMayAssignToNotNullable);
            }
        }

        if (method is {
            ReturnType.SpecialType: SpecialType.System_Boolean,
            Parameters:
            [
                { Type: var inputParameterType },
                { RefKind: RefKind.Out, Type: var resultParameterType, }
            ]
        }) {
            if (inputParameterType.SpecialType is SpecialType.System_String)
                inputKind = MethodParserInputKind.String;
            else {
                inputKind = inputParameterType.ToDisplayString() switch {
                    Constants.ReadOnlySpan_Char_TypeName => MethodParserInputKind.ReadOnlySpanChar,
                    Literals.InputArg_TypeName => MethodParserInputKind.InputArg,
                    _ => MethodParserInputKind.Invalid,
                };
            }

            if (inputKind is not MethodParserInputKind.Invalid) {
                parsedType = resultParameterType;
                return IsTypeImplicitAssignable(semanticModel, parsedType, assignedType, out nullableClassTypeMayAssignToNotNullable);
            }
        }

        parsedType = default;
        nullableClassTypeMayAssignToNotNullable = default;
        inputKind = default;
        return false;
    }

    public static bool IsValidErrorHandler(SemanticModel semanticModel, IMethodSymbol method, ITypeSymbol executionReturnType)
    {
        if (!(method.ReturnsVoid || semanticModel.Compilation.ClassifyCommonConversion(method.ReturnType, executionReturnType).IsImplicit))
            return false;

        switch (method.Parameters) {
            case [{ Type: var type, RefKind: RefKind.None or RefKind.In }] when type.MatchDisplayString(Literals.ArgParsingErrors_TypeName):
                return true;
            case [{ Type: var type, RefKind: RefKind.None or RefKind.In }, { Type.SpecialType: SpecialType.System_String }] when type.MatchDisplayString(Literals.ArgParsingErrors_TypeName):
                return true;
            default:
                break;
        }

        return false;
    }

    public static bool IsTypeImplicitAssignable(SemanticModel semanticModel, ITypeSymbol type, ITypeSymbol targetType, out bool nullableClassTypeMayAssignToNotNullable)
    {
        var conversion = semanticModel.Compilation.ClassifyCommonConversion(type, targetType);

        if (!conversion.IsImplicit) {
            nullableClassTypeMayAssignToNotNullable = default;
            return false;
        }

        // int = int
        // int? = int
        // A? = int; 
        // A = int;
        // A? = int?

        nullableClassTypeMayAssignToNotNullable = conversion.IsIdentity &&
            type.NullableAnnotation == NullableAnnotation.Annotated &&
            targetType.NullableAnnotation == NullableAnnotation.NotAnnotated;
        return true;
    }

    public static bool IsParameterRefKindPassable(RefKind from, RefKind to)
    {
        return (from, to) switch {
            (RefKind.In, RefKind.In or RefKind.None or RefKind.RefReadOnlyParameter) => true,
            (RefKind.None, RefKind.None or RefKind.Out or RefKind.Ref or RefKind.RefReadOnlyParameter or RefKind.In) => true,
            (RefKind.Out, RefKind.Out) => true,
            (RefKind.Ref, RefKind.Ref or RefKind.RefReadOnlyParameter or RefKind.In or RefKind.None or RefKind.Out) => true,
            (RefKind.RefReadOnlyParameter, RefKind.RefReadOnlyParameter or RefKind.In or RefKind.None) => true,
            _ => false,
        };
    }
}
