using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Core.Models;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal sealed class MultiValueParameterData(ParameterModel model) : IParameterData, IRequiredParameterData
{
    public ParameterModel Model { get; } = model;

    public required ParserInfoProvider ParserInfo { get; init; }

    public required ITypeSymbol ParsedTypeSymbol { get; init; }

    public bool Required { get; init; }

    private int _maxCount;
    public int MaxCount
    {
        get => IsRest ? int.MaxValue : _maxCount;
        init => _maxCount = value;
    }

    public bool IsRest => _maxCount <= 0;

    public required MultiValueCollectionType CollectionType { get; init; }
}
