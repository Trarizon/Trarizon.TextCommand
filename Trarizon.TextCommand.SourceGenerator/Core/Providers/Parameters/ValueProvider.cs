﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Trarizon.TextCommand.SourceGenerator.ConstantValues;
using Trarizon.TextCommand.SourceGenerator.Core.Models;
using Trarizon.TextCommand.SourceGenerator.Core.Models.Parameters;
using Trarizon.TextCommand.SourceGenerator.Utilities;

namespace Trarizon.TextCommand.SourceGenerator.Core.Providers.Parameters;
internal sealed class ValueProvider(ExecutorProvider executor, ValueParameterData data, int index) : ParameterProvider(executor)
{
    protected override ParameterModel Model => data.Model;

    protected override (string Identifier, ArgumentSyntax[] Arguments) GetProviderMethodInfo()
    {
        return (Literals.ArgsProvider_GetValue_MethodIdentifier, [
            SyntaxFactory.Argument(
                SyntaxProvider.LiteralInt32Expression(index)),
            ParserArgumentSyntax,
            SyntaxFactory.Argument(SyntaxProvider.LiteralStringExpression(Model.Symbol.Name)),
            SyntaxFactory.Argument(SyntaxProvider.LiteralBooleanExpression(data.Required)),
        ]);
    }
}
