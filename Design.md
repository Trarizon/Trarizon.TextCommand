# Trarizon.TextCommand.Design

��Ҫc#12���ϰ汾

## ���

### Execution

- ������Ψһ����
	- ��������Ϊ`string`��`ReadOnlySpan<char>` ��`Span<string>`��`ReadOnlySpan<string>`��`string[]`��`List<string>`��������ƥ������
		- ��ƥ������ָ����ʹ��`string`����ƥ�䣬����Ƭ���`IEnumerable<string>`���ͣ�������������֤
- [warning] �������Ϳ�`default`����Ϊδƥ��ʱ����`default`
- Execution�ϱ��`[Executor]`�ᱻ����
- CommandName���ܴ��пհ��ַ���ǰ׺`-`

#### ִ��

- ������ת��Ϊ��ƥ��`string`�б�
	- `string`����ʹ������`StringInputMatcher`��`StringInputRest`ƥ�䣬������Ƽ���`string`����
	- `Span<string>`��`ReadOnlySpan<string>`����ֱ��ƥ��
	- `string[]`��`List<string>`ʹ��`AsSpan<string>()`����ƥ��
	- �����������ֱ��ƥ�䣬������֤
- ʹ���б�ģʽƥ������ǰ׺������ȡʣ��ֵ
- ��ʣ��ֵ��Ϊ��������������ָ��Executor
	- ͨ��`ParameterSet`��ȡ���������
		- �����������
	- ͨ��`ArgsProvider`��ȡ����ʵ��
- δƥ���򷵻�`default`

### Executor

- ����������Ҫ����ʽת����Execution�ķ������ͣ�����Execution����`void`
- ��ͬExecutor��Attribute���������ظ��򱻽ضϣ�����switch��������֧ƥ�䣩
- CommandPrefixes���ܴ��пհ��ַ���
- һ���������Ա�Ƕ��`[Executor]`��ʾ��ͬ����ָ��ͬһ������

### Parameter

- һ������ֻ����һ���̳���`[CLParameter]`��attribute
- ������û��Ĭ��Parser���������ʽָ��Parser
	- `bool`��CLParameter����Ĭ��ParserΪ`BooleanFlagParser`��`ParsableParser<>`
	- `ISpanParsable`Ĭ��ParserΪ`ParsableParser<>`
	- `enum`����Ĭ��ParserΪ`EnumParser`
	- `Nullable<T> where T : ISpanParsable<T>`Ĭ��ParserΪ`NullableParser<ParsableParser<T>, T>`
- Parser��Ϊ��ǰ���͵��ֶΡ����Ի򷽷�
	- �ֶ�/����Parser��������ʵ��`IArgParser`��`IArgFlagParser`
	- ����Parserǩ����CLParameter����ƥ��`ArgParsingDelegate`��`ArgParsingDelegate`���������`DelegateParser<>`��`DelegateFlagParser<>`��װʹ��
- �������������ظ�
- [warning] RestValues֮��Ӧ����Value��Values����
- [warning] δ���Required�Ĳ�������Ϊ`default`

#### Flag

- `bool`Ĭ��Ϊ`Flag`��ʹ��`BooleanFlagParser`
- ��������ָ��Parserʵ��`IArgFlagParser<>`���򷽷�ǩ��ƥ��`ArgFlagParsingDelegate`

#### Option

- δָ����`bool`����Ĭ��Ϊ`Option`�������ṩĬ��parser
	- `ISpanParsable<>`ʹ��`ParsableParser`�����ж�`string`���⴦�����Ż�����
	- `enum`ʹ��`EnumParser`
	- `Nullable<T> where T:ISpanParsable<T>`ʹ��`NullableParser<ParsableParser<T>, T>`
- ��������ָ��Parserʵ��`IArgParser`����ǩ��ƥ��`ArgParsingDelegate`

#### Value

- Parserͬ[Option](#option)
 
#### Values

- [warning] ��������<=0����ֵԤ����ΪRestValues
- ���ͱ���Ϊ`Span<>`,`ReadOnlySpan<>`,`T[]`,`List<>`,`IEnumerable<>`֮һ
- Parserͬ[Value](#value)