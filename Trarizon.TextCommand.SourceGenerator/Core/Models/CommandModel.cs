using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class CommandModel(TypeDeclarationSyntax syntax, INamedTypeSymbol symbol)
{
    public TypeDeclarationSyntax Syntax => syntax;
    public INamedTypeSymbol Symbol => symbol;

    public List<ExecutorModel> GetExecutors(ExecutionModel execution)
    {
        List<ExecutorModel> executors = [];
        foreach (var methodSyntax in Syntax.Members.OfType<MethodDeclarationSyntax>()) {
            var methodSymbol = execution.Context.SemanticModel.GetDeclaredSymbol(methodSyntax)!;

            // Not execution method
            if (SymbolEqualityComparer.Default.Equals(execution.Symbol, methodSymbol))
                continue;

            var executorAttrs = methodSymbol.GetAttributes()
                 .Where(attr => attr.AttributeClass?.ToDisplayString() == Literals.ExecutorAttribute_TypeName);

            if (executorAttrs.Any()) {
                executors.Add(new ExecutorModel(execution, methodSyntax, methodSymbol, executorAttrs.ToList()));
            }
        }
        return executors;
    }
}
