using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Core.Models;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal sealed class OptionParameterData(ParameterModel model) : IParameterData, INamedParameterData, IRequiredParameterData
{
    public ParameterModel Model { get; } = model;

    public required ParserInfoProvider ParserInfo { get; init; }

    private ITypeSymbol? _parsedTypeSymbol;
    public ITypeSymbol ParsedTypeSymbol
    {
        get
        {
            if (_parsedTypeSymbol is null)
            {
                _parsedTypeSymbol = Model.Symbol.Type;
                // Remove nullable annotation of reference type for implicit parser
                if (!_parsedTypeSymbol.IsValueType && _parsedTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
                    _parsedTypeSymbol = _parsedTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            }
            return _parsedTypeSymbol;
        }
        init => _parsedTypeSymbol = value;
    }

    public string? Alias { get; init; }

    private string? _name;
    public string Name
    {
        get => _name ??= Model.Symbol.Name;
        init => _name = value;
    }

    public bool Required { get; init; }
}
