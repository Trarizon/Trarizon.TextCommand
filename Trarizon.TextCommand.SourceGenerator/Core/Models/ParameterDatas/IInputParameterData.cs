using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal interface IInputParameterData : IParameterData
{
    ParserInfoProvider ParserInfo { get; }
    /// <summary>
    /// The type `provider.Get` may returns
    /// </summary>
    ITypeSymbol ResultTypeSymbol { get; }
    /// <summary>
    /// The type parser returns, exclude wrappers
    /// </summary>
    ITypeSymbol ParsedTypeSymbol { get; }
}

internal static class IParameterDataExtensions
{
    public static ParserWrapperKind GetParserWrapperKind(this IInputParameterData self)
    {
        var resultType = self.ResultTypeSymbol;
        var parsedType = self.ParsedTypeSymbol;

        // Same type
        // Tuple type cannot use SymbolEqualityComparer to check, so we use this,
        // and check if the conversion is identity 
        var conversion = self.Model.SemanticModel.Compilation.ClassifyCommonConversion(parsedType, resultType);
        if (conversion.IsIdentity) {
            return ParserWrapperKind.None;
        }

        if (resultType.IsNullableValueType(out var underlying)) {
            if (self.Model.SemanticModel.Compilation.ClassifyCommonConversion(underlying, parsedType).IsIdentity)
                return ParserWrapperKind.Nullable;
            else
                return ParserWrapperKind.Nullable | ParserWrapperKind.ImplicitConversion;
        }

        return ParserWrapperKind.ImplicitConversion;
    }
}