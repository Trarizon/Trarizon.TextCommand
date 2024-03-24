using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Core.Tags;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas.Markers;
internal interface IInputParameterData : IParameterData
{
    CustomParserInfo ParserInfo { get; }
    /// <summary>
    /// Actually the first type argument of Get&lt;T,TParser&gt;() method
    /// <br/>
    /// For multi-param, this may be the elementType,
    /// for others, this may be the arguemtn type,
    /// </summary>
    ITypeSymbol TargetElementTypeSymbol { get; }
}

internal static class IInputParameterDataExtensions
{
    public static ParserWrapperKinds GetParserWrapperKinds(this IInputParameterData self) {
        var targetType = self.TargetElementTypeSymbol;
        var parserRetType = self.ParserInfo.ParserReturnType;

        // Same type
        if (self.Model.SemanticModel.Compilation.ClassifyCommonConversion(parserRetType, targetType).IsIdentity)
            return ParserWrapperKinds.None;

        if (targetType.IsNullableValueType(out var underlyingType)) {
            if (self.Model.SemanticModel.Compilation.ClassifyCommonConversion(parserRetType, underlyingType).IsIdentity)
                return ParserWrapperKinds.Nullable;
            else
                return ParserWrapperKinds.Nullable | ParserWrapperKinds.ImplicitConversion;
        }

        return ParserWrapperKinds.ImplicitConversion;
    }
}
