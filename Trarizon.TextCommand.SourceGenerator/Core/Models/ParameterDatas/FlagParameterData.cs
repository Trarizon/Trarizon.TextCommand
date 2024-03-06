using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class FlagParameterData(ParameterModel model) : IParameterData, INamedParameterData
{
    public ParameterModel Model { get; } = model;

    public required ParserInfoProvider ParserInfo { get; init; }

    private ITypeSymbol? _resultTypeSymbol;
    public ITypeSymbol ResultTypeSymbol 
    {
        get {
            if (_resultTypeSymbol is null) {
                _resultTypeSymbol = Model.Symbol.Type;
                // Remove nullable annotation of reference type for implicit parser
                if (_resultTypeSymbol is {
                    IsValueType: false,
                    NullableAnnotation: NullableAnnotation.Annotated
                }) {
                    _resultTypeSymbol = _resultTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
                }
            }
            return _resultTypeSymbol;
        }
        init => _resultTypeSymbol = value;
    }

    public ITypeSymbol ParsedTypeSymbol => ResultTypeSymbol;

    // Attribute data

    public string? Alias { get; init; }

    private string? _name;
    public string Name
    {
        get => _name ??= Model.Symbol.Name;
        [param: AllowNull]
        init => _name = value;
    }
}
