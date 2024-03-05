using Microsoft.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
internal static class SymbolExtensions
{
    public static bool MatchDisplayString(this ISymbol symbol, string displayString, SymbolDisplayFormat? symbolDisplayFormat = null)
        => symbol.ToDisplayString(symbolDisplayFormat) == displayString;
}
