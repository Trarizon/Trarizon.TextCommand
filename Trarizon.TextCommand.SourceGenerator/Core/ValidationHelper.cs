using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
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
                return IsTypeAssignable(semanticModel, parsedType, assignedType, out nullableClassTypeMayAssignToNotNullable);
            }
        }

        parsedType = default;
        nullableClassTypeMayAssignToNotNullable = default;
        return false;
    }

    public static bool IsValidMethodParser(SemanticModel semanticModel, IMethodSymbol method, ITypeSymbol assignedType, bool isFlag,
        [NotNullWhen(true)] out ITypeSymbol? parsedType, out bool nullableClassTypeMayAssignToNotNullable)
    {
        if (isFlag) {
            // Match delegate signature
            if (method is { Parameters: [{ Type.SpecialType: SpecialType.System_Boolean }] }) {
                parsedType = method.ReturnType;
                return IsTypeAssignable(semanticModel, parsedType, assignedType, out nullableClassTypeMayAssignToNotNullable);
            }
            goto Failed;
        }

        if (method is {
            ReturnType.SpecialType: SpecialType.System_Boolean,
            Parameters:
            [
                { Type: var inputParameterType },
                { RefKind: RefKind.Out, Type: var resultParameterType, }
            ]
        } && inputParameterType.MatchDisplayString(Literals.InputArg_TypeName)) {
            parsedType = resultParameterType;
            return IsTypeAssignable(semanticModel, parsedType, assignedType, out nullableClassTypeMayAssignToNotNullable);
        }

    Failed:
        parsedType = default;
        nullableClassTypeMayAssignToNotNullable = default;
        return false;
    }

    public static bool IsValidErrorHandler(SemanticModel semanticModel, IMethodSymbol method, ITypeSymbol executionReturnType)
    {
        if (!(method.ReturnsVoid || IsTypeAssignable(semanticModel, method.ReturnType, executionReturnType, out _)))
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

    private static bool IsTypeAssignable(SemanticModel semanticModel, ITypeSymbol type, ITypeSymbol assignedType, out bool nullableClassTypeMayAssignToNotNullable)
    {
        var conversion = semanticModel.Compilation.ClassifyCommonConversion(type, assignedType);

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
            assignedType.NullableAnnotation == NullableAnnotation.NotAnnotated;
        return true;
    }
}
