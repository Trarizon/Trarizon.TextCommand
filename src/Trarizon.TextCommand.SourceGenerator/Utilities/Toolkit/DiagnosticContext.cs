using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;
internal struct DiagnosticContext<T>
{
    private readonly IEnumerable<T> _values;
    private bool _success;
    private List<Diagnostic> _diagnostics;

    private DiagnosticContext(IEnumerable<T> values, bool success, List<Diagnostic> diagnostics)
    {
        _values = values;
        _success = success;
        _diagnostics = diagnostics;
    }

    public DiagnosticContext(params T[] values) :
        this(values, true, [])
    { }

    public readonly bool IsClosed => !_success;

    public IEnumerable<Diagnostic> Diagnostics => _diagnostics;

    public IEnumerable<T> Values => _values;

    public DiagnosticContext<T> Validate(Func<T, Diagnostic?> validation)
    {
        foreach (var value in _values) {
            Diagnostic? diag;
            try {
                diag = validation(value);
            } catch (Exception) {
                continue;
            }
            if (diag is not null)
                _diagnostics.Add(diag);
        }
        return this;
    }

    public DiagnosticContext<T> Validate(Func<T, IEnumerable<Diagnostic>> validation)
    {
        foreach (var value in _values) {
            IEnumerable<Diagnostic> diags;
            try {
                diags = validation(value);
            } catch (Exception) {
                continue;
            }
            _diagnostics.AddRange(diags);
        }
        return this;
    }

    public readonly DiagnosticContext<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        return new(_values.Select(selector), _success, _diagnostics);
    }

    public readonly DiagnosticContext<TResult> SelectMany<TResult>(Func<T, IEnumerable<TResult>> selector)
    {
        return new(_values.SelectMany(selector), _success, _diagnostics);
    }
}
