using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal sealed class OptionParameterModel(ParameterModel parameter) : ICLParameterModel, IRequiredParameterModel, INamedParameterModel
{
    public ParameterModel Parameter => parameter;

    public string? Alias { get; init; }

    private string? _name;
    public string Name
    {
        get => _name ??= parameter.Symbol.Name;
        set => _name = value;
    }

    public bool Required { get; init; }

    public required Either<ImplicitCLParameterKind, Either<(ITypeSymbol Type, ISymbol Member), IMethodSymbol>> ParserInfo { get; init; }

    private ITypeSymbol? _parsedTypeSymbol;
    public ITypeSymbol ParsedTypeSymbol
    {
        get {
            if (_parsedTypeSymbol is null) {
                _parsedTypeSymbol = Parameter.Symbol.Type;
                // Remove nullable annotation of reference type for implicit parser
                if (!_parsedTypeSymbol.IsValueType && _parsedTypeSymbol.NullableAnnotation is NullableAnnotation.Annotated)
                    _parsedTypeSymbol = _parsedTypeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            }
            return _parsedTypeSymbol;
        }

        init => _parsedTypeSymbol = value;
    }
}
