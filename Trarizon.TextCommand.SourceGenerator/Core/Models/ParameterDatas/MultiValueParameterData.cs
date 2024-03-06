using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class MultiValueParameterData(ParameterModel model) : IParameterData, IRequiredParameterData, IValueParameterData
{
    public ParameterModel Model { get; } = model;

    public required ParserInfoProvider ParserInfo { get; init; }

    private ITypeSymbol? _resultTypeSymbol;
    public required ITypeSymbol ResultTypeSymbol
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

    public bool Required { get; init; }

    private int _maxCount;
    public int MaxCount
    {
        get => IsRest ? int.MaxValue - Index : _maxCount;
        init => _maxCount = value;
    }

    public bool IsRest => _maxCount <= 0;

    public required MultiValueCollectionType CollectionType { get; init; }

    public int Index { get; set; }

    public bool IsUnreachable => Index < 0;
}
