using Microsoft.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
internal static class DiagnosticFactory
{
    public static Diagnostic Create(DiagnosticDescriptor descriptor, SyntaxNode node, params object?[]? messageArgs)
        => Diagnostic.Create(descriptor, Location.Create(node.SyntaxTree, node.Span), messageArgs);

    public static Diagnostic Create(DiagnosticDescriptor descriptor, SyntaxToken token, params object?[]? messageArgs)
        => Diagnostic.Create(descriptor, Location.Create(token.SyntaxTree!, token.Span), messageArgs);
}
