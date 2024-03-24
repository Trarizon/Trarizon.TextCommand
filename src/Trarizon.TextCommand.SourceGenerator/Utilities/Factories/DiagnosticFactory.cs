using Microsoft.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
internal static class DiagnosticFactory
{
    public static Diagnostic Create(DiagnosticDescriptor descriptor, SyntaxNodeOrToken syntax, params object?[]? messageArgs)
        => Diagnostic.Create(descriptor, Location.Create(syntax.SyntaxTree!, syntax.Span), messageArgs);
}
