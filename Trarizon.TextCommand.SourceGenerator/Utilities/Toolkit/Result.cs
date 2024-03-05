using System;
using System.Diagnostics.CodeAnalysis;

namespace Trarizon.TextCommand.SourceGenerator.Utilities.Toolkit;
internal readonly struct Result<T, TError> where TError : class
{
    private readonly T? _value;
    private readonly TError? _error;

    [MemberNotNullWhen(true, nameof(_value), nameof(Value))]
    [MemberNotNullWhen(false, nameof(_error), nameof(Error))]
    public readonly bool Success => _error is null;

    public readonly T Value => _value!;

    [NotNull]
    public readonly TError Error => _error!;

    private Result(T? value, TError? error) { _value = value; _error = error; }

    public Result(T value) : this(value, null) { }

    public Result(TError? error) : this(default, error) { }

    public static implicit operator Result<T, TError>(T value) => new(value, null);
    public static implicit operator Result<T, TError>(TError error) => new(default, error);

    [MemberNotNullWhen(true, nameof(Value)), MemberNotNullWhen(false, nameof(Error))]
    public bool TryGetValue([MaybeNullWhen(false)] out T value, [MaybeNullWhen(true)] out TError error)
    {
        value = _value;
        error = _error;
        return Success;
    }

    public Result<TResult, TError> Select<TResult>(Func<T, TResult> selector)
        => Success ? new(selector(_value)) : new(_error);

    public Result<TResult, TError> SelectWrapped<TResult>(Func<T, Result<TResult, TError>> selector)
        => Success ? selector(_value) : new(_error);

    public (T? Value, TError? error) ToTuple() => (_value, _error);
}
