using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal sealed class MultiValueParameterModel(ParameterModel parameter) : ICLParameterModel, IRequiredParameterModel
{
    public ParameterModel Parameter => parameter;

    private int _maxCount;
    public int MaxCount
    {
        get => IsRest ? int.MaxValue : _maxCount;
        init => _maxCount = value;
    }

    public bool IsRest => _maxCount <= 0;

    public bool Required { get; init; }

    public required MultiValueCollectionType CollectionType { get; init; }

    public required Either<ImplicitCLParameterKind, Either<(ITypeSymbol Type, ISymbol Member), IMethodSymbol>> ParserInfo { get; init; }

    public required ITypeSymbol ParsedTypeSymbol { get; init; }
}
