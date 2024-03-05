using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.TextCommand.Attributes;
using Trarizon.TextCommand.Attributes.Parameters;
using Trarizon.TextCommand.Input;
using Trarizon.TextCommand.Input.Result;
using Trarizon.TextCommand.Parsers;

using TRtn = string;

namespace Trarizon.TextCommand.Tester;


static class PSet
{
    public static readonly ParameterSet Set = new(default, default);
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
                var provider2 = default(ArgsProvider)!;
                var arg = provider2.GetFlag<Option, BinaryFlagParser<Option>>("a", default)
                    | provider2.GetFlag<Option, BinaryFlagParser<Option>>("b", default);
                break;
        }

        return default!;

        TRtn Method(string? str, ReadOnlySpan<int> span) => default!;
    }

    private static TRtn ErrorHandler(in ArgParsingErrors errors, string methodName)
    {
        return default!;
    }

    [Executor("multi", "mark", "no", "param")]
    [Executor("no-param")]
    public TRtn NoParam()
    {
        Console.WriteLine("NoParam");
        return default!;
    }

    // 默认选项
    [Executor("default", "settings")]
    [Executor("multi", "marked")]
    public TRtn DefaultSetting(bool flag, string? str, Option option, int number, int? nullNumber)
    {
        Print(flag);
        Print(str);
        Print(option);
        Print(number);
        Print(nullNumber);
        return default!;
    }

    // 显式标记名字
    // void返回值
    [Executor("explicit", "parameter", "type")]
    public TRtn ExplicitBasicParameterType(
        // bool
        [Flag("f")] bool flag, [Option("nf")] bool nonFlag,
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
        [MultiValue(1)] int[] unreachable)
    {
        Print(flag);
        Print(nonFlag);
        Print(str);
        Print(number);
        Print(option);
        Print(stackallocOpt.ToArray());
        Print(nullableInts.ToArray());
        Print(stringSpan.ToArray());
        Print(intArray);
        Print(intList);
        Print(rest);
        Print(unreachable);
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

    #endregion

    // 自定义其他参数
    [Executor("custom")]
    public TRtn Custom(
        // flag
        [Flag(Parser = nameof(_strFlagParser))] string? strFlag,
        [Flag(Parser = nameof(ParseIntFlag))] int? intParser,
        [Flag(ParserType = typeof(CustomParser))] int? flagTypeParser,
        // option value
        [Option(Parser = nameof(_customParser), Required = true)] string? customParser,
        [Value(ParserType = typeof(CustomParser), Required = true)] string typeParser,
        // multivalue
        [MultiValue(Parser = nameof(_customParser))] string?[] multiValue)
    {
        Print(strFlag);
        Print(intParser);
        Print(flagTypeParser);
        Print(customParser);
        Print(typeParser);
        Print(multiValue);
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

    private static void Print<T>(IEnumerable<T> value, [CallerArgumentExpression(nameof(value))] string? arg = null)
    {
        Console.WriteLine($"{arg}: [{string.Join(',', value)}]");
    }
}
