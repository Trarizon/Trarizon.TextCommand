using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Core.Models;

namespace Trarizon.TextCommand.SourceGenerator.Core.Providers;
internal class CommandProvider(CommandModel model)
{
    public INamedTypeSymbol Symbol => model.Symbol;
    public TypeDeclarationSyntax Syntax => model.Syntax;
}
