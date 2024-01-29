using System.Runtime.InteropServices;
using Trarizon.TextCommand.Exceptions;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.TextCommand.Input;
partial struct ArrayArgsProvider
{
    public T? GetOption<T, TParser>(string key, TParser parser, bool throwIfNotSet) where TParser : IArgParser<T>
    {
        if (TryGetRawOption(key, out var rawArg)) {
            if (TryParseArg<T, TParser>(rawArg, parser, out var result)) {
                return result;
            }
            else {
                ParseException.Throw<T>(key);
            }
        }
        else if (throwIfNotSet) {
            ValueNotSetException.Throw(key);
            return default;
        }
        return default;
    }

    public T GetFlag<T, TParser>(string key, TParser parser) where TParser : IArgFlagParser<T>
    {
        return parser.Parse(GetRawFlag(key));
    }


    public Span<T> GetValues<T, TParser>(int startIndex, Span<T> resultSpan, TParser parser, string paramName, bool throwIfNotSet) where TParser : IArgParser<T>
    {
        if (TryGetRawValues(startIndex, resultSpan.Length, out var rawArgs)) {
            var errIndex = TryParseArgs(rawArgs, parser, resultSpan);
            if (errIndex == -1) {
                return resultSpan[..rawArgs.Length];
            }
            else {
                ParseException.Throw<T>($"{paramName}[{errIndex}]");
                return default;
            }
        }
        else if (throwIfNotSet) {
            ValueNotSetException.Throw(paramName);
            return default;
        }
        else
            return [];
    }

    public T[] GetValuesArray<T, TParser>(int startIndex, int maxLength, TParser parser, string paramName, bool throwIfNotSet) where TParser : IArgParser<T>
    {
        if (TryGetRawValues(startIndex, GetAvailableArrayLength(startIndex, maxLength), out var rawArgs)) {
            T[] array = new T[rawArgs.Length];
            var errIndex = TryParseArgs(rawArgs, parser, array.AsSpan());
            if (errIndex == -1) {
                return array;
            }
            else {
                ParseException.Throw<T>($"{paramName}[{errIndex}]");
                return default;
            }
        }
        else if (throwIfNotSet) {
            ValueNotSetException.Throw(paramName);
            return default;
        }
        else
            return [];
    }

    public List<T> GetValuesList<T, TParser>(int startIndex, int maxLength, TParser parser, string paramName, bool throwIfNotSet) where TParser : IArgParser<T>
    {
        if (TryGetRawValues(startIndex, GetAvailableArrayLength(startIndex, maxLength), out var rawArgs)) {
            List<T> list = new(rawArgs.Length);
            var errIndex = TryParseArgs(rawArgs, parser, CollectionsMarshal.AsSpan(list));
            if (errIndex == -1) {
                return list;
            }
            else {
                ParseException.Throw<T>($"{paramName}[{errIndex}]");
                return default;
            }
        }
        else if (throwIfNotSet) {
            ValueNotSetException.Throw(paramName);
            return default;
        }
        else
            return [];
    }

    public T? GetValue<T, TParser>(int index, TParser parser, string paramName, bool throwIfNotSet) where TParser : IArgParser<T>
    {
        if (TryGetRawValues(index, 1, out var rawArgs)) {
            if (TryParseArg<T, TParser>(rawArgs[0], parser, out var result)) {
                return result;
            }
            else {
                ParseException.Throw<T>(paramName);
            }
        }
        else if (throwIfNotSet) {
            ValueNotSetException.Throw(paramName);
            return default;
        }
        return default;
    }
}
