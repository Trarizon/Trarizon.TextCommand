using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Utilities;
using Trarizon.TextCommand.SourceGenerator.Utilities.Extensions;
using Trarizon.TextCommand.SourceGenerator.Utilities.Factories;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models;
internal sealed class ExecutionModel(ContextModel context, MethodDeclarationSyntax syntax, IMethodSymbol symbol, AttributeData attribute)
{
    public ContextModel Context => context;
    public CommandModel Command => context.CommandModel;

    public MethodDeclarationSyntax Syntax => syntax;
    public IMethodSymbol Symbol => symbol;

    private readonly AttributeData _attribute = attribute;

    private List<ExecutorModel>? _executors;
    public List<ExecutorModel> Executors => _executors ??= Command.GetExecutors(this);

    private string? _commandName;
    public string? CommandName => _commandName ??= _attribute.GetConstructorArgument<string>(Literals.ExecutionAttribute_CommandName_CtorParameterIndex);

    // Values

    public InputParameterType InputParameterType { get; private set; }

    public Filter ValidatePartialKeyWord()
    {
        if (Syntax.Modifiers.Any(SyntaxKind.PartialKeyword)) {
            return Filter.Success;
        }
        return Filter.CreateDiagnostic(DiagnosticFactory.Create(
            DiagnosticDescriptors.ExecutionMethodShouldBePartial,
            Syntax.Identifier));
    }

    public Filter ValidateParameter()
    {
        if (Symbol.Parameters is not [{ Type: var parameterType }]) {
            return Filter.CreateDiagnostic(DiagnosticFactory.Create(
                DiagnosticDescriptors.ExecutionMethodOnlyHasOneParameter,
                Syntax.Identifier));
        }

        // string | ReadOnlySpan<char> -> String
        // string[] -> Array
        // List<string> -> List
        // ReadOnlySpan<string> | Span<string> -> Span
        InputParameterType = parameterType switch {
            ITypeSymbol { SpecialType: SpecialType.System_String }
                => InputParameterType.String,
            INamedTypeSymbol { TypeArguments: [{ SpecialType: SpecialType.System_Char }] } rosCharType
                => rosCharType.ToDisplayString(SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric) is Constants.ReadOnlySpan_TypeName
                    ? InputParameterType.String
                    : InputParameterType.Unknown,
            IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_String }
                => InputParameterType.Array,
            INamedTypeSymbol { TypeArguments: [{ SpecialType: SpecialType.System_String }] } namedType
                => namedType.ToDisplayString(SymbolDisplayFormats.CSharpErrorMessageWithoutGeneric) switch {
                    Constants.ReadOnlySpan_TypeName or Constants.Span_TypeName
                        => InputParameterType.Span,
                    Constants.List_TypeName
                        => InputParameterType.List,
                    _ => InputParameterType.Unknown,
                },
            _ => InputParameterType.Unknown,
        };

        return Filter.Success;
    }

    public Filter ValidateReturnType()
    {
        if (Symbol.ReturnType.IsCanBeDefault()) {
            return Filter.Success;
        }

        return Filter.CreateDiagnostic(DiagnosticFactory.Create(
                DiagnosticDescriptors.ExecutionMethodReturnTypeShouldBeNullable,
                Syntax.ReturnType));
    }

    public Filter ValidateCommandName()
    {
        var name = CommandName;

        if (name is null || ValidationHelper.IsValidCommandPrefix(name))
            return Filter.Success;
        else
            return Filter.CreateDiagnostic(DiagnosticFactory.Create(
                DiagnosticDescriptors.CommandPrefixCannotContainsSpaceOrLeadingWithMinus,
                Syntax.Identifier));
    }

    public Filter ValidateExecutorsCommandPrefixes()
    {
        List<Diagnostic>? result = null;
        for (int i = 0; i < Executors.Count; i++) {
            var former = Executors[i];
            for (int j = i + 1; j < Executors.Count; j++) {
                var latter = Executors[j];
                if (RepeatOfTruncate(former.CommandPrefixes, latter.CommandPrefixes)) {
                    (result ??= []).Add(DiagnosticFactory.Create(
                        DiagnosticDescriptors.ExecutorCommandPrefixRepeatOrTruncate_1,
                        latter.Syntax.Identifier,
                        former.Symbol.Name));
                }
            }
        }

        if (result != null)
            return Filter.CreateDiagnostics(result);
        else
            return Filter.Success;

        static bool RepeatOfTruncate(string[] former, string[] latter)
        {
            if (former.Length > latter.Length)
                return false;

            return former.AsSpan().SequenceEqual(latter.AsSpan(0, former.Length));
        }
    }
}
