using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;
partial struct Filter
{
    public static Filter Success => default;
    public static Filter CreateDiagnostic() => CreateDiagnostic(Array.Empty<Diagnostic>());
    public static Filter CreateDiagnostic(Diagnostic diagnostic) => CreateDiagnostic(new[] { diagnostic });
    public static Filter CreateDiagnostic(IEnumerable<Diagnostic> diagnostics) => Unsafe.As<IEnumerable<Diagnostic>, Filter>(ref diagnostics);

    public static Filter<T> CreateDiagnostic<T>(Diagnostic diagnostic) where T : notnull => new(default, diagnostic);
    public static Filter<T> CreateDiagnostic<T>(IEnumerable<Diagnostic> diagnostics) where T : notnull => new(default, diagnostics);
    public static Filter<T> Create<T>(in T context) where T : notnull => new(context, default(List<Diagnostic>));
    public static Filter<T> Create<T>(in T context, Diagnostic diagnostic) where T : notnull => new(context, diagnostic);
    public static Filter<T> Create<T>(in T context, IEnumerable<Diagnostic> diagnostics) where T : notnull => new(context, diagnostics);

    public static Filter<TResult> Select<T, TResult>(in T context, Func<T, Filter<TResult>> selector) where TResult : notnull
    {
        return selector(context);
    }
}

internal readonly partial struct Filter
{
    private readonly IEnumerable<Diagnostic>? _diagnostics;

    [MemberNotNullWhen(false, nameof(Diagnostics))]
    public bool IsSuccess => _diagnostics is null;

    public readonly IEnumerable<Diagnostic>? Diagnostics => _diagnostics;
}

internal struct Filter<TContext> where TContext : notnull
{
    private Optional<TContext> _context;
    private List<Diagnostic>? _diagnostics;

    public readonly TContext? Context => _context.Value;

    [MemberNotNullWhen(false, nameof(Context))]
    public readonly bool IsClosed => !_context.HasValue;

    public readonly IEnumerable<Diagnostic> Diagnostics => _diagnostics ?? [];

    [MemberNotNullWhen(false, nameof(Context))]
    public readonly bool HasError => _diagnostics?.Any(diag => diag.Severity is DiagnosticSeverity.Error) ?? false;

    internal Filter(Optional<TContext> context, IEnumerable<Diagnostic>? diagnostics)
    {
        _context = context;
        if (diagnostics is not null)
            _diagnostics = diagnostics is List<Diagnostic> list ? list : [.. diagnostics];
    }

    internal Filter(Optional<TContext> context, Diagnostic diagnostic)
    {
        _context = context;
        _diagnostics = [diagnostic];
    }

    private void TryAddDiagnostic(Filter filter)
    {
        if (filter.IsSuccess)
            return;

        if (_diagnostics is null)
            _diagnostics = new(filter.Diagnostics);
        else
            _diagnostics.AddRange(filter.Diagnostics);
    }

    public static implicit operator Filter(Filter<TContext> filter) => filter._diagnostics switch {
        [..] => Filter.CreateDiagnostic(filter._diagnostics),
        null => default,
    };

    public Filter<TContext> CloseIfHasError()
    {
        if (HasError)
            _context = default;
        return this;
    }

    public readonly Filter<TContext> ThrowIfCancelled(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return this;
    }

    public Filter<TContext> Predicate(Func<TContext, Filter> predicate)
    {
        if (IsClosed)
            return this;

        var result = predicate(Context!);
        TryAddDiagnostic(result);
        return this;
    }

    public Filter<TContext> PredicateMany<T>(Func<TContext, IEnumerable<T>> selector, Func<T, Filter> predicate)
    {
        if (IsClosed)
            return this;

        foreach (var item in selector(Context)) {
            var result = predicate(item);
            TryAddDiagnostic(result);
        }

        return this;
    }

    public Filter<TResult> Select<TResult>(Func<TContext, Filter<TResult>> selector) where TResult : notnull
    {
        if (IsClosed)
            return new(default, _diagnostics);

        var result = selector(Context);
        if (result.HasError)
            TryAddDiagnostic(result);
        else
            return new(result.Context, _diagnostics);
        return new(default, _diagnostics);
    }
}
