using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.TextCommand.Attributes;
using Trarizon.TextCommand.Attributes.Parameters;
using Trarizon.TextCommand.Input;
using Trarizon.TextCommand.Input.Result;
using Trarizon.TextCommand.Parsers;

using TRtn = string;

namespace Trarizon.TextCommand.Tester;

internal class _Design
{
    // TODO: 接下来写生成器，argprovider重写了，parameterSet参数小改

    //[Execution("/ghoti")]
    public string? Run(string customInput) => default;

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
                ArgParsingErrors.Builder builder = default;

                var optRes = provider_a.GetOption<string, DelegateParser<string>>("param", new(Par));
                builder.AddWhenError(optRes, "param", ArgResultKind.ParameterNotSet);

                var anoRes = provider_a.GetValuesUnmanaged<int, ParsableParser<int>>(0, default, stackalloc ArgResult<int>[2]);
                builder.AddWhenError(anoRes, "ano", ArgResultKind.ParameterNotSet);

                if (builder.HasError)
                    return ErrorHandler(builder.Build(provider_a, "Method"));
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

    private TRtn ErrorHandler(in ArgParsingErrors errors)
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

    // Default setting
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

    [Executor("explicit", "parameter", "type")]
    public TRtn ExplicitParameterType([Flag("f")] bool flag, [Option("nf")] bool nonFlag, [MultiValue(2)] Span<Option> options,
        [Value(Required = true)] string str, [Option] int number, [MultiValue(3)] Span<int?> ints,
        [MultiValue] ReadOnlySpan<string> rest)
    {
        Print(flag);
        Print(nonFlag);
        foreach (var opt in options)
            Print(opt);
        Print(str);
        Print(number);
        // Print(rest.AsEnumerable());
        return default!;
    }

    private static BinaryFlagParser<string> _strFlagParser = new("success", "failed");
    private static BinaryFlagParser<int> _intFlagParser = new(1, -1);
    private static CustomParser _customParser = default;
    public static bool Par(InputArg input, out string output)
    {
        output = input.ToString().Reverse().ToArray().AsSpan().ToString();
        return true;
    }

    [Executor("custom")]
    public TRtn Custom(
        [Flag(Parser = nameof(_strFlagParser))] string? strFlag,
        [Flag(Parser = nameof(_intFlagParser))] int? intParser,
        [Option(Parser = nameof(_customParser), Required = true)] string? custom,
        [Option(Parser = nameof(Par), Required = true)] string methodParser)
    {
        Print(strFlag);
        Print(custom);
        Print(methodParser);
        return default!;
    }

    public enum Option
    {
        A,
        B,
    }

    readonly struct CustomParser : IArgParser<string?>
    {
        public readonly bool TryParse(InputArg input, [MaybeNullWhen(false)] out string result)
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

    private static void Print<T>(T value, [CallerArgumentExpression(nameof(value))] string? arg = null)
    {
        Console.WriteLine($"{arg}: {value}");
    }

    private static void Print<T>(IEnumerable<T> value, [CallerArgumentExpression(nameof(value))] string? arg = null)
    {
        Console.WriteLine($"{arg}: [{string.Join(',', value)}]");
    }
}
