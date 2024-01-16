using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal interface ICLParameterModel
{
    ParameterModel Parameter { get; }

    Either<ImplicitCLParameterKind, (ITypeSymbol Type, ISymbol Member)> ParserInfo { get; }

    TypeSyntax ParsedTypeSyntax { get; }

    ITypeSymbol ParsedTypeSymbol { get; }
}
