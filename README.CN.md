# Trarizon.TextCommand

用于解析类命令行输入格式，使用源生成器编写。

**该库仍在设计与编写中，使用时请注意**

## 基本使用
本人测试时使用的样例可见[Design.cs]((./Trarizon.TextCommand.Tester/_Design.cs))
或我在[另一个仓库](https://github.com/Trarizon/DeemoToolkit/blob/master/Trarizon.Toolkit.Deemo.Commands/Functions/ChartHandler.Execution.cs)用到的

对于类型
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

会生成一下代码（已简化）

生成代码将通过解析`input`分割为字符串列表，利用列表模式匹配列表开头，并将剩余部分切片用于参数解析。
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

通过参数上的Attribute成员可以自定义解析器，设成员为必需等

## 简要规范

- 单个类型内只能有一个`Execution`，`Execution`参数为单个参数，可以为`string`, `ReadOnlySpan<char>`
    - 当参数为`string`或`ReadOnlySpan<char>`时会以空白字符进行分割后解析
- 使用`""`转义`"`
- 可以在一个方法上标记多个`[Executor]`
- 可自定义Parser，对基本类型提供了默认parser，见下文[Parsing](#Parsing)
- 可自定义ErrorHandler，见下文[ErrorHandler](#ErrorHandler)

## API

Attribute|注释
:-:|:--
`Execution`|方法入口，提供可选单个参数作为命令名
`Executor`|命令分支，提供`params`参数作为命令触发指令
`Flag`|flag参数，根据输入(`-f`/`--flag`)存在与否返回`true`或`false`进行解析
`Option`|命名参数，根据参数名(`-o`/`--option`)返回对应的实参进行解析
`Value`|位置参数，按照顺序解析未命名参数
`MultiValue`|位置参数数组，解析多个位置参数

Attribute参数中提供了部分设置用于自定义，

参数|注释
:-:|:--
`ErrorHandler`|自定义Error处理，详细见[下文](#Error处理)
`Parser`<br/>`ParserType`|二选一。参数的解析器，详细见[下文](#Parsing)
`Alias`|**构造函数参数**，参数别名，即用单个`-`指定的参数
`Name`|参数名，默认使用方法定义的参数名
`Required`|默认`false`，若为`true`，则解析时不存在该参数会抛异常
`MaxCount`|**构造函数参数**，`MultiValue`所含的参数的最大数量。非正数时会解析余下所有`Value`，此时会截断后续所有`Value`和`MultiValue`

### Parsing

#### 默认Parser

- `bool`默认作为flag，使用`BooleanFlagParser`
- `ISpanParsable<T>`默认使用`ParsableParser<T>`
- `enum`类型默认使用`EnumParser<TEnum>`
- 对于`Nullable<T>`，当有对`T`的Parser时，使用`NullableParser<T, TParser>`包装
- 对于任意类型`T`，如果提供了对`T2`的Parser且`T2`可隐式转换到`T`时，使用`ConversionParser<>`包装
- 对于自定义MethodParser，使用`DelegateParser<T>`或`DelegateFlagParser<T>`包装

#### 自定义Parser

自定义Parser应为类型内部的**字段**、**属性**或**方法**。
或任意实现了要求接口的**值类型**

- 对于`Flag`，parser类型需实现`IArgFlagParser<T>`，方法应符合签名`T Parse(bool)`
- 对于其他参数，需实现`IArgParser<T>`，方法应符合签名`bool TryParse(InputArg, out T)`
- 其中`T`可隐式转换为对应的参数类型

该库内置了数个parser：
- `ParsableParser<T>` : 解析实现了`ISpanParsable<T>`的类型
- `EnumParser<T>` : 解析enum类型
- `BooleanFlagParser` : 解析bool类型为`Flag`参数
- `BinaryFlagParser<T>` : 将bool类型解析为指定的两个值
- `DelegateParser<T>` : 包装一个方法进行解析
- `DelegateFlagParser<T>` : 包装一个方法进行解析
- `Wrapped.NullableParser<T, TParser>` : 提供了`Nullable<T>`解析的包装
- `Wrapped.ConversionParser<T, TResult, TParser>`：提供对parse结果的转换

### ErrorHandler

默认情况下，使用`ArgResultErrors.Builder.DefaultErrorHandler()`

自定义ErrorHandler应为类型内部的**方法**，并符合以下要求：
- 返回值为void或可隐式转换为Execution的返回值
- 第一个参数为`ArgResultErrors`类型，可标记为`in`
- 第二个参数可选，应为`string`类型，表示发生错误的`Executor`方法名
