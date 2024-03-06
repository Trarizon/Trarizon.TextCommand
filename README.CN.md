# Trarizon.TextCommand

���ڽ����������������ʽ��ʹ��Դ��������д��

**�ÿ�����������д�У�ʹ��ʱ��ע��**

## ����ʹ��
���˲���ʱʹ�õ������ɼ�[Design.cs]((./Trarizon.TextCommand.Tester/_Design.cs))
������[��һ���ֿ�](https://github.com/Trarizon/DeemoToolkit/blob/master/Trarizon.Toolkit.Deemo.Commands/Functions/ChartHandler.Execution.cs)�õ���

��������
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

������һ�´��루�Ѽ򻯣�

���ɴ��뽫ͨ������`input`�ָ�Ϊ�ַ����б������б�ģʽƥ���б�ͷ������ʣ�ಿ����Ƭ���ڲ���������
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

ͨ�������ϵ�Attribute��Ա�����Զ�������������ԱΪ�����

## ��Ҫ�淶

- ����������ֻ����һ��`Execution`��`Execution`����Ϊ��������������Ϊ`string`, `ReadOnlySpan<char>`
    - ������Ϊ`string`��`ReadOnlySpan<char>`ʱ���Կհ��ַ����зָ�����
- ʹ��`""`ת��`"`
- ������һ�������ϱ�Ƕ��`[Executor]`
- ���Զ���Parser���Ի��������ṩ��Ĭ��parser��������[Parsing](#Parsing)
- ���Զ���ErrorHandler��������[ErrorHandler](#ErrorHandler)

## API

Attribute|ע��
:-:|:--
`Execution`|������ڣ��ṩ��ѡ����������Ϊ������
`Executor`|�����֧���ṩ`params`������Ϊ�����ָ��
`Flag`|flag��������������(`-f`/`--flag`)������񷵻�`true`��`false`���н���
`Option`|�������������ݲ�����(`-o`/`--option`)���ض�Ӧ��ʵ�ν��н���
`Value`|λ�ò���������˳�����δ��������
`MultiValue`|λ�ò������飬�������λ�ò���

Attribute�������ṩ�˲������������Զ��壬

����|ע��
:-:|:--
`ErrorHandler`|�Զ���Error������ϸ��[����](#Error����)
`Parser`<br/>`ParserType`|��ѡһ�������Ľ���������ϸ��[����](#Parsing)
`Alias`|**���캯������**���������������õ���`-`ָ���Ĳ���
`Name`|��������Ĭ��ʹ�÷�������Ĳ�����
`Required`|Ĭ��`false`����Ϊ`true`�������ʱ�����ڸò��������쳣
`MaxCount`|**���캯������**��`MultiValue`�����Ĳ��������������������ʱ�������������`Value`����ʱ��ضϺ�������`Value`��`MultiValue`

### Parsing

#### Ĭ��Parser

- `bool`Ĭ����Ϊflag��ʹ��`BooleanFlagParser`
- `ISpanParsable<T>`Ĭ��ʹ��`ParsableParser<T>`
- `enum`����Ĭ��ʹ��`EnumParser<TEnum>`
- ����`Nullable<T>`�����ж�`T`��Parserʱ��ʹ��`NullableParser<T, TParser>`��װ
- ������������`T`������ṩ�˶�`T2`��Parser��`T2`����ʽת����`T`ʱ��ʹ��`ConversionParser<>`��װ
- �����Զ���MethodParser��ʹ��`DelegateParser<T>`��`DelegateFlagParser<T>`��װ

#### �Զ���Parser

�Զ���ParserӦΪ�����ڲ���**�ֶ�**��**����**��**����**��
������ʵ����Ҫ��ӿڵ�**ֵ����**

- ����`Flag`��parser������ʵ��`IArgFlagParser<T>`������Ӧ����ǩ��`T Parse(bool)`
- ����������������ʵ��`IArgParser<T>`������Ӧ����ǩ��`bool TryParse(InputArg, out T)`
- ����`T`����ʽת��Ϊ��Ӧ�Ĳ�������

�ÿ�����������parser��
- `ParsableParser<T>` : ����ʵ����`ISpanParsable<T>`������
- `EnumParser<T>` : ����enum����
- `BooleanFlagParser` : ����bool����Ϊ`Flag`����
- `BinaryFlagParser<T>` : ��bool���ͽ���Ϊָ��������ֵ
- `DelegateParser<T>` : ��װһ���������н���
- `DelegateFlagParser<T>` : ��װһ���������н���
- `Wrapped.NullableParser<T, TParser>` : �ṩ��`Nullable<T>`�����İ�װ
- `Wrapped.ConversionParser<T, TResult, TParser>`���ṩ��parse�����ת��

### ErrorHandler

Ĭ������£�ʹ��`ArgResultErrors.Builder.DefaultErrorHandler()`

�Զ���ErrorHandlerӦΪ�����ڲ���**����**������������Ҫ��
- ����ֵΪvoid�����ʽת��ΪExecution�ķ���ֵ
- ��һ������Ϊ`ArgResultErrors`���ͣ��ɱ��Ϊ`in`
- �ڶ���������ѡ��ӦΪ`string`���ͣ���ʾ���������`Executor`������
