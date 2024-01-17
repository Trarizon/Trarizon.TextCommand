using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal interface ICLParameterModel
{
    ParameterModel Parameter { get; }

    Either<ImplicitCLParameterKind, Either<(ITypeSymbol Type, ISymbol Member), IMethodSymbol>> ParserInfo { get; }

    ITypeSymbol ParsedTypeSymbol { get; }
}
