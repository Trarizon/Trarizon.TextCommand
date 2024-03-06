using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.ParameterDatas;
internal sealed class ValueParameterData(ParameterModel model) : IParameterData, IRequiredParameterData, IValueParameterData
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

    public bool Required { get; init; }

    public int Index { get; set; }

    public bool IsUnreachable => Index < 0;

    int IValueParameterData.MaxCount => 1;

    bool IValueParameterData.IsRest => false;
}
