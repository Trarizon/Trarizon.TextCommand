using Microsoft.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class ValueParameterData(ParameterModel model) : IParameterData, IRequiredParameterData, IValueParameterData
{
    public ParameterModel Model { get; } = model;

    public required ParserInfoProvider ParserInfo { get; init; }

    private ITypeSymbol? _parsedTypeSymbol;
    public ITypeSymbol ParsedTypeSymbol
    {
        get {
            if (_parsedTypeSymbol is null) {
                _parsedTypeSymbol = Model.Symbol.Type;
                // Remove nullable annotation of reference type for implicit parser
                if (!_parsedTypeSymbol.IsValueType && _parsedTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
                    _parsedTypeSymbol = _parsedTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            }
            return _parsedTypeSymbol;
        }
        init => _parsedTypeSymbol = value;
    }

    public bool Required { get; init; }

    public int Index { get; set; }

    public bool IsUnreachable => Index < 0;

    int IValueParameterData.MaxCount => 1;

    bool IValueParameterData.IsRest => false;
}
