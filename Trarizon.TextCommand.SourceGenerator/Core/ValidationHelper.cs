using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal static class ValidationHelper
{
    public static InputParameterKind ValidateNonCustomInputParameterKind(ITypeSymbol inputType)
    {
        return inputType switch {
            ITypeSymbol { SpecialType: SpecialType.System_String } => InputParameterKind.String,
            ITypeSymbol when inputType.MatchDisplayString(Constants.ReadOnlySpan_Char_TypeName) => InputParameterKind.ReadOnlySpan_Char,
            _ => InputParameterKind.Invalid,
        };
    }

    public static InputParameterKind ValidateCustomMatcherSelector(IMethodSymbol method, SemanticModel semanticModel, ITypeSymbol executionInputType)
    {
        if (method.Parameters is not [{ Type: var inputType }])
            return InputParameterKind.Invalid;

        if (!executionInputType.IsImplicitAssignableTo(inputType, semanticModel, out _))
            return InputParameterKind.Invalid;

        return ValidateNonCustomInputParameterKind(method.ReturnType);
    }

    public static bool IsValidCommandPrefix(string commandPrefix)
    {
        if (commandPrefix[0] is '-')
            return false;
        if (commandPrefix.Any(char.IsWhiteSpace))
            return false;
        return true;
    }

    public static ErrorHandlerKind ValidateErrorHandler(IMethodSymbol method, SemanticModel semanticModel, ITypeSymbol executionReturnType)
    {
        if (!method.ReturnsVoid)
            return ErrorHandlerKind.Invalid;
        if (method.ReturnType.IsImplicitAssignableTo(executionReturnType, semanticModel, out _))
            return ErrorHandlerKind.Invalid;

        switch (method.Parameters) {
            case [{ Type: var errType, RefKind: RefKind.None or RefKind.In }] when errType.MatchDisplayString(Literals.ArgParsingErrors_TypeName):
                return ErrorHandlerKind.Minimal;
            case [{ Type: var errType, RefKind: RefKind.None or RefKind.In }, { Type.SpecialType: SpecialType.System_String }] when errType.MatchDisplayString(Literals.ArgParsingErrors_TypeName):
                return ErrorHandlerKind.WithExecutorName;
            default:
                return ErrorHandlerKind.Invalid;
        }
    }

    public static ExecutorParameterKind ValidateExecutorParameter(AttributeData attribute)
    {
        return attribute.AttributeClass?.ToDisplayString() switch {
            Literals.FlagAttribute_TypeName => ExecutorParameterKind.Flag,
            Literals.OptionAttribute_TypeName => ExecutorParameterKind.Option,
            Literals.ValueAttribute_TypeName => ExecutorParameterKind.Value,
            Literals.MultiValueAttribute_TypeName => ExecutorParameterKind.MultiValue,
            Literals.ContextParameterAttribute_TypeName => ExecutorParameterKind.Context,
            _ => ExecutorParameterKind.Invalid,
        };
    }

    public static ImplicitExecutorParameterKind ValidateExecutorImplicitParameterKind(ITypeSymbol parameterType, bool isFlag,
        out ITypeSymbol parserReturnType)
    {
        // Boolean
        if (parameterType.SpecialType is SpecialType.System_Boolean) {
            parserReturnType = parameterType;
            return ImplicitExecutorParameterKind.Boolean;
        }

        if (isFlag) {
            parserReturnType = default!;
            return ImplicitExecutorParameterKind.Invalid;
        }

        // Nullable

        if (parameterType.IsNullableValueType(out var underlyingType))
            parameterType = underlyingType;

        // Enum
        if (parameterType.TypeKind == TypeKind.Enum) {
            parserReturnType = parameterType.IsValueType ? parameterType : parameterType.RemoveNullableAnnotation();
            return ImplicitExecutorParameterKind.Enum;
        }

        // ISpanParsable
        bool isSpanParsable = parameterType.AllInterfaces
            .Any(iType => iType.TypeArguments.Length is 1 && iType.MatchDisplayString(Constants.ISpanParsable_TypeName, SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric));
        if (isSpanParsable) {
            parserReturnType = parameterType.IsValueType ? parameterType : parameterType.RemoveNullableAnnotation();
            return ImplicitExecutorParameterKind.ISpanParsable;
        }

        parserReturnType = default!;
        return ImplicitExecutorParameterKind.Invalid;
    }

    public static MultiParameterCollectionKind ValidateMultiParameterCollectionKind(ITypeSymbol collectionType, out ITypeSymbol elementType)
    {
        if (collectionType is IArrayTypeSymbol arrayType) {
            elementType = arrayType.ElementType;
            return MultiParameterCollectionKind.Array;
        }

        if (collectionType is INamedTypeSymbol { TypeArguments: [var typeArg] } namedType) {
            elementType = typeArg;
            return namedType.ToDisplayString(SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric) switch {
                Constants.ReadOnlySpan_TypeName => MultiParameterCollectionKind.ReadOnlySpan,
                Constants.Span_TypeName => MultiParameterCollectionKind.Span,
                Constants.List_TypeName => MultiParameterCollectionKind.List,
                Constants.IEnumerable_TypeName => MultiParameterCollectionKind.Enumerable,
                _ => MultiParameterCollectionKind.Invalid,
            };
        }

        elementType = null!;
        return MultiParameterCollectionKind.Invalid;
    }

    public static bool IsValidParserType(ITypeSymbol parserType, SemanticModel semanticModel, ITypeSymbol targetType, bool isFlag,
        [NotNullWhen(true)] out ITypeSymbol? parserReturnType)
    {
        var displayString = isFlag ? Literals.IArgFlagParser_TypeName : Literals.IArgParser_TypeName;

        var interfaceType = parserType.AllInterfaces
            .FirstOrDefault(iType => iType.MatchGenericType(displayString, 1));
        if (interfaceType is null) {
            parserReturnType = default;
            return false;
        }

        parserReturnType = interfaceType.TypeArguments[0];
        return parserReturnType.IsImplicitAssignableTo(targetType, semanticModel, out _);
    }

    public static bool IsValidMethodParser(IMethodSymbol method, SemanticModel semanticModel, ITypeSymbol targetType, bool isFlag,
        [NotNullWhen(true)] out ITypeSymbol? parserReturnType, out MethodParserInputParameterKind inputParameterKind)
    {
        if (isFlag) {
            if (method is { Parameters: [{ Type.SpecialType: SpecialType.System_Boolean, RefKind: RefKind.None }] }) {
                parserReturnType = method.ReturnType;
                inputParameterKind = MethodParserInputParameterKind.Flag;
                goto Success;
            }
        }

        // Non-flag

        if (method is not {
            ReturnType.SpecialType: SpecialType.System_Boolean,
            Parameters:
            [
                { Type: var inputParameterType, RefKind: RefKind.None },
                { Type: var resultParameterType, RefKind: RefKind.Out },
            ]
        }) {
            goto Fail;
        }

        if (inputParameterType.SpecialType is SpecialType.System_String) {
            inputParameterKind = MethodParserInputParameterKind.String;
        }
        else {
            inputParameterKind = inputParameterType.ToDisplayString() switch {
                Constants.ReadOnlySpan_Char_TypeName => MethodParserInputParameterKind.ReadOnlySpan_Char,
                Literals.InputArg_TypeName => MethodParserInputParameterKind.InputArg,
                _ => MethodParserInputParameterKind.Invalid,
            };
        }

        if (inputParameterKind is MethodParserInputParameterKind.Invalid) {
            goto Fail;
        }

        parserReturnType = resultParameterType;

    Success:
        return parserReturnType.IsImplicitAssignableTo(targetType, semanticModel, out _);

    Fail:
        parserReturnType = null;
        inputParameterKind = MethodParserInputParameterKind.Invalid;
        return false;
    }
}
