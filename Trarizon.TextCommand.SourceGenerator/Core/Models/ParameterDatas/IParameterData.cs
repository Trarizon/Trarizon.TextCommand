using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal interface IParameterData
{
    ParameterModel Model { get; }
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
    public static ParserWrapperKind GetParserWrapperKind(this IParameterData self)
    {
        var resultType = self.ResultTypeSymbol;
        var parsedType = self.ParsedTypeSymbol;
        if (SymbolEqualityComparer.Default.Equals(resultType, parsedType)) {
            return ParserWrapperKind.None;
        }

        if (resultType.IsNullableValueType(out var underlying)) {
            if (SymbolEqualityComparer.Default.Equals(underlying, parsedType))
                return ParserWrapperKind.Nullable;
            else
                return ParserWrapperKind.Nullable | ParserWrapperKind.ImplicitConversion;
        }

        return ParserWrapperKind.ImplicitConversion;
    }
}