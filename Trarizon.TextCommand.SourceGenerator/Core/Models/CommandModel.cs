using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class CommandModel(TypeDeclarationSyntax syntax, INamedTypeSymbol symbol)
{
    public TypeDeclarationSyntax Syntax => syntax;
    public INamedTypeSymbol Symbol => symbol;

    public List<ExecutorModel> GetExecutors(ExecutionModel execution)
    {
        List<ExecutorModel> executors = [];
        foreach (var member in Syntax.Members) {
            if (member is MethodDeclarationSyntax method) {
                var symbol = execution.Context.SemanticModel.GetDeclaredSymbol(method)!;
                if (symbol.GetAttributes() is [var attr, ..] &&
                    attr.AttributeClass?.ToDisplayString() == Literals.ExecutorAttribute_TypeName &&
                    !SymbolEqualityComparer.Default.Equals(symbol, Symbol)
                ) {
                    executors.Add(new ExecutorModel(execution, method, symbol, attr));
                }
            }
        }
        return executors;
    }
}
