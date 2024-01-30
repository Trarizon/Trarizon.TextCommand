using Microsoft.CodeAnalysis;
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
internal sealed class ExecutionModel(CommandModel command, MethodDeclarationSyntax syntax, IMethodSymbol symbol, AttributeData attribute)
{
    public CommandModel Command { get; } = command;

    private IReadOnlyList<ExecutorModel>? _executors;
    public IReadOnlyList<ExecutorModel> Executors => _executors ??= Command.GetExecutors(this);

    public MethodDeclarationSyntax Syntax { get; } = syntax;

    public IMethodSymbol Symbol { get; } = symbol;

    private readonly AttributeData _attribute = attribute;

    // Data

    public InputParameterType InputParameterType { get; private set; }

    private Optional<string?> _commandName;
    public string? CommandName
    {
        get {
            if (!_commandName.HasValue) {
                _commandName = _attribute.GetConstructorArgument<string>(Literals.ExecutionAttribute_CommandName_CtorParameterIndex);
            }
            return _commandName.Value;
        }
    }

    public Filter ValidateParameter_SetInputParameterType()
    {
        if (Symbol.Parameters is not [{ Type: var parameterType }]) {
            return Filter.CreateDiagnostic(DiagnosticFactory.Create(
                DiagnosticDescriptors.ExecutionMethodOnlyHasOneParameter,
                Syntax.Identifier));
        }

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
        if (Symbol.ReturnsVoid)
            return Filter.Success;

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

        return Filter.CreateDiagnostic(DiagnosticFactory.Create(
            DiagnosticDescriptors.CommandPrefixCannotContainsSpaceOrLeadingWithMinus,
            Syntax.Identifier));
    }

    public Filter ValidateExecutorsCommandPrefixes()
    {
        List<Diagnostic>? result = null;
        var tmp = Executors.SelectMany(e => e.CommandPrefixes, (executor, cmdPrefix) => (executor, cmdPrefix)).ToList();

        for (int i = 0; i < tmp.Count; i++) {
            var (formerExecutor, formerCmdPrefixes) = tmp[i];
            for (int j = i + 1; j < tmp.Count; j++) {
                var (latterExecutor, latterCmdPrefixes) = tmp[j];
                if (RepeatOfTruncate(formerCmdPrefixes, latterCmdPrefixes)) {
                    (result ??= []).Add(DiagnosticFactory.Create(
                        DiagnosticDescriptors.ExecutorCommandPrefixRepeatOrTruncate_1,
                        latterExecutor.Syntax.Identifier,
                        formerExecutor.Symbol.Name));
                }
            }
        }

        if (result != null)
            return Filter.CreateDiagnostic(result);
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
