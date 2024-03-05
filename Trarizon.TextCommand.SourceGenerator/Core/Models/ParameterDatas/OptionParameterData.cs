using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class OptionParameterData(ParameterModel model) : IParameterData, INamedParameterData, IRequiredParameterData
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
                if (_parsedTypeSymbol is {
                    IsValueType: false,
                    NullableAnnotation: NullableAnnotation.Annotated
                }) {
                    _parsedTypeSymbol = _parsedTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                }
            }
            return _parsedTypeSymbol;
        }
        init => _parsedTypeSymbol = value;
    }

    // Attribute data

    public string? Alias { get; init; }

    private string? _name;
    public string Name
    {
        get => _name ??= Model.Symbol.Name;
        [param: AllowNull]
        init => _name = value;
    }

    public bool Required { get; init; }
}
