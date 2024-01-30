using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit.Extensions;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class CommandModel(GeneratorAttributeSyntaxContext context)
{
    public SemanticModel SemanticModel => context.SemanticModel;

    private ExecutionModel? _executionModel;
    public ExecutionModel ExecutionModel => _executionModel ??=
        new ExecutionModel(this,
            (MethodDeclarationSyntax)context.TargetNode,
            (IMethodSymbol)context.TargetSymbol,
            context.Attributes[0]);

    private TypeDeclarationSyntax? _syntax;
    public TypeDeclarationSyntax Syntax
    {
        get => _syntax ?? throw new InvalidOperationException();
        private set => _syntax = value;
    }

    private INamedTypeSymbol? _symbol;
    public INamedTypeSymbol Symbol => _symbol ??= SemanticModel.GetDeclaredSymbol(Syntax)!;

    public Filter<ExecutionModel> SelectExecution()
    {
        var method = (MethodDeclarationSyntax)context.TargetNode;
        var syntax = method.GetParent<TypeDeclarationSyntax>();
        if (syntax is null)
            return default;
        else {
            Syntax = syntax;
            return Filter.Create(ExecutionModel);
        }
    }

    public IReadOnlyList<ExecutorModel> GetExecutors(ExecutionModel execution)
    {
        List<ExecutorModel> executors = [];

        foreach (var methodSyntax in Syntax.Members.OfType<MethodDeclarationSyntax>()) {
            var methodSymbol = SemanticModel.GetDeclaredSymbol(methodSyntax);

            // Not well declared || not the executor
            if (methodSymbol is null || SymbolEqualityComparer.Default.Equals(execution.Symbol, methodSymbol))
                continue;

            var executorAttrs = methodSymbol?.GetAttributes()
                .Where(attr => attr.AttributeClass?.ToDisplayString() == Literals.ExecutorAttribute_TypeName);

            if (executorAttrs?.Any() == true) {
                executors.Add(new ExecutorModel(execution, methodSyntax, methodSymbol!, executorAttrs.ToList()));
            }
        }
        return executors;
    }
}
