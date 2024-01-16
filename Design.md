# Trarizon.TextCommand.Design

TODO:
- [ ] `Multi-Flag` for enum type
- [ ] Better parsing-exception design
- [ ] `Multi-Value` supports more types
- [ ] `Value.Order` (maybe?)

Diagnostic TODO:
- [ ] `Execution.CommandName` cannot contains white space
- [ ] `Executor.CommandPrefixes` cannot contains white space
- [ ] (warning) `Value` or `MultiValue` shouldn't appears after RestValue
- [ ] (warning) Parameters Not `Required` may be `default`
- [ ] (warning) `MultiValue.MaxCount` cannot <= 0, use non-param ctor for RestValue

��Ҫc#12���ϰ汾

## ���

### Execution

- ������Ψһ����
	- ��������Ϊ`string`��`Span<string>`��`ReadOnlySpan<string>`��`string[]`��`List<string>`��������ƥ������
		- ��ƥ������ָ����ʹ��`string`����ƥ�䣬����Ƭ���`IEnumerable<string>`���ͣ�������������֤
- [warning] �������Ϳ�`default`����Ϊδƥ��ʱ����`default`
- Execution�ϱ��`[Executor]`�ᱻ����
- CommandName���ܴ��пհ��ַ���

### Executor

- ����������Ҫ����ʽת����Execution�ķ�������
- ��ͬExecutor��Attribute���������ظ��򱻽ضϣ�����switch��������֧ƥ�䣩
- CommandPrefixes���ܴ��пհ��ַ���

### Parameter

һ������ֻ����һ���̳���`[CLParameter]`��attribute
- ������û��Ĭ��Parser���������ʽָ��Parser
    - Ĭ��Flag��������`bool`
	- Ĭ��Option��������`ISpanParsable<T>`, enum����
- Parser��Ϊ��ǰ���͵��ֶλ�����
- Parser����ʵ�ֶ�Ӧ�Ľӿ�
	- FlagParserʵ��`IArgFlagParser`
	- ����Parserʵ��`IArgParser`
- �������������ظ�
- [warning] RestValues֮��Ӧ����Value��Values����
- [warning] δ���Required�Ĳ�������Ϊ`default`

#### Values

- [warning] ��������<=0����ֵԤ����ΪRestValues
- ���ͱ���Ϊ`Span<>`,`ReadOnlySpan<>`,`T[]`,`List<>`,`IEnumerable<>`֮һ

## ִ��

- ������ָ�Ϊ`string[]`
	- `string`����ʹ��`StringInputMatcher`��`StringInputRest`ƥ��
	- `Span<string>`��`ReadOnlySpan<string>`����ֱ��ƥ��
	- `string[]`��`List<string>`ʹ��`AsSpan<string>()`����ƥ��
	- �����������ֱ��ƥ��
- ʹ���б�ģʽƥ������ǰ׺������ȡʣ��ֵ
- ��ʣ��ֵ��Ϊ��������������ָ��Executor
	- ͨ��`ParameterSet`��ȡ���������
	- ͨ��`ArgsProvider`��ȡ����ʵ��
- δƥ���򷵻�`default`
