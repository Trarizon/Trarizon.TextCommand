using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;

namespace Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
internal sealed class OptionParameterModel(ParameterModel parameter) : ICLParameterModel
{
    public ParameterModel Parameter => parameter;

    public string? Alias { get; init; }

    private string? _name;
    public string Name
    {
        get => _name ??= parameter.Symbol.Name;
        set => _name = value;
    }

    public bool Required { get; init; }

    public required Either<ImplicitCLParameterKind, (ITypeSymbol Type, ISymbol Member)> ParserInfo { get; init; }

    public TypeSyntax ParsedTypeSyntax => Parameter.Syntax.Type!;

    public ITypeSymbol ParsedTypeSymbol => Parameter.Symbol.Type;
}
