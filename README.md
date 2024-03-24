# Trarizon.TextCommand([CN](./README.CN.md))

For CLI-like input parsing, written with source generator.

**The library is still under designing and developing, and writing test is such a hassle, so be careful**

## Use

(My test examples [here](./Trarizon.TextCommand.Tester/_Design.cs))
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

The generated code split `input` into string array, match leading words with [list pattern](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/pattern-matching#list-patterns), 
and the rest parts are using for argument parsing.
``` csharp
partial class Command
{
    public partial bool Run(string input)
    {
        switch (input.SplitByWhiteSpaces())
        {
            case ["/cmd", "foo", "bar", ..]:
                return this.FooBar();
            case ["/cmd", "ghoti", .. var rest]:
                var provider = ParsingContextProvider.Ghoti.Parse(rest);
                var errorBuilder = new();

                var arg0 = provider.GetFlag<bool, FlagParser>("--flag", parser: default);
                errorBuilder.AddWhenError(arg0);
                var arg1 = provider.GetOption<string, Parser>("--option", parser: default);
                errorBuilder.AddWhenError(arg1);
                var arg2 = provider.GetValue<int, Parser>(0, parser: default);
                errorBuilder.AddWhenError(arg2);
                var arg3 = provider.GetValues<int, Parser>(0, parser: default, stackalloc[5]));
                errorBuilder.AddWhenError(arg3);

                if (errorBuilder.HasError)
                    return errorBuilder.DefaultErrorHandler();
                else
                    return Ghoti(arg0, arg1, arg2, arg3);
        }
        return default;
    }
}

file static class ParsingContextProvider
{
    public static readonly ParsingContext Ghoti = new();
}
```

You can set custom parser by set properties on attribute.

### Rule

- Only single `Execution` method in a type, which accept single parameter in `string`, `ReadOnlySpan<char>`
    - If input is `string` or `ReadOnlySpan<char>`, the input will be split with white spaces.
- Use `""` escape `"`
- You can mark multiple `[Executor]` on a method
- Custom parser is allowed. Default parser are provided for common types, see [Parsing](#Parsing)
- Custom error handler is allowes. See [ErrorHandler](#ErrorHandler)

## API

Attribute|Comments
:-:|:--
`Execution`|Extrance of execution, a single-parameter method
`Executor`|Branches of execution
`Flag`|Flag parameter, pass a `bool` value to parser accoding to input
`Option`|Named option parameter,pass a `ReadOnlySpan<char>` value to parser
`Value`|Positional parameter, parse all unmatched arguments in order
`MultiValue`|Collection of positional parameters
`ContextParameter`|Get the parameter passed to execution method, can be marked with ref keywords

Attribute params|Comments
:-:|:--
`ErrorHandler`|Custom error handling. See [ErrorHandler](#ErrorHandler)
`Parser`<br/>`ParserType`|Use one of these. Parser of parameter. See [Parsing](#Parsing)
`Alias`|*Constructor param.* Alias of parameter, use with prefix `-`
`Name`|Name of parameer, default is the name of parameter
`Required`|Default is `false`. Exceptions threw when not existing if `true`
`MaxCount`|*Constructor param.* Max value count in `MultiValue`, all rest `Value`s if non-positive

### Parsing

#### Default Parsers

- `bool` is `flag` by default, using `BooleanFlagParser`
- `ISpanParsable<T>` use `ParsableParser<T>` default
- `enum` types use `EnumParser<TEnum>` default
- For `Nullable<T>`, if there's default or provided parser for `T`, use `NullableParser<T, TParser>` to wrap
- For any type `T`, if there's provided parser for `T2`, and `T2` can implicit convert into `T`, use `Conversion<T2, T, TParser>` to wrap
- For custom method parsers, use `DelegateParser<T>` or `DelagateFlagParser<T>` to wrap
- For custom type parser `TParser`, use `default(TParser)`

### Custom parser

Custom parsers should be *field*, *property* or *method* in current type.
Or any value type implements required interfaces.

- For `flag`, parser type implements `IArgFlagParser<T>`, method parser should match `T Parse(bool)`
- For others, parser type implements `IArgParser<T>`, method parser should match `bool TryParse(InputArg|string|ReadOnlySpan<char>, out T)`

Build-in parser:
- `ParsableParser<T>` : Parse `ISpanParsable<T>` 
- `EnumParser<T>` : Parse enum type
- `BooleanFlagParser` : Parse `Flag` to `bool`
- `BinaryFlagParser<T>` : Parse `Flag` to one of specified values.
- `DelegateParser<T>` : Wrap a parser delegate
- `DelegateFlagParser<T>` : Wrap a parser delegate
- `Wrapped.NullableParser<T, TParser>` : Wrap parser for `Nullable<T>`
- `Wrapped.ConversionParser<T, TResult, TParser>`: Convert result of parser into another type 

### ErrorHandler

By default, we use `ArgResultErrors.Builder.DefaultErrorHandler()`

Custom error handler should be *method* in current type, and match following rules:
- Return type of current type should be void or implicit converted to return type of execution method.
- First parameter is `ArgResultErrors`, could be marked with `in`
- Second parameter is optional, with type `string`, means the executor method name where the error comes.
