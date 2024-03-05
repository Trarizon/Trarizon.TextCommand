# Trarizon.TextCommand([CN](./README.CN.md))

For CLI-like input parsing, written with source generator.

**The library is still under designing and developing, and writing test is such a hassle, so be careful**

## Use

(An example [here](./Trarizon.TextCommand.Tester/_Design.cs))
or [here in another repo](https://github.com/Trarizon/DeemoToolkit/blob/master/Trarizon.Toolkit.Deemo.Commands/Functions/ChartHandler.Execution.cs)

For Type:
``` csharp
partial class Command
{
    [Execution("/cmd")]
    public partial bool Run(string input);

    [Executor("foo", "bar")]
    bool FooBar() => throw null!;

    [Executor("ghoti")]
    bool Ghoti(
        [Flag("f")] bool flag,
        [Option("op")] string? option,
        [Value] int value,
        [MultiValue(5)] ReadOnlySpan<int> values)
        => throw null!;
}
```

The following code will be generated (simplified):

The generated code split `input` into string array, match leading words with [list pattern](https://learn.microsoft.com/zh-cn/dotnet/csharp/fundamentals/functional/pattern-matching#list-patterns), 
and the rest parts are using for argument parsing.
``` csharp
partial class Command
{
    public partial bool Run(string input)
    {
        switch (input.SplitByWhiteSpaces()) {
            case ["/cmd", "foo", "bar", ..]:
                return this.FooBar();
            case ["/cmd", "ghoti", .. var rest]:
                var provider = ParameterSets.Ghoti.Parse(rest);
                return Ghoti(
                    provider.GetFlag<bool, BooleanFlagParser>("--flag", parser: default),
                    provider.GetOption<string, ParsableParser<string>>("--option", parser: default, false),
                    provider.GetValue<int, ParsableParser<int>>(0, parser: default, null),
                    provider.GetValues<int, ParsableParser<int>>(0, stackalloc int[5], parser: default, "values", false));
        }
        return default;
    }
}

file static class ParameterSets
{
    public static readonly ParameterSet Ghoti = new();
}
```

You can set custom parser by set properties on attribute.

### Rule

- Current a type can only contains one `Execution`, multi-execution may be supported in the future.
- `Execution` is a single-parameter method, the parameter type can be `string`, `ReadOnlySpan<char>`, `Span<string>`, `ReadOnlySpan<string>`, `string[]`, `List<string>`.
    - When input is `string` or `ReadOnlySpan<char>`, the input will be split with white spaces.
- Use `""` to escape `"` in raw string.

- You can mark multiple `[Executor]` on a method

## API

Attribute|Comments
:-:|:--
`Execution`|Extrance of execution, a single-parameter method
`Executor`|Branches of execution
`Flag`|Flag parameter, pass a `bool` value to parser accoding to input
`Option`|Named option parameter,pass a `ReadOnlySpan<char>` value to parser
`Value`|Positional parameter, parse all unmatched arguments in order
`MultiValue`|Array of positional parameter, 

Attribute params|Comments
:-:|:--
`Parser`<br/>`ParserType`|Use one of these. Parser of parameter, See rules [below](#parser-rules)
`Alias`|*Constructor param.* Alias of parameter, use with prefix `-`
`Name`|Name of parameer, default is the name of parameter
`Required`|Default is `false`. Exceptions threw when not existing if `true`
`MaxCount`|*Constructor param.* Max value count in `MultiValue`, all rest `Value`s if non-positive

### Parser Rules

Custom parsers should be *field*, *property* or *method* in current type. Or any value type implements required interfaces.

For `flag`, parser implements `IArgFlagParser<T>`; for others, implements `IArgParser<T>`.
Method parser should be implicit converted to `ArgParsingDelegate` or `ArgFlagParsingDelegate`

Build-in parser:
- `ParsableParser<T>` : Parse `ISpanParsable<T>` 
- `EnumParser<T>` : Parse enum type
- `BooleanFlagParser` : Parse `Flag` to `bool`
- `BinaryFlagParser<T>` : Parse `Flag` to one of specified values.
- `DelegateParser<T>` : Wrap a parser delegate
- `DelegateFlagParser<T>` : Wrap a parser delegate
- `NullableParser<TParser, T>` : Wrap parser for `Nullable<T>`

### Error Handling

Custom error handler should be *method* in current type, and match following rules:
- Return type of current type should be void or implicit converted to return type of execution method.
- First parameter is `ArgResultErrors`, could be marked with `in`
- Second parameter is optional, with type `string`, means the executor method name where the error comes.
