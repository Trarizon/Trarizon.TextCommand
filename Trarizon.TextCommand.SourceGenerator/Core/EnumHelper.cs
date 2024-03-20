using Microsoft.CodeAnalysis;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal static class EnumHelper
{
    public static InputParameterType GetInputParameterType(ITypeSymbol symbol)
    {
        return symbol switch {
            ITypeSymbol { SpecialType: SpecialType.System_String } => InputParameterType.String,
            ITypeSymbol when symbol.MatchDisplayString(Constants.ReadOnlySpan_Char_TypeName) => InputParameterType.String,
            _ => InputParameterType.Invalid,
        };
    }

    public static ParameterKind GetParameterKind(AttributeData attributeData)
    {
        return attributeData.AttributeClass?.ToDisplayString() switch {
            Literals.FlagAttribute_TypeName => ParameterKind.Flag,
            Literals.OptionAttribute_TypeName => ParameterKind.Option,
            Literals.ValueAttribute_TypeName => ParameterKind.Value,
            Literals.MultiValueAttribute_TypeName => ParameterKind.MultiValue,
            Literals.ContextParameterAttribute_TypeName => ParameterKind.Context,
            _ => ParameterKind.Invalid,
        };
    }

    public static ImplicitParameterKind GetImplicitParameterKind(this ITypeSymbol type)
    {
        // Boolean
        if (type.SpecialType is SpecialType.System_Boolean)
            return ImplicitParameterKind.Boolean;

        // Nullable

        if (type.IsNullableValueType(out var underlyingType))
            type = underlyingType;

        // ISpanParsable
        var isSpanParsable = type.AllInterfaces.Any(interfaceType
            => interfaceType.MatchDisplayString(Constants.ISpanParsable_TypeName, SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric)
            && interfaceType.TypeArguments.Length == 1);
        if (isSpanParsable) {
            return ImplicitParameterKind.SpanParsable;
        }

        // Enum
        if (type.TypeKind == TypeKind.Enum) {
            return ImplicitParameterKind.Enum;
        }

        // Invalid
        return ImplicitParameterKind.Invalid;
    }

}
