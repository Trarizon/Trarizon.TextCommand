using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Trarizon.TextCommand.Attributes;
using Trarizon.TextCommand.Attributes.Parameters;
using Trarizon.TextCommand.Input;
using Trarizon.TextCommand.Input.Result;
using Trarizon.TextCommand.Parsers;

using TRtn = string;

namespace Trarizon.TextCommand.Tester;


static class PSet
{
    public static readonly ParsingContext Set = new(default, default);
}

internal partial class _Design
{
    [Execution("/ghoti", ErrorHandler = nameof(ErrorHandler))]
    public partial string? Run(string customInput);

    //[Execution("/ghoti", ErrorHandler = "")]
    private TRtn MaunallyRun(string input)
    {
        var matcher = new StringInputMatcher(input);
        switch (matcher) {
            case ["/ghoti", "no-param", ..]:
                return NoParam();
            case ["/ghoti", "a", .. var rest]:
                var provider_a = default(ArgsProvider);
            __B_Label:
                // var str = "--opt";
                var builder = new ArgParsingErrors.Builder();

                var optRes = provider_a.GetOption<string, DelegateParser<string>>("param", new(ParseMethod));
                builder.AddWhenError(optRes, "param", ArgResultKind.ParameterNotSet);

                var anoRes = provider_a.GetValuesUnmanaged<int, ParsableParser<int>>(0, default, stackalloc ArgResult<int>[2]);
                builder.AddWhenError(anoRes, "ano", ArgResultKind.ParameterNotSet);

                if (builder.HasError)
                    return ErrorHandler(builder.Build(provider_a), "Method");
                else
                    return Method(optRes.Value, anoRes.Values);
            case ["/ghoti", "b", .. var rest1]:
                provider_a = default(ArgsProvider);

                goto __B_Label;
            case ["multi-flag"]:
                // Deprecate，多个bool请（
                var provider2 = default(ArgsProvider)!;

                var arg1 = provider2.GetFlag<Option, BinaryFlagParser<Option>>("a", default);
                var arg2 = provider2.GetFlag<Option, BinaryFlagParser<Option>>("a", default);
                var arg = CombineOption([arg1, arg2]);
                break;
        }
        return default!;
        TRtn Method(string? str, ReadOnlySpan<int> span) => default!;

        Option CombineOption(ReadOnlySpan<Option> options)
        {
            Option rtn = default;
            foreach (var opt in options) {
                rtn |= opt;
            }
            return rtn;
        }
    }

    enum MultiFlag { None, A, B }

    private static TRtn ErrorHandler(in ArgParsingErrors errors, string methodName)
    {
        Console.WriteLine($"Errors in {methodName}");
        foreach (var err in errors) {
            Print(err.ErrorKind);
            Print(err.RawInput);
            Print(err.RawInputSpan.ToString());
            Print(err.ParameterName);
            Print(err.ResultType);
        }
        return default!;
    }


    [Executor("multi", "mark", "no", "param")]
    [Executor("no-param")]
    public TRtn NoParam()
    {
        Console.WriteLine("NoParam");
        return default!;
    }

    [Executor("value-only")]
    public TRtn ValueOnly([Value] int a)
    {
        Print(a);
        return default!;
    }

    // 默认选项
    [Executor("default", "settings")]
    [Executor("multi", "marked")]
    public TRtn DefaultSetting(
        bool flag, string? str,
        Option option, int number, int? nullNumber)
    {
        Print(flag);
        Print(str);
        Print(option);
        Print(number);
        Print(nullNumber);
        return default!;
    }

    public struct A<T>
    {
        private T _val;
        public static implicit operator A<T>(T a) => new() { _val = a };
        public override readonly string ToString() => $"A {{{_val}}}";
    }

    bool TryParseTuple(InputArg input, out A<(int, int)> res)
    {
        res = default;
        return true;
    }
    bool TryParseSpanTuple(ReadOnlySpan<char> input, out A<(int, int)> res)
    {
        res = default;
        return true;
    }
    bool TryParseStringTuple(string input, out A<(int, int)> res)
    {
        res = default;
        return true;
    }

    [Executor("implicit-conversion")]
    public TRtn ImplicitConversion(
        [Option(Parser = nameof(TryParseTuple))] A<(int A, int B)>? tuple,
        int? nullable,
        string? nullableString,
        [Option(ParserType = typeof(ParsableParser<int>))] A<int> intToA,
        [Option(ParserType = typeof(ParsableParser<int>))] long intToLong,
        [Option(ParserType = typeof(ParsableParser<int>))] A<int>? intToNullableA,
        [MultiValue(1)] int?[] nullableArr,
        [MultiValue(1)] string?[] nullableStringArr,
        [MultiValue(1, ParserType = typeof(ParsableParser<int>))] A<int>[] intToAArr,
        [MultiValue(1, ParserType = typeof(ParsableParser<int>))] long[] intToLongArr,
        [MultiValue(1, ParserType = typeof(ParsableParser<int>))] A<int>?[] intToNullableAArr)
    {
        Print(tuple);
        Print(nullable);
        Print(nullableString);
        Print(intToA);
        Print(intToLong);
        Print(intToNullableA);
        PrintArr(nullableArr);
        PrintArr(nullableStringArr);
        PrintArr(intToAArr);
        PrintArr(intToLongArr);
        PrintArr(intToNullableAArr);
        return default!;
    }

