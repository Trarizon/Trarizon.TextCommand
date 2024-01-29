using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Trarizon.TextCommand.Exceptions;

namespace Trarizon.TextCommand.Input.Parsing;
/// <summary>
/// Set of errors occured during parsing
/// </summary>
public readonly ref struct ParsingErrors
{
    private readonly ReadOnlySpan<char> _source;
    private readonly string[] _unescapeds;
    private readonly string _executorMethodName;
    private readonly ReadOnlySpan<ParsingError> _errorInfos;

    internal ParsingErrors(ReadOnlySpan<char> sourceSpan, string[] sourceUnescapeds, string executorMethodName, ReadOnlySpan<ParsingError> errorInfos)
    {
        _source = sourceSpan;
        _unescapeds = sourceUnescapeds;
        _executorMethodName = executorMethodName;
        _errorInfos = errorInfos;
    }

    /// <summary>
    /// The name of executor method
    /// </summary>
    public string ExecutorMethodName => _executorMethodName;

    /// <summary>
    /// Get error data
    /// </summary>
    public ErrorData this[int index]
    {
        get {
            ref readonly var info = ref _errorInfos[index];
            if (info._error._index.Kind == ArgIndexKind.Slice) {
                var (start, length) = info._error._index.SliceRange;
                return new(_source.Slice(start, length), in info);
            }
            else {
                return new(_unescapeds[info._error._index.CachedIndex], in info);
            }
        }
    }

    /// <summary>
    /// Count of errors
    /// </summary>
    public int Count => _errorInfos.Length;

    /// <summary>
    /// Get the enumerator 
    /// </summary>
    /// <returns>The enumerator of current error set</returns>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>
    /// Datas of an error
    /// </summary>
    public ref struct ErrorData
    {
        private readonly ReadOnlySpan<char> _sourceSpan;
        private string? _sourceString;
        private readonly ref readonly ParsingError _errorInfo;

        internal ErrorData(ReadOnlySpan<char> sourceSpan, in ParsingError errorInfo)
        {
            _sourceSpan = sourceSpan;
            _errorInfo = ref errorInfo;
        }

        internal ErrorData(string sourceString, in ParsingError errorInfo)
        {
            _sourceString = sourceString;
            _errorInfo = ref errorInfo;
        }

        /// <summary>
        /// Get the unescaped input argument in <see cref="string"/>, this may allocate new string
        /// </summary>
        public string RawString => _sourceString ??= _sourceSpan.ToString();

        /// <summary>
        /// Get the unescaped input argument in <see cref="ReadOnlySpan{T}"/>
        /// </summary>
        public readonly ReadOnlySpan<char> RawSpan => _sourceSpan.Length == 0 ? _sourceString.AsSpan() : _sourceSpan;

        /// <summary>
        /// Error kind
        /// </summary>
        public readonly ParsingErrorKind ErrorKind => _errorInfo._error._errorKind;

        /// <summary>
        /// The type of result of parser.
        /// This is not the parameter type
        /// </summary>
        public readonly Type ParsedType => _errorInfo._parsedType;

        /// <summary>
        /// The name of command line parameter name, without prefix "--".
        /// This is not the parameter name in executor method signature
        /// </summary>
        public readonly string ParameterName => _errorInfo._parameterName;
    }

    /// <summary>
    /// Enumerator of <see cref="ParsingErrors"/>
    /// </summary>
    public ref struct Enumerator
    {
        private readonly ReadOnlySpan<char> _source;
        private readonly string[] _unescapeds;
        private readonly ReadOnlySpan<ParsingError> _errorInfos;
        private int _index;

        internal Enumerator(scoped in ParsingErrors errors)
        {
            _source = errors._source;
            _unescapeds = errors._unescapeds;
            _errorInfos = errors._errorInfos;
            _index = 0;
        }

        /// <summary>
        /// Current data
        /// </summary>
        public readonly ErrorData Current
        {
            get {
                ref readonly var info = ref _errorInfos[_index];
                if (info._error._index.Kind == ArgIndexKind.Slice) {
                    var (start, length) = info._error._index.SliceRange;
                    return new(_source.Slice(start, length), in info);
                }
                else {
                    return new(_unescapeds[info._error._index.CachedIndex], in info);
                }
            }
        }

        /// <summary>
        /// Move enumerator to next
        /// </summary>
        public bool MoveNext()
        {
            var index = _index + 1;
            if (index < _errorInfos.Length) {
                _index = index;
                return true;
            }
            else {
                return false;
            }
        }
    }

    public ref struct Builder
    {
        private List<ParsingError>? _errors;

        [MemberNotNullWhen(true, nameof(_errors))]
        public readonly bool HasError => _errors != null;

        private void AddError(ParsingErrorSimple error, Type parsedType, string parameterName, ParsingErrorKind minErrorLevel)
        {
            if (error._errorKind >= minErrorLevel)
                (_errors ??= []).Add(new ParsingError(error, parsedType, parameterName));
        }

        public void Add<T>(ParsingResult<T> result, string parameterName, ParsingErrorKind minErrorLevel)
        {
            AddError(result.Error, typeof(T), parameterName, minErrorLevel);
        }

        public void Add<T>(scoped ParsingResultsArray<T> results, string parameterName, ParsingErrorKind minErrorLevel)
        {
            foreach (var err in results.Errors)
                AddError(err, typeof(T), parameterName, minErrorLevel);
        }

        public void Add<T>(scoped ParsingResultsList<T> results, string parameterName, ParsingErrorKind minErrorLevel)
        {
            foreach (var err in results.Errors)
                AddError(err, typeof(T), parameterName, minErrorLevel);
        }

        public void Add<T>(scoped ParsingResultsUnmanaged<T> results, string parameterName, ParsingErrorKind minErrorLevel) where T : unmanaged
        {
            foreach (var err in results.Errors)
                AddError(err, typeof(T), parameterName, minErrorLevel);
        }

        public ParsingErrors GetErrors(StringArgsProvider provider, string executorMethodName) => provider.GetErrors(executorMethodName, CollectionsMarshal.AsSpan(_errors));

        /// <summary>
        /// Default error handler, throw the first error if has
        /// </summary>
        public readonly void DefaultErrorHandler()
        {
            if (HasError) {
                var err = _errors[0];
                switch (err._error._errorKind) {
                    case ParsingErrorKind.ParameterNotSet:
                        ValueNotSetException.Throw(err._parameterName);
                        break;
                    case ParsingErrorKind.ParsingFailed:
                        ParseException.Throw(err._parameterName, err._parsedType);
                        break;
                }
            }
        }
    }
}