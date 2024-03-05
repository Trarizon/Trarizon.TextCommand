using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal static class EnumConverters
{
    public static InputParameterType GetInputParameterType(this ITypeSymbol symbol)
    {
        return symbol switch {
            ITypeSymbol { SpecialType: SpecialType.System_String } => InputParameterType.String,
            _ => InputParameterType.Invalid,
        };
    }

    public static ParameterKind GetParameterKind(this AttributeData attributeData)
    {
        return attributeData.AttributeClass?.ToDisplayString() switch {
            Literals.FlagAttribute_TypeName => ParameterKind.Flag,
            Literals.OptionAttribute_TypeName => ParameterKind.Option,
            Literals.ValueAttribute_TypeName => ParameterKind.Value,
            Literals.MultiValueAttribute_TypeName => ParameterKind.MultiValue,
            _ => ParameterKind.Invalid,
        };
    }
}
