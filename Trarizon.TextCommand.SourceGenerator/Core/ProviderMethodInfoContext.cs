using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Trarizon.TextCommand.SourceGenerator.Core;
internal readonly record struct ProviderMethodInfoContext(
    string GetterMethodIdentifier,
    IEnumerable<ExpressionSyntax> ArgExpressions);
