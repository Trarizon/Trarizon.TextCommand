using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;
partial struct Filter
{
    public static Filter Success => default;
    public static Filter Create(Diagnostic diagnostic) => Unsafe.As<Diagnostic, Filter>(ref diagnostic);
    public static Filter Create(IEnumerable<Diagnostic> diagnostics) => Unsafe.As<IEnumerable<Diagnostic>, Filter>(ref diagnostics);

    public static Filter<T> Create<T>(Diagnostic diagnostic) where T : notnull => new(default, diagnostic);
    public static Filter<T> Create<T>(IEnumerable<Diagnostic> diagnostics) where T : notnull => new(default, diagnostics.ToList());
    public static Filter<T> Create<T>(in T context) where T : notnull => new(context, default(List<Diagnostic>));

    public static Filter<TResult> Select<T, TResult>(in T context, Func<T, Filter<TResult>> selector) where TResult : notnull
    {
        return selector(context);
    }
}

internal readonly partial struct Filter
{
    private readonly object? _diagnostic;
    public readonly Diagnostic? Diagnostic => _diagnostic as Diagnostic;
    public readonly IEnumerable<Diagnostic>? Diagnostics => _diagnostic as IEnumerable<Diagnostic>;
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
    public readonly bool HasDiagnostic => _diagnostics != null;

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

    public static implicit operator Filter(Filter<TContext> filter) => filter._diagnostics switch {
        [var diag] => Filter.Create(diag),
        [..] => Filter.Create(filter._diagnostics),
        null => default,
    };

    public Filter<TContext> CloseIfHasDiagnostic()
    {
        if (HasDiagnostic)
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
        if (result.Diagnostic != null)
            (_diagnostics ??= []).Add(result.Diagnostic);
        else if (result.Diagnostics != null)
            (_diagnostics ??= []).AddRange(result.Diagnostics);
        return this;
    }

    public Filter<TContext> PredicateMany<T>(Func<TContext, IEnumerable<T>> selector, Func<T, Filter> predicate)
    {
        if (IsClosed)
            return this;

        foreach (var item in selector(Context)) {
            var result = predicate(item);
            if (result.Diagnostic is { } diag)
                (_diagnostics ??= []).Add(diag);
            else if (result.Diagnostics is { } diags)
                (_diagnostics ??= []).AddRange(diags);
        }

        return this;
    }

    public Filter<TResult> Select<TResult>(Func<TContext, Filter<TResult>> selector) where TResult : notnull
    {
        if (IsClosed)
            return new(default, _diagnostics);

        var result = selector(Context);
        if (result.HasDiagnostic)
            (_diagnostics ??= []).AddRange(result.Diagnostics);
        else
            return new(result.Context, _diagnostics);
        return new(default, _diagnostics);
    }
}
