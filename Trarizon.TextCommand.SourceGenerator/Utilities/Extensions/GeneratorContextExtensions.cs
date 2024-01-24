using Microsoft.CodeAnalysis;
using System;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit.Extensions;
internal static class GeneratorContextExtensions
{
    public static void RegisterFilteredSourceOutput<TContext>(this in IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<Filter<TContext>> filterSource,
        Action<SourceProductionContext, TContext> action) where TContext : notnull
    {
        context.RegisterSourceOutput(filterSource, (context, source) =>
        {
            foreach (var diagnostic in source.Diagnostics) {
                context.ReportDiagnostic(diagnostic);
            }
            if (!source.HasError) {
                action(context, source.Context);
            }
        });
    }
}
