using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.TextCommand.Attributes;
using Trarizon.TextCommand.Attributes.Parameters;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.TextCommand.Tester;

internal partial class _Design
{
    [Execution("/ghoti")]
    public partial bool Run(string input);

    [Executor("no-param")]
    public bool NoParam()
    {
        Console.WriteLine("NoParam");
        return false;
    }

    // Default setting
    [Executor("default", "settings")]
    public bool DefaultSetting(bool flag, string? str, Option option, int number, int? nullNumber)
    {
        Print(flag);
        Print(str);
        Print(option);
        Print(number);
        Print(nullNumber);
        return default!;
    }

    [Executor("explicit", "parameter", "type")]
    public bool ExplicitParameterType([Flag("f")] bool flag, [Option("nf")] bool nonFlag, [MultiValue(2)] Span<Option> options, [Value] string? str, [Option] int number, [MultiValue] int?[] rest)
    {
        Print(flag);
        Print(nonFlag);
        foreach (var opt in options)
            Print(opt);
        Print(str);
        Print(number);
        Print(rest.AsEnumerable());
        return default!;
    }

    private static BinaryFlagParser<string> _strFlagParser = new("success", "failed");
    private static CustomParser _customParser = default;

    [Executor("custom")]
    public bool Custom(
        [Flag(Parser = nameof(_strFlagParser))] string? strFlag,
        [Option(Parser = nameof(_customParser))] string? custom)
    {
        Print(strFlag);
        Print(custom);
        return default!;
    }
    public enum Option
    {
        A,
        B,
    }

    readonly struct CustomParser : IArgParser<string>
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
