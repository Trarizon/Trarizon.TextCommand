﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Trarizon.TextCommand.Attributes;
using Trarizon.TextCommand.Attributes.Parameters;
using Trarizon.TextCommand.Input;
using Trarizon.TextCommand.Parsers;

namespace Trarizon.TextCommand.Tester;

internal partial class _Design
{
    [Execution("/ghoti")]
    public partial void Run(string customInput);

    private bool MaunallyRun(string input)
    {
        var matcher = new StringInputMatcher(input);
        switch (input.SplitAsArgs()) {
            case ["/ghoti", "no-param", ..]:
                return NoParam();
            case ["/ghoti", "a", .. var rest]:
                var provider_a = default(StringArgsProvider);
            __B_Label:
                var str = "--opt";
                return Method(
                    provider_a.GetOption<string, DelegateParser<string>>("--opt", new(Par), false));
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

        return default;

    }

    [Executor("multi", "mark", "no", "param")]
    [Executor("no-param")]
    public bool NoParam()
    {
        Console.WriteLine("NoParam");
        return false;
    }

    public static bool Par(ReadOnlySpan<char> input, out string output)
    {
        output = input.ToString();
        return default;
    }

    [Executor("def")]
    [Executor("multi", "marked")]
    public bool Method([Option(Parser = nameof(Par))] string? opt)
    {
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
    public bool ExplicitParameterType([Flag("f")] bool flag, [Option("nf")] bool nonFlag, [MultiValue(2)] Span<Option> options, [Value(Required = true)] string? str, [Option] int number, [MultiValue] Span<int?> rest)
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
