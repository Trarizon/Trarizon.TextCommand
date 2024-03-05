using Microsoft.CodeAnalysis;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
internal static class DiagnosticFactory
{
    public static Diagnostic Create(DiagnosticDescriptor descriptor, Either<SyntaxNode, SyntaxToken> syntax, params object?[]? messageArgs)
        => syntax.TryGetLeft(out var node, out var token)
        ? Diagnostic.Create(descriptor, Location.Create(node.SyntaxTree, node.Span), messageArgs)
        : Diagnostic.Create(descriptor, Location.Create(token.SyntaxTree!, token.Span), messageArgs);
}
