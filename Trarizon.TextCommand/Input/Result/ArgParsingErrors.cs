using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Trarizon.TextCommand.Exceptions;
using Trarizon.TextCommand.Utilities;

namespace Trarizon.TextCommand.Input.Result;
/// <summary>
/// A collection of errors on parsing
/// </summary>
public readonly ref struct ArgParsingErrors
{
    // Source when string input
    // default when array input
    private readonly ReadOnlySpan<char> _sourceString;
    // unescapeds when string input
    // source when array input
    private readonly ReadOnlySpan<string> _sourceArray;
    private readonly ReadOnlySpan<ArgParsingError> _errors;

    internal ArgParsingErrors(
        ReadOnlySpan<char> sourceString, ReadOnlySpan<string> sourceArray,
        ReadOnlySpan<ArgParsingError> errors)
    {
        _sourceString = sourceString;
        _sourceArray = sourceArray;
        _errors = errors;
    }

    /// <summary>
    /// Count of errors
    /// </summary>
    public int Count => _errors.Length;

    /// <summary>
    /// Get error by index
    /// </summary>
    public ErrorData this[int index]
    {
        get {
            ref readonly var error = ref _errors[index];
            if (error._rawInfo._argIndex.Kind == ArgIndexKind.Slice) {
                var (start, length) = error._rawInfo._argIndex.SliceRange;
                return new(_sourceString.Slice(start, length), error);
            }
            else {
                return new(_sourceArray[error._rawInfo._argIndex.CachedIndex], in error);
            }
        }
    }

    /// <summary>
    /// Get enumerator
    /// </summary>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>
    /// Data context of an error
    /// </summary>
    public ref struct ErrorData
    {
        private InputArg _input;
        private readonly ref readonly ArgParsingError _error;

        internal ErrorData(ReadOnlySpan<char> sourceSpan, in ArgParsingError error)
        {
            _input = new(sourceSpan);
            _error = ref error;
        }

        internal ErrorData(string sourceString, in ArgParsingError error)
        {
            _input = new(sourceString);
            _error = ref error;
        }

        /// <summary>
        /// Raw input of this argument
        /// </summary>
        public readonly InputArg RawInputArg => _input;

        /// <summary>
        /// Raw string input of this argument
        /// </summary>
        public string RawInput => _input.RawInput;

        /// <summary>
        /// Raw span input of this argument
        /// </summary>
        public readonly ReadOnlySpan<char> RawInputSpan => _input.RawInputSpan;

        /// <summary>
        /// Error kind
        /// </summary>
        public readonly ArgResultKind ErrorKind => _error._rawInfo._kind;

        /// <summary>
        /// The result type
        /// </summary>
        public readonly Type ResultType => _error._resultType;

        /// <summary>
        /// Command parameter, this is not the parameter in executor method signature
        /// </summary>
        public readonly string ParameterName => _error._parameterName;
    }

    /// <summary>
    /// Enumerator of <see cref="ArgParsingErrors"/>
    /// </summary>
    public ref struct Enumerator
    {
        private readonly ArgParsingErrors _errors;
        private int _index;

        internal Enumerator(scoped in ArgParsingErrors errors)
        {
            _errors = errors;
            _index = -1;
        }

        /// <summary>
        /// Current
        /// </summary>
        public readonly ErrorData Current => _errors[_index];

        /// <summary>
        /// Move Next
        /// </summary>
        public bool MoveNext()
        {
            var index = _index + 1;
            if (index < _errors.Count) {
                _index = index;
                return true;
            }
            else {
                return false;
            }
        }
    }

    /// <summary>
    /// Errors Builder, do not use <c>default</c> to create it
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public ref struct Builder()
    {
        private AllocOptList<ArgParsingError> _errors = new();

        /// <summary>
        /// Indicate if the current Builder contains any error
        /// </summary>
        [MemberNotNullWhen(true, nameof(_errors))]
        public readonly bool HasError => _errors.Count > 0;

        private void AddWhenErrorInternal(ArgRawResultInfo rawResultInfo, Type resultType, string parameterName, ArgResultKind minErrorLevel)
        {
            if (rawResultInfo._kind >= minErrorLevel) {
                _errors.Add(new ArgParsingError(rawResultInfo, resultType, parameterName));
            }
        }

        /// <summary>
        /// Add when result contains error
        /// </summary>
        /// <typeparam name="T">The type parser returns</typeparam>
        /// <param name="result">result struct</param>
        /// <param name="parameterName">Command parameter name</param>
        /// <param name="minErrorLevel">
        /// The min level to occur error,
        /// if <paramref name="result"/> contains error that has same or higher error level,
        /// the error is added.
        /// </param>
        public void AddWhenError<T>(ArgResult<T> result, string parameterName, ArgResultKind minErrorLevel)
        {
            AddWhenErrorInternal(result._rawResultInfo, typeof(T), parameterName, minErrorLevel);
        }

        /// <summary>
        /// Add when result contains errors
        /// </summary>
        /// <typeparam name="T">The type parser returns</typeparam>
        /// <param name="results">result struct</param>
        /// <param name="parameterName">Command parameter name</param>
        /// <param name="minErrorLevel">
        /// The min level to occur error,
        /// if <paramref name="results"/> contains error that has same or higher error level,
        /// the error is added.
        /// </param>
        public void AddWhenError<T>(scoped ArgResultsArray<T> results, string parameterName, ArgResultKind minErrorLevel)
        {
            foreach (var info in results.RawInfos) {
                AddWhenErrorInternal(info, typeof(T), parameterName, minErrorLevel);
            }
        }

        /// <summary>
        /// Add when result contains errors
        /// </summary>
        /// <typeparam name="T">The type parser returns</typeparam>
        /// <param name="results">result struct</param>
        /// <param name="parameterName">Command parameter name</param>
        /// <param name="minErrorLevel">
        /// The min level to occur error,
        /// if <paramref name="results"/> contains error that has same or higher error level,
        /// the error is added.
        /// </param>
        public void AddWhenError<T>(scoped ArgResultsList<T> results, string parameterName, ArgResultKind minErrorLevel)
        {
            foreach (var info in results.RawInfos) {
                AddWhenErrorInternal(info, typeof(T), parameterName, minErrorLevel);
            }
        }

        /// <summary>
        /// Add when result contains errors
        /// </summary>
        /// <typeparam name="T">The type parser returns</typeparam>
        /// <param name="results">result struct</param>
        /// <param name="parameterName">Command parameter name</param>
        /// <param name="minErrorLevel">
        /// The min level to occur error,
        /// if <paramref name="results"/> contains error that has same or higher error level,
        /// the error is added.
        /// </param>
        public void AddWhenError<T>(scoped ArgResultsUnmanaged<T> results, string parameterName, ArgResultKind minErrorLevel)
        {
            foreach (var info in results.RawInfos) {
                AddWhenErrorInternal(info, typeof(T), parameterName, minErrorLevel);
            }
        }

        /// <summary>
        /// Build the <see cref="ArgParsingErrors"/>
        /// </summary>
        /// <param name="provider">The provider creating ArgResults</param>
        /// <returns></returns>
        public readonly ArgParsingErrors Build(in ArgsProvider provider)
            => new(provider._sourceInput, provider._sourceArray, _errors.AsSpan());

        /// <summary>
        /// The default error handler
        /// </summary>
        /// <remarks>
        /// This handler throw exception with first error.
        /// </remarks>
        public readonly void DefaultErrorHandler()
        {
            if (HasError) {
                var err = _errors[0];
                switch (err._rawInfo._kind) {
                    case ArgResultKind.ParameterNotSet:
                        ValueNotSetException.Throw(err._parameterName);
                        break;
                    case ArgResultKind.ParsingFailed:
                        ParseException.Throw(err._parameterName, err._resultType);
                        break;
                }
            }
        }
    }
}
