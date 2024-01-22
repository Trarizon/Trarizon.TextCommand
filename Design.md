# Trarizon.TextCommand.Design

TODO:
- [ ] `Multi-Flag` for enum type
- [ ] Better parsing-exception design
- [ ] `Multi-Value` supports more types
- [ ] `Value.Order` (maybe?)
- [x] `Parser` supports method
- [x] (BUG): `Get<,>`�ĵ�һ������Ӧ��Ϊ��ȫ�޶���
- [x] (BUG): No implicit parser��option������������
- [x] Input supports `ReadOnlySpan<char>`, as `string`
- [ ] �ǿ�ƥ������ʱҪ������
- [ ] Error or support multiple execution

Diagnostic TODO:
- [x] `Execution.CommandName` cannot contains white space, cannot with `-` prefix
- [x] `Executor.CommandPrefixes` cannot contains white space, cannot with `-` prefix
- [x] (warning) `Value` or `MultiValue` shouldn't appears after RestValue
- [x] (warning) Parameters Not `Required` may be `default`
- [x] OptionKey cannot repeat
- [x] static Execution with non-static Executors

��Ҫc#12���ϰ汾

## ���

### Execution

- ������Ψһ����
	- ��������Ϊ`string`��`ReadOnlySpan<char>` ��`Span<string>`��`ReadOnlySpan<string>`��`string[]`��`List<string>`��������ƥ������
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
- Parser��Ϊ��ǰ���͵��ֶΡ����Ի򷽷�
	- �ֶ�/����Parser����ʵ�ֶ�Ӧ�Ľӿ�
		- ����Parserʵ�ֶ�Ӧ��`IArgParser`��`IArgFlagParser`
	- ����Parserʹ��`DelegateParser<>`��`DelegateFlagParser<>`��װʹ��
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
		- �����������
	- ͨ��`ArgsProvider`��ȡ����ʵ��
- δƥ���򷵻�`default`
