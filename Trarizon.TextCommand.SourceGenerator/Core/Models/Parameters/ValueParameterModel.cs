using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal sealed class ValueParameterModel(ParameterModel parameter) : ICLParameterModel, IRequiredParameterModel
{
    public ParameterModel Parameter => parameter;

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
