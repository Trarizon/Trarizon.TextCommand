using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal sealed class MultiValueParameterModel(ParameterModel parameter) : ICLParameterModel
{
    public ParameterModel Parameter => parameter;

    public int MaxCount { get; init; }

    public bool Required { get; init; }

    public required MultiValueCollectionType CollectionType { get; init; }

    public required Either<ImplicitCLParameterKind, (ITypeSymbol Type, ISymbol Member)> ParserInfo { get; init; }

    public required TypeSyntax? ParsedTypeSyntax { get; init; }

    public required ITypeSymbol ParsedTypeSymbol { get; init; }
}
