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
            if (source.HasError) {
                foreach (var item in source.Diagnostics) {
                    context.ReportDiagnostic(item);
                }
            }
            else {
                action(context, source.Context);
            }
        });
    }
}
