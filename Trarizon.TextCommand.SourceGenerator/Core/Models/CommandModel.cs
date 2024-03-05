using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class CommandModel(GeneratorAttributeSyntaxContext context)
{
    public SemanticModel SemanticModel => context.SemanticModel;

    private ExecutionModel? _executionModel;
    public ExecutionModel ExecutionModel => _executionModel ??= new ExecutionModel(this, context.Attributes[0]) {
        Syntax = (MethodDeclarationSyntax)context.TargetNode,
        Symbol = (IMethodSymbol)context.TargetSymbol,
    };

    private TypeDeclarationSyntax? _syntax;
    public TypeDeclarationSyntax Syntax => _syntax ??= ExecutionModel.Syntax.Ancestors()
        .OfType<TypeDeclarationSyntax>()
        .First();

    private INamedTypeSymbol? _symbol;
    public INamedTypeSymbol Symbol => _symbol ??= SemanticModel.GetDeclaredSymbol(Syntax)!;

    public IEnumerable<ExecutorModel> GetExecutorModels(ExecutionModel execution)
    {
        foreach (var methodSyntax in Syntax.Members.OfType<MethodDeclarationSyntax>()) {
            var methodSymbol = SemanticModel.GetDeclaredSymbol(methodSyntax);

            // Not well declared or not the executor
            if (methodSymbol is null || SymbolEqualityComparer.Default.Equals(execution.Symbol, methodSymbol))
                continue;

            var executorAttrs = methodSymbol.GetAttributes()
                .Where(attr => attr.AttributeClass?.MatchDisplayString(Literals.ExecutorAttribute_TypeName) == true);

            if (executorAttrs.Any()) {
                yield return new ExecutorModel(execution, executorAttrs) {
                    Symbol = methodSymbol!,
                    Syntax = methodSyntax,
                };
            }
        }
    }
}
