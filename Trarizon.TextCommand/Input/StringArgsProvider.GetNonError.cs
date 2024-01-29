using System.Runtime.InteropServices;
using Trarizon.TextCommand.Exceptions;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.TextCommand.Input;
// TODO: Remove this file
partial struct StringArgsProvider
{
    public T? GetOption<T, TParser>(string key, TParser parser, bool throwIfNotSet) where TParser : IArgParser<T>
    {
        if (TryGetRawOptionIndex(key, out var argIndex)) {
            if (TryParseArg<T, TParser>(argIndex, parser, out var result)) {
                return result;
            }
            else {
                ParseException.Throw<T>(key);
                return default;
            }
        }
        else if (throwIfNotSet) {
            ValueNotSetException.Throw(key);
            return default;
        }
        else
            return default;
    }

    public Span<T> GetValues<T, TParser>(int startIndex, Span<T> resultSpan, TParser parser, string paramName, bool throwIfNotSet) where TParser : IArgParser<T>
    {
        if (TryGetRawValuesIndices(startIndex, GetAvailableArrayLength(startIndex, resultSpan.Length), out var argIndices)) {
            var errIndex = TryParseArgs(argIndices, parser, resultSpan);
            if (errIndex == -1) {
                return resultSpan[..argIndices.Length];
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
        if (TryGetRawValuesIndices(startIndex, GetAvailableArrayLength(startIndex, maxLength), out var argIndices)) {
            T[] array = new T[argIndices.Length];
            var errIndex = TryParseArgs(argIndices, parser, array.AsSpan());
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
        if (TryGetRawValuesIndices(startIndex, GetAvailableArrayLength(startIndex, maxLength), out var argIndices)) {
            List<T> list = new(argIndices.Length);
            var errIndex = TryParseArgs(argIndices, parser, CollectionsMarshal.AsSpan(list));
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
        if (TryGetRawValuesIndices(index, 1, out var argIndices)) {
            if (TryParseArg<T, TParser>(argIndices[0], parser, out var result)) {
                return result;
            }
            else {
                ParseException.Throw<T>(paramName);
                return default;
            }
        }
        else if (throwIfNotSet) {
            ValueNotSetException.Throw(paramName);
            return default;
        }
        else
            return default;
    }
}
