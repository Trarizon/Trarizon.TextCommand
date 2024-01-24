using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.TextCommand.Attributes;
using Trarizon.TextCommand.Attributes.Parameters;
using Trarizon.TextCommand.Input;
using Trarizon.TextCommand.Parsers;

using TRtn = string;

namespace Trarizon.TextCommand.Tester;

internal partial class _Design
{
    [Execution("/ghoti")]
    public partial string? Run(string customInput);

    private TRtn MaunallyRun(string input)
    {
        var matcher = new StringInputMatcher(input);
        switch (input.SplitAsArgs()) {
            case ["/ghoti", "no-param", ..]:
                return NoParam();
            case ["/ghoti", "a", .. var rest]:
                var provider_a = default(StringArgsProvider);
            __B_Label:
               // var str = "--opt";
                return Method(provider_a.GetOption<string, DelegateParser<string>>("--opt", new(Par), false));
            case ["/ghoti", "b", .. var rest1]:
                provider_a = default(StringArgsProvider);
                goto __B_Label;
            case ["multi-flag"]:
                var provider2 = default(StringArgsProvider)!;
                var arg = provider2.GetFlag<Option, BinaryFlagParser<Option>>("a", default)
                    | provider2.GetFlag<Option, BinaryFlagParser<Option>>("b", default);
                break;
            default:
                break;
        }

        return default!;

        TRtn Method(string? str) => default!;
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
    public TRtn ExplicitParameterType([Flag("f")] bool flag, [Option("nf")] bool nonFlag, [MultiValue(2)] Span<Option> options, [Value(Required = true)] string? str, [Option] int number, [MultiValue] Span<int?> rest)
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
    public static bool Par(ReadOnlySpan<char> input, out string? output)
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
        public readonly bool TryParse(ReadOnlySpan<char> rawArg, [MaybeNullWhen(false)] out string result)
        {
            // Repeat string twice
            var span = (stackalloc char[rawArg.Length * 2]);
            rawArg.CopyTo(span);
            rawArg.CopyTo(span[rawArg.Length..]);
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