    // 显式标记名字
    [Executor("explicit", "parameter", "type")]
    public TRtn ExplicitBasicParameterType(
        // bool
        [Flag("f")] bool flag, [Option(Name = "non-flag")] bool nonFlag,
        // value option
        [Value(Required = true)] string str, [Option] int number, [Option] Option option,
        // multivalue stackalloc
        [MultiValue(2)] Span<Option> stackallocOpt,
        // multivalue ref type span
        [MultiValue(3)] Span<int?> nullableInts, [MultiValue(2)] ReadOnlySpan<string> stringSpan,
        // multivalue other type
        [MultiValue(2)] int[] intArray, [MultiValue(3)] List<int> intList, [MultiValue(3)] IEnumerable<int> intEnumerable,
        // multivalue rest values
        [MultiValue] int[] rest,
        [MultiValue(1)] int[] unreachable,
        [Value] string? unreachableValue)
    {
        Print(flag);
        Print(nonFlag);
        Print(str);
        Print(number);
        Print(option);
        PrintArr(stackallocOpt.ToArray());
        PrintArr(nullableInts.ToArray());
        PrintArr(stringSpan.ToArray());
        PrintArr(intArray);
        PrintArr(intList);
        PrintArr(rest);
        PrintArr(unreachable);
        Print(unreachableValue);
        return default!;
    }

    #region Parsers

    private static BinaryFlagParser<string> _strFlagParser = new("success", "failed");
    private static CustomParser _customParser = default;

    private readonly struct CustomParser : IArgFlagParser<int>, IArgParser<string>
    {
        public int Parse(bool flag) => flag ? 1 : 0;
        public bool TryParse(InputArg input, [MaybeNullWhen(false)] out string result)
        {
            // Repeat string twice
            var inputSpan = input.RawInputSpan;
            var span = (stackalloc char[inputSpan.Length * 2]);
            inputSpan.CopyTo(span);
            inputSpan.CopyTo(span[inputSpan.Length..]);
            result = new(span);
            return true;
        }
    }

    private static int ParseIntFlag(bool input)
    {
        return input.ToString().Length;
    }
    public static bool ParseMethod(InputArg input, out string output)
    {
        output = input.ToString().Reverse().ToArray().AsSpan().ToString();
        return true;
    }
    public bool ParseSpan(ReadOnlySpan<char> input, out string output)
    {
        output = input.ToString().Reverse().ToArray().AsSpan().ToString();
        return true;
    }
    public static bool ParseString(string input, out string output)
    {
        output = input.Reverse().ToArray().AsSpan().ToString();
        return true;
    }

    #endregion

    // 自定义其他参数
    [Executor("custom")]
    public TRtn Custom(
        // flag
        [Flag(Parser = nameof(_strFlagParser))] string? strFlag,
        [Flag(Parser = nameof(ParseIntFlag))] int intFlag,
        [Flag(ParserType = typeof(CustomParser))] int? flagTypeParser,
        // option value
        [Option(Parser = nameof(_customParser), Required = true)] string? customParser,
        [Option(Parser = nameof(ParseMethod))] string? methodParser,
        [Option(Parser = nameof(ParseSpan))] string? methodSpanParser,
        [Option(Parser = nameof(ParseString))] string? methodStringParser,
        [Value(ParserType = typeof(CustomParser), Required = true)] string typeParser,
        // multivalue
        [MultiValue(Parser = nameof(_customParser))] string?[] multiValue)
    {
        Print(strFlag);
        Print(intFlag);
        Print(flagTypeParser);
        Print(customParser);
        Print(methodParser);
        Print(methodSpanParser);
        Print(methodStringParser);
        Print(typeParser);
        PrintArr(multiValue);
        return default!;
    }

    public enum Option
    {
        A,
        B,
    }

    private static void Print<T>(T value, [CallerArgumentExpression(nameof(value))] string? arg = null)
    {
        Console.WriteLine($"{arg}: {value}");
    }

    private static void PrintArr<T>(IEnumerable<T> values, [CallerArgumentExpression(nameof(values))] string? arg = null)
    {
        Console.WriteLine($"{arg}: [{string.Join(", ", values)}]");
    }
}
