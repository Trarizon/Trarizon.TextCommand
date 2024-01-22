# Trarizon.TextCommand

用于解析类命令行输入格式，使用源生成器编写。

核心功能写完了，正在修补诊断分析器

本库原设计用于聊天Bot的命令格式解析

## 基本使用
使用例可见[Design.cs]((./Trarizon.TextCommand.Tester/_Design.cs))
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
        [Values(5)] ReadOnlySpan<int> values)
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
        switch (input.SplitByWhiteSpaces()) {
            case ["/cmd", "foo", "bar", ..]:
                return this.FooBar();
            case ["/cmd", "ghoti", .. var rest]:
                var provider = ParameterSets.Ghoti.Parse(rest);
                return Ghoti(
                    provider.GetFlag<bool, BooleanFlagParser>("--flag", parser: default),
                    provider.GetOption<string, ParsableParser<string>>("--option", parser: default, false),
                    provider.GetValue<int, ParsableParser<int>>(0, parser: default, null),
                    provider.GetValues<int, ParsableParser<int>>(0, stackalloc int[5], parser: default, null));
        }
        return default;
    }
}

file static class ParameterSets
{
    public static readonly ParameterSet Ghoti = new();
}
```

通过参数上的Attribute成员可以自定义解析器，设成员为必需等

### 规范

- 当前一个类型内只能有一个`Execution`，以后可能允许多个
- `Execution`参数为单个参数，可以为`string`, `ReadOnlySpan<char>`, `Span<string>`, `ReadOnlySpan<string>`, `string[]`, `List<string>`
    - 目前其他任意允许列表模式匹配的类型都可以，但不做保证，以后有可能会加上error诊断限制，所以不推荐
    - 当参数为`string`或`ReadOnlySpan<char>`时会以空白字符进行分割后解析
- 使用`""`转义`"`

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
`Parser`|参数的解析器，详细见下文[Parser规范](#Parser规范)
`Alias`|**构造函数参数**，参数别名，即用单个`-`指定的参数
`Name`|参数名，默认使用方法定义的参数名
`Required`|默认`false`，若为`true`，则解析时不存在该参数会抛异常
`MaxCount`|**构造函数参数**，`MultiValue`所含的参数的最大数量。非正数时会解析余下所有`Value`，此时会截断后续所有`Value`和`MultiValue`

### Parser规范

自定义Parser应为类型内部的**字段**或**属性**

对于`Flag`，parser需实现`IArgFlagParser<T>`；对于其他参数，需实现`IArgParser<T>`

该库提供了数个parser作为默认行为：
- `ParsableParser<T>` : 解析实现了`ISpanParsable<T>`的类型
- `EnumParser<T>` : 解析enum类型
- `BooleanFlagParser` : 解析bool类型为`Flag`参数
- `BinaryFlagParser<T>` : 将bool类型解析为指定的两个值
- `DelegateParser<T>` : 包装一个方法进行解析
- `DelegateFlagParser<T>` : 包装一个方法进行解析
- `NullableParser<TParser, T>` : 提供了`Nullable<T>`解析的包装