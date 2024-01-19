using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal sealed class ValueParameterModel(ParameterModel parameter) : ICLParameterModel, IRequiredParameterModel
{
    public ParameterModel Parameter => parameter;

    public bool Required { get; init; }

    public required Either<ImplicitCLParameterKind, Either<(ITypeSymbol Type, ISymbol Member), IMethodSymbol>> ParserInfo { get; init; }

    public ITypeSymbol ParsedTypeSymbol => Parameter.Symbol.Type;
}
