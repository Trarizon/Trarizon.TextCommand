using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class OptionParameterData(ParameterModel model) : IParameterData, INamedParameterData, IRequiredParameterData
{
    public ParameterModel Model { get; } = model;

    public required ParserInfoProvider ParserInfo { get; init; }

    private ITypeSymbol? _resultTypeSymbol;
    public ITypeSymbol ResultTypeSymbol
    {
        get => _resultTypeSymbol ?? Model.Symbol.Type;
        init => _resultTypeSymbol = value;
    }

    private ITypeSymbol? _parsedTypeSymbol;
    public ITypeSymbol ParsedTypeSymbol
    {
        // For implicit parser, ParsedType will not has nullable annotation
        get => _parsedTypeSymbol ??= ResultTypeSymbol.RemoveNullableAnnotation();
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
