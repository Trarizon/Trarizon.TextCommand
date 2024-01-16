using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class CommandModel(TypeDeclarationSyntax syntax, INamedTypeSymbol symbol)
{
    public TypeDeclarationSyntax Syntax => syntax;
    public INamedTypeSymbol Symbol => symbol;
}
