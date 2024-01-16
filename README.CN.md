# Trarizon.TextCommand

���ڽ����������������ʽ��ʹ��Դ��������д��

���Ĺ���д���ˣ������޲���Ϸ�����

����ԭ�����������Bot�������ʽ����

## ����ʹ��

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
        [Values(5)] ReadOnlySpan<int> values)
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

ͨ�������ϵ�Attribute��Ա�����Զ�������������ԱΪ�����

### �淶

- `Execution`����Ϊ��������������Ϊ`string`, `Span<string>`, `ReadOnlySpan<string>`, `string[]`, `List<string>`
    - ������Ϊ`string`ʱ���Կհ��ַ����зָ�����

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
`Parser`|�����Ľ���������ϸ������[Parser�淶](#Parser�淶)
`Alias`|**���캯������**���������������õ���`-`ָ���Ĳ���
`Name`|��������Ĭ��ʹ�÷�������Ĳ�����
`Required`|Ĭ��`false`����Ϊ`true`�������ʱ�����ڸò��������쳣
`MaxCount`|**���캯������**��`MultiValue`�����Ĳ��������������������ʱ�������������`Value`����ʱ��ضϺ�������`Value`��`MultiValue`

### Parser�淶

�Զ���ParserӦΪ�����ڲ���**�ֶ�**��**����**

����`Flag`��parser��ʵ��`IArgFlagParser<T>`������������������ʵ��`IArgParser<T>`

�ÿ��ṩ������parser��ΪĬ����Ϊ��
- `ParsableParser<T>` : ����ʵ����`ISpanParsable<T>`������
- `EnumParser<T>` : ����enum����
- `BooleanFlagParser` : ����bool����Ϊ`Flag`����
- `BinaryFlagParser<T>` : ��bool���ͽ���Ϊָ��������ֵ
- `NullableParser<TParser, T>` : �ṩ��`Nullable<T>`�����İ�װ